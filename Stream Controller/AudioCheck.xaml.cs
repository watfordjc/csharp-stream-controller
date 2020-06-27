using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudioWrapperLibrary;
using OBSWebSocketLibrary;
using Stream_Controller.SharedModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WebSocketLibrary;

namespace Stream_Controller
{
    /// <summary>
    /// Interaction logic for AudioCheck.xaml
    /// </summary>
    public partial class AudioCheck : Window
    {
        private readonly SynchronizationContext _Context;
        private static readonly AudioInterfaces audioInterfaces = AudioInterfaces.Instance;
        private readonly ObservableCollection<AudioInterface> devices = audioInterfaces.Devices;
        private readonly ObsWsClient webSocket;
        private bool audioDevicesEnumerate = false;
        private string connectionError = String.Empty;
        private WaveOutEvent silentAudioEvent = null;
        private static readonly Brush primaryBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xE2, 0xC1, 0xEA));
        private static readonly Brush secondaryBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xC5, 0xC0, 0xEB));
        private CancellationTokenSource pulseCancellationToken = new CancellationTokenSource();
        private readonly System.Timers.Timer _ReconnectCountdownTimer = new System.Timers.Timer(1000);
        private int _ReconnectTimeRemaining;
        private OBSWebSocketLibrary.Models.RequestReplies.GetCurrentScene currentScene;

        #region Instantiation and initialisation

        public AudioCheck()
        {
            InitializeComponent();
            _Context = SynchronizationContext.Current;
            Uri obs_uri = new UriBuilder(
                Preferences.Default.obs_uri_scheme,
                Preferences.Default.obs_uri_host,
                int.Parse(Preferences.Default.obs_uri_port)
                ).Uri;
            webSocket = new ObsWsClient(obs_uri)
            {
                AutoReconnect = Preferences.Default.obs_auto_reconnect
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateUIConnectStatus(null, null, null);
            devices.CollectionChanged += DeviceCollectionChanged;
            audioInterfaces.DeviceCollectionEnumerated += AudioDevicesEnumerated;
            audioInterfaces.DefaultDeviceChange += DefaultAudioDeviceChanged;
            webSocket.SetExponentialBackoff(Preferences.Default.obs_reconnect_min_seconds, Preferences.Default.obs_reconnect_max_minutes);
            webSocket.StateChange += WebSocket_StateChange;
            webSocket.ErrorState += WebSocket_Error;
            webSocket.OnObsEvent += WebSocket_Event;
            webSocket.OnObsReply += Websocket_Reply;
            _ReconnectCountdownTimer.Elapsed += ReconnectCountdownTimer_Elapsed;
            ObsWebsocketConnect();
        }

        #endregion

        #region Audio interfaces

        private void DeviceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateUIConnectStatus(null, null, null);
        }

        private void AudioDevicesEnumerated(object sender, bool e)
        {
            if (e)
            {
                audioDevicesEnumerate = true;
                UpdateUIConnectStatus(null, null, null);
                DisplayPortAudioWorkaround();
            }
        }

        private void DefaultAudioDeviceChanged(object sender, DataFlow dataFlow)
        {
            if (dataFlow == DataFlow.Render)
            {
                DisplayPortAudioWorkaround();
            }
        }

        private void DisplayPortAudioWorkaround()
        {
            if (audioDevicesEnumerate && audioInterfaces.DefaultRender.FriendlyName.Contains("NVIDIA") && silentAudioEvent?.PlaybackState != PlaybackState.Playing)
            {
                _ = Task.Run(
                    () => StartPlaySilence(audioInterfaces.DefaultRender)
                );
            }
            else if (audioDevicesEnumerate)
            {
                _ = Task.Run(
                    () => StopPlaySilence()
                    );
            }
        }

        private Task StartPlaySilence(AudioInterface audioInterface)
        {
            if (audioInterface.IsActive)
            {
                SilenceProvider provider = new SilenceProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
                silentAudioEvent = new WaveOutEvent()
                {
                    DeviceNumber = GetWaveOutDeviceNumber(audioInterface)
                };
                silentAudioEvent.Init(provider);
                silentAudioEvent.Play();
            }
            return Task.CompletedTask;
        }

        private Task StopPlaySilence()
        {
            if (silentAudioEvent?.PlaybackState == PlaybackState.Playing)
            {
                silentAudioEvent.Stop();
                silentAudioEvent.Dispose();
            }
            return Task.CompletedTask;
        }

        private int GetWaveOutDeviceNumber(AudioInterface audioInterface)
        {
            int deviceNameMaxLength = Math.Min(audioInterface.FriendlyName.Length, 31);
            string deviceNameTruncated = audioInterface.FriendlyName.Substring(0, deviceNameMaxLength);
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                if (WaveOut.GetCapabilities(i).ProductName == deviceNameTruncated)
                {
                    return i;
                }
            }
            return -1;
        }

        #endregion

        #region obs-websocket

        private async void ObsWebsocketConnect()
        {
            await webSocket.AutoReconnectConnectAsync();
        }

        private void ReconnectCountdownTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ReconnectTimeRemaining--;
            if (_ReconnectTimeRemaining > 0)
            {
                UpdateUIConnectStatus(_ReconnectTimeRemaining.ToString(), null, null);
            }
        }

        private void WebSocket_StateChange(object sender, WebSocketState newState)
        {
            if (newState == WebSocketState.Open)
            {
                connectionError = String.Empty;
                _ReconnectCountdownTimer.Stop();
                UpdateUIConnectStatus(String.Empty, Brushes.DarkGreen, null);
                Obs_GetCurrentScene();
            }
            else if (newState != WebSocketState.Connecting)
            {
                _ReconnectCountdownTimer.Start();
                UpdateUIConnectStatus(null, Brushes.Red, null);
            }
            else
            {
                connectionError = String.Empty;
                _ReconnectCountdownTimer.Stop();
                UpdateUIConnectStatus("\u2026", Brushes.DarkGoldenrod, null);
            }
        }

        private void WebSocket_Error(object sender, GenericClient.ErrorMessage e)
        {
            if (e.ReconnectDelay > 0)
            {
                UpdateUIConnectStatus(e.ReconnectDelay.ToString(), null, null);
                _ReconnectTimeRemaining = e.ReconnectDelay;
            }
            if (e.Error != null)
            {
                connectionError = $"{e.Error.Message}\n{e.Error.InnerException?.Message}";
            }
        }

        private void WebSocket_Event(object sender, ObsWsClient.ObsEvent eventObject)
        {
            switch (eventObject.EventType)
            {
                case OBSWebSocketLibrary.Data.Events.Heartbeat:
                    Heartbeat_Event((OBSWebSocketLibrary.Models.Events.Heartbeat)eventObject.MessageObject);
                    break;
                case OBSWebSocketLibrary.Data.Events.SwitchScenes:
                    SwitchScenes_Event((OBSWebSocketLibrary.Models.Events.SwitchScenes)eventObject.MessageObject);
                    break;
                case OBSWebSocketLibrary.Data.Events.ScenesChanged:
                    Obs_GetCurrentScene();
                    break;
                case OBSWebSocketLibrary.Data.Events.TransitionBegin:
                    string nextScene = ((OBSWebSocketLibrary.Models.Events.TransitionBegin)eventObject.MessageObject).ToScene;
                    UpdateTransitionMessage($"\u27a1\ufe0f {nextScene}\u2026");
                    break;
                case OBSWebSocketLibrary.Data.Events.TransitionEnd:
                case OBSWebSocketLibrary.Data.Events.TransitionVideoEnd:
                    UpdateTransitionMessage(String.Empty);
                    break;
            }
        }

        private void Websocket_Reply(object sender, ObsWsClient.ObsReply replyObject)
        {
            switch (replyObject.RequestType)
            {
                case OBSWebSocketLibrary.Data.Requests.GetCurrentScene:
                    currentScene = (OBSWebSocketLibrary.Models.RequestReplies.GetCurrentScene)replyObject.MessageObject;
                    UpdateSceneInformation();
                    break;
            }
        }

        #region obs-events

        private void Heartbeat_Event(OBSWebSocketLibrary.Models.Events.Heartbeat messageObject)
        {
            if (!pulseCancellationToken.IsCancellationRequested)
            {
                pulseCancellationToken.Cancel();
            }
            pulseCancellationToken = new CancellationTokenSource();
            try
            {
                UpdateUIConnectStatus(
                    null,
                    messageObject.Pulse ? primaryBrush : secondaryBrush,
                    Brushes.Gray);
            }
            catch (TaskCanceledException)
            {
                UpdateUIConnectStatus(null, Brushes.Gray, null);
            }
        }

        private void SwitchScenes_Event(OBSWebSocketLibrary.Models.Events.SwitchScenes messageObject)
        {
            currentScene.Name = messageObject.SceneName;
            currentScene.Sources = messageObject.Sources;
            UpdateSceneInformation();
        }

        #endregion

        #region obs-requests

        private Guid Obs_GetCurrentScene()
        {
            OBSWebSocketLibrary.Models.Requests.GetCurrentScene request = new OBSWebSocketLibrary.Models.Requests.GetCurrentScene();
            return webSocket.OBS_Send(request).Result;
        }

        #endregion

        #endregion

        #region User Interface

        private void UpdateUIConnectStatus(string countdownText, Brush brush1, Brush brush2)
        {
            Trace.WriteLine($"Start UI thread update: {DateTime.Now.Ticks}");
            _ = Task.Run(
                () => UpdateConnectStatus(countdownText, brush1, brush2)
                );
            Trace.WriteLine($"Finished UI thread update: {DateTime.Now.Ticks}");
        }

        private async Task UpdateConnectStatus(string countdownText, Brush brush1, Brush brush2)
        {
            _Context.Send(
                _ => tbAudioInterfaceStatus.Text = $"{devices.Count} audio devices",
                null);
            _Context.Send(
                _ => tbStatus.Text = connectionError,
                null);
            if (countdownText != null)
            {
                _Context.Send(
                    _ => tbReconnectCountdown.Text = countdownText,
                    null);
            }
            if (brush1 != null)
            {
                _Context.Send(
                    _ => sbCircleStatus.Fill = brush1,
                    null);
            }
            if (brush2 != null)
            {
                await Task.Delay(250, pulseCancellationToken.Token);
                _Context.Send(
                    _ => sbCircleStatus.Fill = brush2,
                    null);
            }
        }

        private Task UpdateSceneInformation()
        {
            if (Guid.Parse(currentScene?.MessageId) == Guid.Empty)
            {
                return Task.CompletedTask;
            }
            _Context.Send(
                x => tbActiveScene.Text = currentScene.Name,
                null);
            if (currentScene.Sources != null)
            {
                tbSourceList.Text = String.Empty;
                string[] sourceNames = currentScene.Sources.Select(a => a.Name).ToArray();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < sourceNames.Length; i++)
                {
                    sb.Append($"* {sourceNames[i]}");
                    if (i != sourceNames.Length - 1)
                    {
                        sb.Append("\n");
                    }
                }
                _Context.Send(
                    x => tbSourceList.Text = sb.ToString(),
                    null);
            }
            return Task.CompletedTask;
        }

        private Task UpdateTransitionMessage(string transitionMessage)
        {
            _Context.Send(
                x => tbTransitioning.Text = transitionMessage,
                null);
            return Task.CompletedTask;
        }

        #endregion
    }
}
