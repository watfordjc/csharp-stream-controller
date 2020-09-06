﻿using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
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
        private long logFileSize;
        private static readonly System.Text.RegularExpressions.Regex ircLogTweetRegex = new System.Text.RegularExpressions.Regex(@"^\[(.....)\] \<(.*)\>( \u0002\u000309,01 (.*) \u0003\u0002 \(\u0002(.*)\u0002\) (.*) )?.*\u0002\u000311,01 (.*) \u0003\u0002 \(\u0002(.*)\u0002\).?(.*)?:\u000305 (.*) \u0003\| (.*)$");
        private readonly SemaphoreSlim createMessageImageSemaphore = new SemaphoreSlim(1, 1);
        private readonly Queue<QueuedDisplayMessage> tweetImageQueue = new Queue<QueuedDisplayMessage>();
        private string previousTweetImage = String.Empty;
        private const int minimumTweetDisplayPeriods = 2;
        private const int maximumTweetDisplayPeriods = 4;
        private volatile int tweetDisplayPeriodsRemaining;
        private bool disposedValue;
        private readonly Windows.Media.SpeechSynthesis.SpeechSynthesizer speechSynthesizer = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
        private WaveOut waveOut;
        WaveFileReader waveFileReader;

        public volatile int countdownTimerValue;

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
                { "NHS", "-N H S-" },
                { "FRS", "-F R S-" },
                { "coronavirus", "corona virus" },
                { "999", "nine-nine-nine" },
                { "101", "one-oh-one" },
                { "555 111", "triple-five triple-one" },
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
            string today = DateTime.Now.ToString("yyyyMM", CultureInfo.InvariantCulture) + ((DateTime.Now.Day / 7) + 1).ToString("00", CultureInfo.InvariantCulture);
            DirectoryInfo dirInfo = new DirectoryInfo(@"G:\Program Files (x86)\mIRC\logs\");
            FileInfo[] fileNames = dirInfo.GetFiles($"#UK-Emergency-Advice.DALnet.{today}.log");
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
            string today = DateTime.Now.ToString("yyyyMM", CultureInfo.InvariantCulture) + ((DateTime.Now.Day / 7) + 1).ToString("00", CultureInfo.InvariantCulture);
            DirectoryInfo dirInfo = new DirectoryInfo(@"G:\Program Files (x86)\mIRC\logs\");
            FileInfo[] fileNames = dirInfo.GetFiles($"#UK-Emergency-Advice.DALnet.{today}.log");
            if (e.Name == fileNames[0].Name && Thread.VolatileRead(ref logFileSize) > 0)
            {
                using FileStream fileStream = File.Open(fileNames[0].FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                fileStream.Seek(Thread.VolatileRead(ref logFileSize), SeekOrigin.Begin);
                using StreamReader streamReader = new StreamReader(fileStream);
                while (streamReader.Peek() >= 0)
                {
                    string currentLine = streamReader.ReadLine();
                    ParseMessageFromIrcLog(currentLine).ConfigureAwait(false).GetAwaiter();
                }
                Thread.VolatileWrite(ref logFileSize, fileNames[0].Length);
            }
            else
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
            System.Text.RegularExpressions.Match match = ircLogTweetRegex.Match(message);

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

            string createdImage = null;

            try
            {
                createdImage = verticalMessagePanel.DrawVerticalTweet(profileImageFile, displayName, username, tweetText, time, retweeterDisplayName, retweeterUsername);
                Trace.WriteLine($"Tweet image saved to {createdImage}.");
            }
            catch (System.Runtime.InteropServices.COMException ce)
            {
                Trace.WriteLine($"Exception whilst processing Tweet with ID {tweetId} by {username} (retrying...) - {ce.Message} - {ce.InnerException?.Message}");
                try
                {
                    //verticalMessagePanel.Dispose();
                    //verticalMessagePanel = new VerticalMessagePanel();
                    createdImage = verticalMessagePanel.DrawVerticalTweet(profileImageFile, displayName, username, tweetText, time, retweeterDisplayName, retweeterUsername);
                }
                catch (System.Runtime.InteropServices.COMException ce2)
                {
                    Trace.WriteLine($"Exception whilst processing Tweet with ID {tweetId} by {username} (retry failed) - {ce2.Message} - {ce2.InnerException?.Message}");
                }
            }
            catch (AccessViolationException ave)
            {
                Trace.WriteLine($"Exception whilst processing Tweet with ID {tweetId} by {username} (retrying...) - {ave.Message}");
                try
                {
                    //verticalMessagePanel.Dispose();
                    //verticalMessagePanel = new VerticalMessagePanel();
                    createdImage = verticalMessagePanel.DrawVerticalTweet(profileImageFile, displayName, username, tweetText, time, retweeterDisplayName, retweeterUsername);
                }
                catch (AccessViolationException ave2)
                {
                    Trace.WriteLine($"Exception whilst processing Tweet with ID {tweetId} by {username} (retry failed) - {ave2.Message}");
                }
            }
            createMessageImageSemaphore.Release();

            if (createdImage != null)
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
            // Previous Tweet hasn't reached maximum display time yet - there are no more Tweets in the queue
            if (!String.IsNullOrEmpty(previousTweetImage) && (
                (tweetDisplayPeriodsRemaining > 0) ||
                (tweetImageQueue.Count == 0 && tweetDisplayPeriodsRemaining > finalTweetDisplayPeriod)
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
                Trace.WriteLine($"Dequeue at {e.Hour}:{e.Minute}:{e.Second} - {displayMessage.Filename}");
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
                    SpeakingRate = 1.0
                }
            };
            if (tweetImageQueue.Count > 5)
            {
                speechSynth.Options.SpeakingRate = 1.4;
            }

            string parsedSpeechString = displayMessage.SpeechPrepend + displayMessage.SpeechText;

            foreach (KeyValuePair<string, string> keyValuePair in speechReplacementDictionary)
            {
                parsedSpeechString = parsedSpeechString.Replace(keyValuePair.Key, keyValuePair.Value, StringComparison.InvariantCultureIgnoreCase);
            }

            parsedSpeechString = System.Text.RegularExpressions.Regex.Replace(parsedSpeechString, @"https?://[^ ]*", ", Earl. ", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(2));
            Windows.Media.SpeechSynthesis.SpeechSynthesisStream speechStream = await speechSynth.SynthesizeTextToStreamAsync(parsedSpeechString);

            AudioInterface audioInterface = AudioInterfaceCollection.Devices.Where(x => x.FriendlyName.Contains("CABLE", StringComparison.Ordinal) && x.DataFlow == NAudio.CoreAudioApi.DataFlow.Render).FirstOrDefault();
            waveOut = new WaveOut()
            {
                DeviceNumber = AudioWorkarounds.GetWaveOutDeviceNumber(audioInterface)
            };

            waveFileReader = new WaveFileReader(speechStream.AsStream());
            waveOut.Init(waveFileReader);

            // Add additional Tweet display time if necessary
            tweetDisplayPeriodsRemaining += Math.Max((int)Math.Ceiling(waveFileReader.TotalTime.TotalSeconds / Preferences.Default.obs_local_clock_cycle_delay) - minimumTweetDisplayPeriods, 0);
            Trace.WriteLine($"Expected playback length: {waveFileReader.TotalTime}. Number of Tweet display periods: {tweetDisplayPeriodsRemaining}.");
            countdownTimerValue = tweetDisplayPeriodsRemaining * Preferences.Default.obs_local_clock_cycle_delay;
            UpdateCountdownDisplay();

            waveOut.Play();
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
                    speechSynthesizer.Dispose();
                    waveFileReader.Dispose();
                    waveOut.Dispose();
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