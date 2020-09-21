using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using uk.JohnCook.dotnet.NAudioWrapperLibrary;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequests;

namespace uk.JohnCook.dotnet.StreamController
{
    public class TweetProcessing : IDisposable
    {
        #region Instantiation

        #region Properties and Variables

        private readonly SynchronizationContext _Context;
        private readonly ChronoTimer chronoTimer = ChronoTimer.Instance;
        private readonly VerticalMessagePanel verticalMessagePanel = new VerticalMessagePanel();
        private readonly FileSystemWatcher fileSystemWatcher;
        private readonly SemaphoreSlim logChangeCheckSemaphore = new SemaphoreSlim(1, 1);
        private volatile bool newDay;
        private long previousLogFileSize = -1;
        private long logFileSize = -1;
        private static readonly Regex ircLogTweetRegex = new Regex(@"^\[(.....)\] \<(.*)\>( \u0002\u000309,01 (.*) \u0003\u0002 \(\u0002(.*)\u0002\) (.*) )?.*\u0002\u000311,01 (.*) \u0003\u0002 \(\u0002(.*)\u0002\).?(.*)?:\u000305 (.*) \u0003\| (.*)$");
        private readonly SemaphoreSlim createMessageImageSemaphore = new SemaphoreSlim(1, 1);
        private readonly Queue<QueuedDisplayMessage> tweetImageQueue = new Queue<QueuedDisplayMessage>();
        private volatile string lastRenderedTweet = String.Empty;
        private QueuedDisplayMessage currentDisplayedTweet;
        private bool previousTweet;
        private const int minimumTweetDisplayPeriods = 2;
        private const int maximumTweetDisplayPeriods = 4;
        private volatile int tweetDisplayPeriodsRemaining;
        private bool disposedValue;
        private readonly SemaphoreSlim speechSemaphore = new SemaphoreSlim(1, 1);
        private readonly Windows.Media.SpeechSynthesis.SpeechSynthesizer speechSynthesizer = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
        private Stream speechStream;
        private WaveOut waveOut;
        WaveFileReader waveFileReader;
        private volatile int countdownTimerValue;

        public int CountdownTimerValue
        {
            get { return countdownTimerValue; }
            set { countdownTimerValue = value; }
        }

        private static readonly Dictionary<string, string> speechReplacementDictionary = new Dictionary<string, string>()
            {
                { "#", "hash-tag-" },
                { "hash-tag-stayhomesavelives", "hash-tag-stay-home-save-lithes" },
                { "hash-tag-clapforthe", "hash-tag-clap-for-the-" },
                { "hash-tag-uklockdown", "hash-tag-UK-lockdown" },
                { "hash-tag-oneteam", "hash-tag-one-team" },
                { "hash-tag-takeextracare", "hash-tag-take-extra-care" },
                { "hash-tag-mentalhealth", "hash-tag-mental-health" },
                { "hash-tag-domesticabuse", "hash-tag-domestic-abuse" },
                { "hash-tag-worldhealthday", "hash-tag-world-health-day" },
                { "hash-tag-helpushelpyou", "hash-tag-help-us-help-you" },
                { "NHS", "-N H S-" },
                { "FRS", "-F R S-" },
                { "coronavirus", "corona virus" },
                { "999", "nine-nine-nine" },
                { "101", "one-oh-one" },
                { "119", "one-one-nine" },
                { "555 111", "triple-five triple-one" },
                { "111", "one-one-one" },
                { "herts", "hearts" },
                { "carers", "-carers-" },
                { "lockdownuk", "lockdown-UK" },
                { "lives", "-lithes-" },
                { "stayathome", "-stay-at-home-" },
                { "govuk", "guv-UK"}
            };

        private class QueuedDisplayMessage
        {
            public bool IsTweet { get; set; }
            public bool IsRetweet { get; set; }
            public TwitterUser Tweeter { get; set; }
            public TwitterUser CurrentRetweeter { get; set; }
            public Queue<TwitterUser> Retweeters { get; set; } = new Queue<TwitterUser>();
            public string MessageText { get; set; }
            public string MessageId { get; set; }
            public string SpeechPrepend { get; set; }
            public string SpeechText { get; set; }
        }

        private class TwitterUser
        {
            public DateTime RelayTime { get; set; }
            public string DisplayName { get; set; }
            public string Username { get; set; }
            public string ProfileImageFile { get; set; }
        }

        #endregion

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public TweetProcessing()
        {
            _Context = SynchronizationContext.Current;
            fileSystemWatcher = new FileSystemWatcher()
            {
                Path = @"G:\Program Files (x86)\mIRC\logs\",
                NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.LastAccess,
                Filter = "*.log",
                IncludeSubdirectories = false
            };
            fileSystemWatcher.Changed += FileSystemWatcher_Changed;
            fileSystemWatcher.EnableRaisingEvents = true;
            Trace.WriteLine("Monitoring log changes...");
            FileSystemWatcher_Changed(this, new FileSystemEventArgs(WatcherChangeTypes.Changed, "", ""));
            chronoTimer.MinuteChanged += RefreshFileStats;
            chronoTimer.SecondChanged += RefreshFileStats;
            chronoTimer.MinuteChanged += ShowNextTweet;
            chronoTimer.SecondChanged += ShowNextTweet;
        }

        #endregion

        #region mIRC log monitoring and parsing

        /// <summary>
        /// Checks if a mIRC log file has changed
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">DateTime from a ChronoTimer</param>
        private void RefreshFileStats(object sender, DateTime e)
        {
            if (e.Hour == 0 && e.Minute == 0 && e.Second == 0)
            {
                newDay = true;
            }
            if (e.Second % 2 == 0 || logChangeCheckSemaphore.CurrentCount == 0) { return; }
            string today = DateTime.Now.ToString("yyyyMM", CultureInfo.InvariantCulture) + Math.Max(DateTime.Now.Day / 7 * 7, 1).ToString("00", CultureInfo.InvariantCulture);
            DirectoryInfo dirInfo = new DirectoryInfo(@"G:\Program Files (x86)\mIRC\logs\");
            FileInfo[] fileNames = dirInfo.GetFiles($"#UK-Emergency-Advice.DALnet.{today}.log");
            if (fileNames.Length == 0)
            {
                return;
            }
            using FileStream fileStream = File.Open(fileNames[0].FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fileStream.Seek(0, SeekOrigin.End);
            //if (fileStream.Position > Thread.VolatileRead(ref logFileSize))
            //{
            //    FileSystemWatcher_Changed(this, new FileSystemEventArgs(WatcherChangeTypes.Changed, "", ""));
            //}
        }

        /// <summary>
        /// Parses a mIRC log file for new relayed Tweets
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">FileSystemEventArgs from a FileSystemWatcher</param>
        private async void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (logChangeCheckSemaphore.CurrentCount == 0 || String.IsNullOrEmpty(e.Name))
            {
                return;
            }
            await logChangeCheckSemaphore.WaitAsync().ConfigureAwait(false);
            string today = DateTime.Now.ToString("yyyyMM", CultureInfo.InvariantCulture) + Math.Max(DateTime.Now.Day / 7 * 7, 1).ToString("00", CultureInfo.InvariantCulture);
            string todayFile = $"#UK-Emergency-Advice.DALnet.{today}.log";
            string yesterday = DateTime.Now.Subtract(TimeSpan.FromDays(1)).ToString("yyyyMM", CultureInfo.InvariantCulture) + Math.Max(DateTime.Now.Day / 7 * 7, 1).ToString("00", CultureInfo.InvariantCulture);
            string yesterdayFile = $"#UK-Emergency-Advice.DALnet.{yesterday}.log";
            // Cope with logfile rollovers
            if (newDay)
            {
                Thread.VolatileWrite(ref previousLogFileSize, Thread.VolatileRead(ref logFileSize));
                if (today != yesterday)
                {
                    Thread.VolatileWrite(ref logFileSize, 0);
                }
                newDay = false;
            }
            long tmpPreviousLogFileLength;
            DirectoryInfo dirInfo = new DirectoryInfo(@"G:\Program Files (x86)\mIRC\logs\");
            FileInfo[] fileNames;
            // Tweet is from yesterday
            if (today != yesterday && e.Name == yesterdayFile)
            {
                fileNames = dirInfo.GetFiles(yesterdayFile);
                tmpPreviousLogFileLength = Thread.VolatileRead(ref previousLogFileSize);
            }
            // Tweet is from today
            else
            {
                fileNames = dirInfo.GetFiles(todayFile);
                tmpPreviousLogFileLength = Thread.VolatileRead(ref logFileSize);
            }
            // Log file exists but this is our first run - set seek position
            if (fileNames.Length > 0 && tmpPreviousLogFileLength == -1 && e.Name == todayFile)
            {
                Thread.VolatileWrite(ref logFileSize, fileNames[0].Length);
                logChangeCheckSemaphore.Release();
                return;
            }
            // Log file doesn't exist
            else if (fileNames.Length == 0)
            {
                Trace.WriteLine("Unable to find mIRC log file.");
                logChangeCheckSemaphore.Release();
                return;
            }
            // Parse end of log file
            try
            {
                using FileStream fileStream = File.Open(fileNames[0].FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                long tmpNewLogFileLength = fileNames[0].Length;
                fileStream.Seek(tmpPreviousLogFileLength, SeekOrigin.Begin);
                using StreamReader streamReader = new StreamReader(fileStream);
                while (streamReader.Peek() >= 0)
                {
                    string currentLine = streamReader.ReadLine();
                    ParseMessageFromIrcLog(currentLine);
                }
                if (fileNames[0].Name == yesterdayFile && today != yesterday)
                {
                    // Update previous log file's read to state if Tweet is from yesterday and there has been a log file rollover
                    Thread.VolatileWrite(ref previousLogFileSize, streamReader.BaseStream.Position);
                }
                else
                {
                    // Update current log file's read to state if Tweet is from today
                    Thread.VolatileWrite(ref logFileSize, streamReader.BaseStream.Position);
                }
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is PathTooLongException || ex is DirectoryNotFoundException)
            {
                if (e.Name == todayFile)
                {
                    // Update current log file's read to state to start of file if file doesn't exist
                    Thread.VolatileWrite(ref logFileSize, 0);
                }
                logChangeCheckSemaphore.Release();
                Debug.Assert(fileNames.Length == 0, "File not found despite file existing?");
                return;
            }
            catch (IOException)
            {
                if (e.Name == todayFile)
                {
                    Thread.VolatileWrite(ref logFileSize, -1);
                }
                Thread.VolatileWrite(ref previousLogFileSize, -1);
                logChangeCheckSemaphore.Release();
                return;
            }
            logChangeCheckSemaphore.Release();
        }

        #endregion

        #region IRC-relayed Tweet parsing

        /// <summary>
        /// Get the path for a Twitter profile image, downloading if necessary
        /// </summary>
        /// <param name="username">Twitter username including @</param>
        /// <returns>Full file path for the image</returns>
        private static string FetchProfileImage(string username)
        {
            // Default Twitter profile image
            string fallbackProfileImage = @"G:\Program Files (x86)\mIRC\twitter_default.png";
            // One-dimensional array of folders containing profile images.
            // The last item in the array is where freshly downloaded images are expected to be saved.
            string[] profileImageDirs = {
                @"G:\Program Files (x86)\mIRC\twimg\",
                @"G:\Program Files (x86)\mIRC\twimg\tmp\"
            };
            DirectoryInfo userProfileImageDir = null;
            FileInfo[] userProfileImageFiles = null;

            foreach (string dir in profileImageDirs)
            {
                userProfileImageDir = new DirectoryInfo(dir);
                userProfileImageFiles = userProfileImageDir.GetFiles(username.Remove(0, 1) + ".*");
                if (userProfileImageFiles.Length > 0)
                {
                    return userProfileImageFiles[0].FullName;
                }
            }

            Trace.WriteLine($"Attempting to download profile image for Twitter user {username}.");

            TaskCompletionSource<int> returnCode = new TaskCompletionSource<int>();

            // Path to dash script and username parameter
            string commandArguments = $@"/home/thejc/Scripts/tweets/get-profile-image.sh {username.Remove(0, 1)}";
            // Path to WSL2 instance of Ubuntu
            string wslPath = @"C:\Users\John\AppData\Local\Microsoft\WindowsApps\ubuntu.exe";

            using Process getProfileImageProcess = new Process()
            {
                StartInfo = {
                    FileName = wslPath,
                    Arguments = $"-c {commandArguments}",
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };
            getProfileImageProcess.Exited += (sender, args) =>
            {
                returnCode.SetResult(getProfileImageProcess.ExitCode);
                getProfileImageProcess.Dispose();
            };

            getProfileImageProcess.Start();

            if (returnCode.Task.Result == 0)
            {
                userProfileImageFiles = userProfileImageDir.GetFiles(username.Remove(0, 1) + ".*");
                if (userProfileImageFiles.Length > 0)
                {
                    return userProfileImageFiles[0].FullName;
                }
                else
                {
                    return fallbackProfileImage;
                }
            }
            else
            {
                return fallbackProfileImage;
            }
        }

        /// <summary>
        /// Parses a mIRC logfile line into a QueuedDisplayMessage
        /// </summary>
        /// <param name="message">A single line from a mIRC log file</param>
        /// <returns>A Task</returns>
        private void ParseMessageFromIrcLog(string message)
        {
            DateTime today = DateTime.Now;
            Match match = ircLogTweetRegex.Match(message);

            bool isTweet = match.Groups[9].Value == "Tweeted";
            bool isRetweet = match.Groups[6].Value == "Retweeted";

            if ((!isTweet && !isRetweet) || match.Groups[2].Value != "UKEM-Bot")
            {
                return;
            }

            int year;
            int month;
            int day;
            int hour = int.Parse(match.Groups[1].Value.Substring(0, 2), CultureInfo.InvariantCulture);
            int minute = int.Parse(match.Groups[1].Value.Substring(3, 2), CultureInfo.InvariantCulture);
            if (today.Hour == 0 && today.Minute == 0 && hour == 23)
            {
                year = today.Subtract(TimeSpan.FromDays(1)).Year;
                month = today.Subtract(TimeSpan.FromDays(1)).Month;
                day = today.Subtract(TimeSpan.FromDays(1)).Day;
            }
            else
            {
                year = today.Year;
                month = today.Month;
                day = today.Day;
            }

            TwitterUser retweeter = null;
            if (isRetweet)
            {
                retweeter = new TwitterUser()
                {
                    RelayTime = new DateTime(year, month, day, hour, minute, 0, 0, DateTimeKind.Local),
                    ProfileImageFile = String.Empty,
                    DisplayName = match.Groups[4].Value,
                    Username = match.Groups[5].Value
                };
            }

            QueuedDisplayMessage displayMessage =
                currentDisplayedTweet?.MessageId == match.Groups[11].Value ?
                currentDisplayedTweet :
                tweetImageQueue.Where(x => x.MessageId == match.Groups[11].Value).FirstOrDefault();
            if (displayMessage == default)
            {
                displayMessage = new QueuedDisplayMessage()
                {
                    IsTweet = isTweet,
                    IsRetweet = isRetweet,
                    Tweeter = new TwitterUser()
                    {
                        RelayTime = new DateTime(year, month, day, hour, minute, 0, 0, DateTimeKind.Local),
                        DisplayName = match.Groups[7].Value,
                        Username = match.Groups[8].Value,
                        ProfileImageFile = FetchProfileImage(match.Groups[8].Value),
                    },
                    MessageText = match.Groups[10].Value,
                    MessageId = match.Groups[11].Value,
                    SpeechText = match.Groups[10].Value,
                };
                if (isRetweet)
                {
                    displayMessage.Retweeters.Enqueue(retweeter);
                    displayMessage.SpeechPrepend = $"{retweeter.DisplayName} Retweeted {displayMessage.Tweeter.DisplayName}: ";
                }
                else if (isTweet)
                {
                    displayMessage.SpeechPrepend = $"{displayMessage.Tweeter.DisplayName} Tweeted: ";
                }
                tweetImageQueue.Enqueue(displayMessage);
            }
            else
            {
                displayMessage.Retweeters.Enqueue(retweeter);
            }
            
        }

        #endregion

        #region Display a Tweet
        /// <summary>
        /// Parses a QueuedDisplayMessage and sends it for image creation and TTS treatment
        /// </summary>
        /// <param name="message">A QueuedDisplayMessage to display</param>
        /// <returns>A Task</returns>
        private bool DisplayTweet(QueuedDisplayMessage message, bool useTTS)
        {
            bool imageCreated;
            try
            {
                imageCreated = verticalMessagePanel.UpdateImage();
                if (useTTS)
                {
                    _Context.Send(
                        async x => await Speak(message).ConfigureAwait(false), null
                        );
                }
            }
            catch (COMException ce)
            {
                Trace.WriteLine($"Error creating image: {ce.Message} - {ce.HResult} - {ce.InnerException?.Message}");
                createMessageImageSemaphore.Release();
                Debugger.Break();
                return false;
            }
            Trace.WriteLine($"Tweet image{(imageCreated ? "" : " not")} displayed.");
            return true;
        }
        #endregion

        #region Tweet display

        /// <summary>
        /// Replaces currently displayed Tweet with the blank panel image
        /// </summary>
        /// <returns>A Task</returns>
        public void ClearTweet()
        {
            if (previousTweet)
            {
                if (verticalMessagePanel.ClearVerticalTweet())
                {
                    previousTweet = false;
                    currentDisplayedTweet = null;
                    lastRenderedTweet = String.Empty;
                }
            }
        }

        /// <summary>
        /// Tweet display timing code
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">DateTime from a ChronoTimer</param>
        private async void ShowNextTweet(object sender, DateTime e)
        {
            if (previousTweet)
            {
                countdownTimerValue--;
                UpdateCountdownDisplay();
            }
            //Trace.WriteLine($"Display periods remaining: {tweetDisplayPeriodsRemaining}, countdown value: {countdownTimerValue}, Tweet queue size: {tweetImageQueue.Count}");

            int clockCycleRemainder = e.Second % Preferences.Default.obs_local_clock_cycle_delay;
            int remainderPrecedingSecond = Preferences.Default.obs_local_clock_cycle_delay - 1;
            if (clockCycleRemainder > 0 && (clockCycleRemainder < remainderPrecedingSecond && Preferences.Default.obs_local_clock_cycle_delay > 1))
            {
                return;
            }

            int finalTweetDisplayPeriod = minimumTweetDisplayPeriods - maximumTweetDisplayPeriods;

            // The second preceding a new display period, reduce the display period number
            if (clockCycleRemainder == remainderPrecedingSecond)
            {
                // Previous Tweet hasn't reached end of minimum display time yet, or
                // Previous Tweet hasn't reached maximum display time yet - there are no more Tweets in the queue
                if (previousTweet && (
                    (tweetDisplayPeriodsRemaining > 0) ||
                    (tweetImageQueue.Count == 0 && tweetDisplayPeriodsRemaining > finalTweetDisplayPeriod)
                    ))
                {
                    tweetDisplayPeriodsRemaining--;
                }
                // Prepare display of next Tweet in queue
                if (tweetDisplayPeriodsRemaining == 0 && // We can prepare the next Tweet/Retweet
                    (tweetImageQueue.Count > 0 || currentDisplayedTweet?.Retweeters.Count > 0) || // There is a Tweet or Retweet to be displayed
                    (tweetDisplayPeriodsRemaining < 0 && // Sometimes we're late downloading a profile image
                    tweetImageQueue.Count > 0 && lastRenderedTweet != tweetImageQueue.Peek().MessageId // We haven't already processed the Tweet
                    ))
                {
                    QueuedDisplayMessage message = currentDisplayedTweet?.Retweeters.Count > 0 ? currentDisplayedTweet : tweetImageQueue.Peek();
                    TwitterUser tweeter = message.Tweeter;
                    TwitterUser retweeter = message.IsRetweet ? message.Retweeters.Peek() : null;
                    if (retweeter == null && message == currentDisplayedTweet && message.Retweeters.Count > 0)
                    {
                        retweeter = message.Retweeters.Peek();
                    }

                    DateTime today = e.AddSeconds(1);
                    // Display of a Tweet might straddle both sides of midnight
                    bool possiblyYesterday = today.Hour == 23 && today.Minute == 59;
                    // First minute of the day, queue might have Tweets from yesterday
                    bool fromYesterday = today.Hour == 0 && today.Minute == 0 && (tweeter.RelayTime.Hour == 23 || retweeter?.RelayTime.Hour == 23);

                    string day = fromYesterday ? "Yesterday" : "Today";
                    string time = message.IsRetweet ?
                        $"Retweeted {(possiblyYesterday ? retweeter.RelayTime.Hour.ToString("00", CultureInfo.InvariantCulture) + ":" + retweeter.RelayTime.Minute.ToString("00", CultureInfo.InvariantCulture) + " UTC+1" : day)}" :
                        $"{(possiblyYesterday ? "Before Midnight, " : $"{day}, ")}{tweeter.RelayTime.Hour.ToString("00", CultureInfo.InvariantCulture)}:{tweeter.RelayTime.Minute.ToString("00", CultureInfo.InvariantCulture)} UTC+1";
                    await createMessageImageSemaphore.WaitAsync().ConfigureAwait(false);
                    bool tweetRendered = verticalMessagePanel.DrawVerticalTweet(tweeter.ProfileImageFile, tweeter.DisplayName, tweeter.Username, message.MessageText, time, retweeter?.DisplayName, retweeter?.Username);
                    if (tweetRendered)
                    {
                        message.CurrentRetweeter = retweeter;
                        lastRenderedTweet = message.MessageId;
                    }
                    createMessageImageSemaphore.Release();
                }
                return;
            }

            // Previous Tweet hasn't reached end of minimum display time yet, or
            // Previous Tweet hasn't reached maximum display time yet - there are no more Tweets in the queue, or
            // TTS hasn't finished speaking yet
            if (previousTweet && (
                (tweetDisplayPeriodsRemaining > 0) ||
                (tweetImageQueue.Count == 0 && tweetDisplayPeriodsRemaining > finalTweetDisplayPeriod) ||
                speechSemaphore.CurrentCount == 0
                ))
            {
                return;
            }

            // Previous Tweet has reached maximum display time - there are no more Tweets in the queue
            else if (previousTweet && tweetImageQueue.Count == 0)
            {
                tweetDisplayPeriodsRemaining = 0;
                ClearTweet();
                countdownTimerValue = 0;
                UpdateCountdownDisplay();
                return;
            }

            // There are more Tweets in the queue and we're ready to show the next one
            else if (tweetImageQueue.Count > 0 && speechSemaphore.CurrentCount > 0 && (currentDisplayedTweet?.Retweeters.Count > 0 || tweetImageQueue.Peek().MessageId == lastRenderedTweet))
            {
                tweetDisplayPeriodsRemaining = minimumTweetDisplayPeriods;
                bool useTTS = true;
                if (tweetImageQueue.Peek().MessageId == lastRenderedTweet)
                {
                    currentDisplayedTweet = tweetImageQueue.Dequeue();
                }
                else if (currentDisplayedTweet?.Retweeters.Count > 0)
                {
                    currentDisplayedTweet.Retweeters.Dequeue();
                    tweetDisplayPeriodsRemaining = 1;
                    useTTS = false;
                }
                else
                {
                    return;
                }
                //QueuedDisplayMessage displayMessage = currentDisplayedTweet.Retweeters.Count > 0 ? currentDisplayedTweet : tweetImageQueue.Dequeue();

                Trace.WriteLine($"Dequeue at {e.Hour.ToString("00", CultureInfo.InvariantCulture)}:{e.Minute.ToString("00", CultureInfo.InvariantCulture)}:{e.Second.ToString("00", CultureInfo.InvariantCulture)} - Tweet ID: {currentDisplayedTweet.MessageId}");
                verticalMessagePanel.UpdateImage();
                bool imageCreated = DisplayTweet(currentDisplayedTweet, useTTS);
                // Unable to display next Tweet
                if (ObsWebsocketConnection.Instance.Client != null && !ObsWebsocketConnection.Instance.Client.CanSend)
                {
                    tweetDisplayPeriodsRemaining = 0;
                    countdownTimerValue = 0;
                    UpdateCountdownDisplay();
                    ClearTweet();
                    return;
                }
                else if (ObsWebsocketConnection.Instance.Client == null || !imageCreated)
                {
                    tweetDisplayPeriodsRemaining = 0;
                    countdownTimerValue = 0;
                    return;
                }
                previousTweet = true;
            }
        }

        public async void UpdateCountdownDisplay()
        {
            if (ObsWebsocketConnection.Instance.Client == null || !ObsWebsocketConnection.Instance.Client.CanSend)
            {
                return;
            }
            //string localDisplayTime = String.Format(CultureInfo.CurrentCulture, Properties.Resources.obs_time_display_format, e.ToLocalTime().ToString(Properties.Resources.obs_time_string_format, CultureInfo.InvariantCulture), TimezoneString);
            SetTextGDIPlusPropertiesRequestTextPropertyOnly request = new SetTextGDIPlusPropertiesRequestTextPropertyOnly()
            {
                Source = "Tweet Countdown",
                Text = countdownTimerValue.ToString(CultureInfo.InvariantCulture)
            };
            SetTextGDIPlusPropertiesRequestTextPropertyOnly request2 = new SetTextGDIPlusPropertiesRequestTextPropertyOnly()
            {
                Source = "Tweet Countdown 2",
                Text = countdownTimerValue.ToString(CultureInfo.InvariantCulture)
            };
            await ObsWebsocketConnection.Instance.Client.ObsSend(request).ConfigureAwait(false);
            await ObsWebsocketConnection.Instance.Client.ObsSend(request2).ConfigureAwait(false);
        }

        #endregion

        #region Tweet TTS

        /// <summary>
        /// Synthesise speech from a Tweet's text and play the audio
        /// </summary>
        /// <param name="displayMessage">The QueuedDisplayMessage being displayed</param>
        /// <returns>A Task</returns>
        private async Task Speak(QueuedDisplayMessage displayMessage)
        {
            using Windows.Media.SpeechSynthesis.SpeechSynthesizer speechSynth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer()
            {
                Options =
                {
                    IncludeSentenceBoundaryMetadata = true,
                    IncludeWordBoundaryMetadata = true,
                    AudioPitch = 1.0,
                    AudioVolume = 1.0,
                    SpeakingRate = 1.1
                }
            };
            if (tweetImageQueue.Count > 8)
            {
                speechSynth.Options.SpeakingRate = 1.35;
            }
            else if (tweetImageQueue.Count > 5)
            {
                speechSynth.Options.SpeakingRate = 1.3;
            }
            else if (tweetImageQueue.Count > 2)
            {
                speechSynth.Options.SpeakingRate = 1.2;
            }
            else if (tweetImageQueue.Count == 0)
            {
                speechSynth.Options.SpeakingRate = 1.0;
            }

            // Rudimentary Welsh language detection. No Welsh TTS language so avoid butchering it.
            if ((displayMessage.SpeechPrepend.Contains("wales", StringComparison.OrdinalIgnoreCase) && displayMessage.SpeechText.Contains(" yn", StringComparison.Ordinal)) ||
                displayMessage.SpeechText.Contains("gwnewch", StringComparison.OrdinalIgnoreCase) || // gwnewch = do
                Regex.Matches(displayMessage.SpeechText, " yn", RegexOptions.None).Count > 1 // yn is the Welsh language equivalent of using English suffix -ly to create adverbs from adjectives
                )
            {
                displayMessage.SpeechText = " Something in Welsh.";
            }

            string parsedSpeechString = displayMessage.SpeechPrepend + displayMessage.SpeechText;

            foreach (KeyValuePair<string, string> keyValuePair in speechReplacementDictionary)
            {
                parsedSpeechString = parsedSpeechString.Replace(keyValuePair.Key, keyValuePair.Value, StringComparison.InvariantCultureIgnoreCase);
            }

            parsedSpeechString = Regex.Replace(parsedSpeechString, @"https?://[^ ]*", ", Earl. ", RegexOptions.None, TimeSpan.FromSeconds(2));
            await speechSemaphore.WaitAsync().ConfigureAwait(false);
            Windows.Media.SpeechSynthesis.SpeechSynthesisStream asyncSpeechStream = await speechSynth.SynthesizeTextToStreamAsync(parsedSpeechString);
            speechStream = asyncSpeechStream.AsStream();

            // TODO: Add configuration option for TTS output interface name
            AudioInterface audioInterface = AudioInterfaceCollection.ActiveDevices.Where(x => x.FriendlyName.Contains("CABLE", StringComparison.Ordinal) && x.DataFlow == NAudio.CoreAudioApi.DataFlow.Render).FirstOrDefault();
            AudioInterface defaultRenderInterface = AudioInterfaceCollection.Instance.DefaultRender;
            // TODO: Add configuration option for monitor device and option for using default device if monitor device isn't the current default
            bool useDefaultRender = audioInterface == default || !audioInterface.IsActive || defaultRenderInterface == null || !defaultRenderInterface.FriendlyName.Contains("Headphones", StringComparison.Ordinal);
            waveOut = new WaveOut()
            {
                DeviceNumber = useDefaultRender ? -1 : AudioWorkarounds.GetWaveOutDeviceNumber(audioInterface)
            };
            waveOut.PlaybackStopped += WaveOut_PlaybackStopped;

            waveFileReader = new WaveFileReader(speechStream);
            waveOut.Init(waveFileReader);

            // Add additional Tweet display time if necessary
            tweetDisplayPeriodsRemaining += Math.Max((int)Math.Ceiling(waveFileReader.TotalTime.TotalSeconds / Preferences.Default.obs_local_clock_cycle_delay) - minimumTweetDisplayPeriods, 0);
            Trace.WriteLine($"Expected TTS playback length: {waveFileReader.TotalTime}. Number of Tweet display periods: {tweetDisplayPeriodsRemaining}.");
            countdownTimerValue = tweetDisplayPeriodsRemaining * Preferences.Default.obs_local_clock_cycle_delay;
            UpdateCountdownDisplay();

            waveOut.Play();
        }

        private void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            waveOut.PlaybackStopped -= WaveOut_PlaybackStopped;
            Trace.WriteLine("Playback stopped.");
            ClearSpeech();
            waveOut.Dispose();
            speechSemaphore.Release();
        }

        private void ClearSpeech()
        {
            if (speechStream != null)
            {
                speechStream.Dispose();
                speechStream = null;
            }
        }

        #endregion

        #region dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    verticalMessagePanel.Dispose();
                    logChangeCheckSemaphore.Dispose();
                    fileSystemWatcher.Dispose();
                    createMessageImageSemaphore.Dispose();
                    waveOut.Dispose();
                    waveFileReader.Dispose();
                    speechStream.Dispose();
                    speechSynthesizer.Dispose();
                    speechSemaphore.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TweetProcessing()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
