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
        private long logFileSize = -1;
        private static readonly Regex ircLogTweetRegex = new Regex(@"^\[(.....)\] \<(.*)\>( \u0002\u000309,01 (.*) \u0003\u0002 \(\u0002(.*)\u0002\) (.*) )?.*\u0002\u000311,01 (.*) \u0003\u0002 \(\u0002(.*)\u0002\).?(.*)?:\u000305 (.*) \u0003\| (.*)$");
        private readonly SemaphoreSlim createMessageImageSemaphore = new SemaphoreSlim(1, 1);
        private readonly Queue<QueuedDisplayMessage> tweetImageQueue = new Queue<QueuedDisplayMessage>();
        private string previousTweetImage = String.Empty;
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
            public string Filename { get; set; }
            public string SpeechPrepend { get; set; }
            public string SpeechText { get; set; }
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
            if (fileStream.Position > Thread.VolatileRead(ref logFileSize))
            {
                FileSystemWatcher_Changed(this, new FileSystemEventArgs(WatcherChangeTypes.Changed, "", ""));
            }
        }

        /// <summary>
        /// Parses a mIRC log file for new relayed Tweets
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">FileSystemEventArgs from a FileSystemWatcher</param>
        private async void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (logChangeCheckSemaphore.CurrentCount == 0)
            {
                return;
            }
            await logChangeCheckSemaphore.WaitAsync().ConfigureAwait(false);
            string today = DateTime.Now.ToString("yyyyMM", CultureInfo.InvariantCulture) + Math.Max(DateTime.Now.Day / 7 * 7, 1).ToString("00", CultureInfo.InvariantCulture);
            long tmpPreviousLogFileLength = Thread.VolatileRead(ref logFileSize);
            DirectoryInfo dirInfo = new DirectoryInfo(@"G:\Program Files (x86)\mIRC\logs\");
            FileInfo[] fileNames = dirInfo.GetFiles($"#UK-Emergency-Advice.DALnet.{today}.log");
            // Cope with logfile rollovers
            if (fileNames.Length == 0 || (tmpPreviousLogFileLength > -1 && e.Name != fileNames[0].Name && !String.IsNullOrEmpty(e.Name)))
            {
                fileNames = dirInfo.GetFiles(e.Name);
                if (fileNames.Length == 0)
                {
                    Trace.WriteLine("Unable to find mIRC log file.");
                    logChangeCheckSemaphore.Release();
                    return;
                }
            }
            // Log file exists and we have a previous position to start reading from
            if (fileNames.Length > 0 && e.Name == fileNames[0].Name && tmpPreviousLogFileLength >= 0)
            {
                try
                {
                    using FileStream fileStream = File.Open(fileNames[0].FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    Thread.VolatileWrite(ref logFileSize, fileNames[0].Length);
                    if (Thread.VolatileRead(ref logFileSize) < tmpPreviousLogFileLength)
                    {
                        tmpPreviousLogFileLength = 0;
                    }
                    fileStream.Seek(tmpPreviousLogFileLength, SeekOrigin.Begin);
                    using StreamReader streamReader = new StreamReader(fileStream);
                    while (streamReader.Peek() >= 0)
                    {
                        string currentLine = streamReader.ReadLine();
                        ParseMessageFromIrcLog(currentLine).ConfigureAwait(false).GetAwaiter();
                    }
                }
                catch (Exception ex) when (ex is FileNotFoundException || ex is PathTooLongException || ex is DirectoryNotFoundException)
                {
                    Thread.VolatileWrite(ref logFileSize, 0);
                    logChangeCheckSemaphore.Release();
                    Debug.Assert(fileNames.Length == 0, "File not found despite file existing?");
                    return;
                }
            }
            // Log file exists but this is our first run - set seek position
            else if (fileNames.Length > 0 && tmpPreviousLogFileLength == -1)
            {
                Thread.VolatileWrite(ref logFileSize, fileNames[0].Length);
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
        private async Task ParseMessageFromIrcLog(string message)
        {
            Match match = ircLogTweetRegex.Match(message);

            string relayTime = match.Groups[1].Value;
            string ircUser = match.Groups[2].Value;
            bool isTweet = match.Groups[9].Value == "Tweeted";
            bool isRetweet = match.Groups[6].Value == "Retweeted";
            if ((!isTweet && !isRetweet) || ircUser != "UKEM-Bot")
            {
                return;
            }
            string displayName = match.Groups[7].Value;
            string username = match.Groups[8].Value;
            string retweeterDisplayName = isRetweet ? match.Groups[4].Value : null;
            string retweeterUsername = isRetweet ? match.Groups[5].Value : null;
            string tweetText = match.Groups[10].Value;
            string tweetId = match.Groups[11].Value;

            //Trace.WriteLine($"time: {relayTime}, luser: {ircUser}, isRetweet: {isRetweet} ({retweeterDisplayName}({retweeterUsername})), {displayName} ({username}) Tweeted ({tweetId}) {tweetText}");

            string time = isRetweet ? $"Retweeted Today" : $"Today, {relayTime} UTC+1";
            string profileImageFile = FetchProfileImage(username);

            await createMessageImageSemaphore.WaitAsync().ConfigureAwait(false);
            string createdImage;
            try
            {
                createdImage = verticalMessagePanel.DrawVerticalTweet(profileImageFile, displayName, username, tweetText, time, retweeterDisplayName, retweeterUsername);
            }
            catch (COMException ce)
            {
                Trace.WriteLine($"Error creating image: {ce.Message} - {ce.HResult} - {ce.InnerException?.Message}");
                createMessageImageSemaphore.Release();
                await Task.FromException(ce).ConfigureAwait(true);
                Debugger.Break();
                return;
            }
            Trace.WriteLine($"Tweet image saved to {createdImage}.");
            createMessageImageSemaphore.Release();

            if (!String.IsNullOrEmpty(createdImage))
            {
                QueuedDisplayMessage displayMessage = new QueuedDisplayMessage()
                {
                    Filename = createdImage,
                    SpeechPrepend = isRetweet ? $"{retweeterDisplayName} Retweeted {displayName}: " : $"{displayName} Tweeted: ",
                    SpeechText = tweetText
                };
                tweetImageQueue.Enqueue(displayMessage);
            }
        }

        #endregion

        #region Tweet display

        /// <summary>
        /// Replaces currently displayed Tweet with the blank panel image
        /// </summary>
        /// <returns>A Task</returns>
        public async Task ClearTweet()
        {
            if (!string.IsNullOrEmpty(previousTweetImage))
            {
                File.Delete(previousTweetImage);
                previousTweetImage = String.Empty;
            }
            OBSWebSocketLibrary.TypeDefs.ImageSource imageSource = new OBSWebSocketLibrary.TypeDefs.ImageSource()
            {
                File = verticalMessagePanel.blankPanel
            };
            SetSourceSettingsRequest request = new SetSourceSettingsRequest()
            {
                SourceName = "TextFormatterOutput",
                SourceSettings = imageSource
            };
            await ObsWebsocketConnection.Instance.Client.ObsSend(request).ConfigureAwait(false);
        }

        /// <summary>
        /// Tweet display timing code
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">DateTime from a ChronoTimer</param>
        private async void ShowNextTweet(object sender, DateTime e)
        {
            if (!string.IsNullOrEmpty(previousTweetImage))
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
                if (!String.IsNullOrEmpty(previousTweetImage) && (
                    (tweetDisplayPeriodsRemaining > 0) ||
                    (tweetImageQueue.Count == 0 && tweetDisplayPeriodsRemaining > finalTweetDisplayPeriod)
                    ))
                {
                    tweetDisplayPeriodsRemaining--;
                }
                return;
            }

            // Previous Tweet hasn't reached end of minimum display time yet, or
            // Previous Tweet hasn't reached maximum display time yet - there are no more Tweets in the queue, or
            // TTS hasn't finished speaking yet
            if (!String.IsNullOrEmpty(previousTweetImage) && (
                (tweetDisplayPeriodsRemaining > 0) ||
                (tweetImageQueue.Count == 0 && tweetDisplayPeriodsRemaining > finalTweetDisplayPeriod) ||
                speechSemaphore.CurrentCount == 0
                ))
            {
                return;
            }

            // Previous Tweet has reached maximum display time - there are no more Tweets in the queue
            else if (!String.IsNullOrEmpty(previousTweetImage) && tweetImageQueue.Count == 0)
            {
                tweetDisplayPeriodsRemaining = 0;
                await ClearTweet().ConfigureAwait(false);
                countdownTimerValue = 0;
                UpdateCountdownDisplay();
                return;
            }

            // There are more Tweets in the queue and we're ready to show the next one
            else if (tweetImageQueue.Count > 0)
            {
                tweetDisplayPeriodsRemaining = minimumTweetDisplayPeriods;
                QueuedDisplayMessage displayMessage = tweetImageQueue.Dequeue();
                Trace.WriteLine($"Dequeue at {e.Hour.ToString("00", CultureInfo.InvariantCulture)}:{e.Minute.ToString("00", CultureInfo.InvariantCulture)}:{e.Second.ToString("00", CultureInfo.InvariantCulture)} - {displayMessage.Filename}");
                // Unable to display next Tweet
                if (ObsWebsocketConnection.Instance.Client != null && (!(ObsWebsocketConnection.Instance.Client.CanSend) || String.IsNullOrEmpty(displayMessage.Filename)))
                {
                    tweetDisplayPeriodsRemaining = 0;
                    countdownTimerValue = 0;
                    UpdateCountdownDisplay();
                    await ClearTweet().ConfigureAwait(false);
                    if (File.Exists(displayMessage.Filename))
                    {
                        File.Delete(displayMessage.Filename);
                    }
                    return;
                }
                else if (ObsWebsocketConnection.Instance.Client == null)
                {
                    tweetDisplayPeriodsRemaining = 0;
                    countdownTimerValue = 0;
                    if (File.Exists(displayMessage.Filename))
                    {
                        File.Delete(displayMessage.Filename);
                    }
                    return;
                }
                OBSWebSocketLibrary.TypeDefs.ImageSource imageSource = new OBSWebSocketLibrary.TypeDefs.ImageSource()
                {
                    File = displayMessage.Filename
                };
                SetSourceSettingsRequest request = new SetSourceSettingsRequest()
                {
                    SourceName = "TextFormatterOutput",
                    SourceSettings = imageSource
                };
                await ObsWebsocketConnection.Instance.Client.ObsSend(request).ConfigureAwait(false);
                _Context.Send(
                    async x => await Speak(displayMessage).ConfigureAwait(true), null
                    );
                if (!string.IsNullOrEmpty(previousTweetImage))
                {
                    File.Delete(previousTweetImage);
                }
                previousTweetImage = displayMessage.Filename;
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
            await ObsWebsocketConnection.Instance.Client.ObsSend(request).ConfigureAwait(true);
            await ObsWebsocketConnection.Instance.Client.ObsSend(request2).ConfigureAwait(true);
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
            await speechSemaphore.WaitAsync().ConfigureAwait(true);
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
            Trace.WriteLine($"Expected playback length: {waveFileReader.TotalTime}. Number of Tweet display periods: {tweetDisplayPeriodsRemaining}.");
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
