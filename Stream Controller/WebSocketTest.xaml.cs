using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Converters;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OBSWebSocketLibrary;

namespace Stream_Controller
{
    /// <summary>
    /// Interaction logic for WebSocketTest.xaml
    /// </summary>
    public partial class WebSocketTest : Window, IDisposable
    {
        private readonly ObsWsClient webSocket;
        private bool autoscroll = false;
        private readonly System.Timers.Timer heartBeatCheck = new System.Timers.Timer(8000);
        private bool disposedValue;
        private int reconnectDelay;
        private readonly Dictionary<Guid, string> sentMessageGuids = new Dictionary<Guid, string>();

        public WebSocketTest()
        {
            InitializeComponent();
            InitialiseWindow();
            Uri obs_uri = new UriBuilder(
                Preferences.Default.obs_uri_scheme,
                Preferences.Default.obs_uri_host,
                int.Parse(Preferences.Default.obs_uri_port)
                ).Uri;
            webSocket = new ObsWsClient(obs_uri);
        }

        private void InitialiseWindow()
        {
            this.Width = Preferences.Default.obs_width;
            this.Height = Preferences.Default.obs_height;
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = Preferences.Default.obs_left;
            this.Top = Preferences.Default.obs_top;
            this.cbAutoScroll.IsChecked = Preferences.Default.obs_autoscroll;
        }

        private void MenuItemPreferences_Click(object sender, RoutedEventArgs e)
        {
            WindowUtilityLibrary.MakeWindowActive(WindowUtilityLibrary.PREFERENCES);
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItemAudioInterfaces_Click(object sender, RoutedEventArgs e)
        {
            WindowUtilityLibrary.MakeWindowActive(WindowUtilityLibrary.MAIN_WINDOW);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Preferences.Default.obs_width = this.Width;
            Preferences.Default.obs_height = this.Height;
            Preferences.Default.obs_left = this.Left;
            Preferences.Default.obs_top = this.Top;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            webSocket.SetExponentialBackoff(Preferences.Default.obs_reconnect_min_seconds, Preferences.Default.obs_reconnect_max_minutes);
            webSocket.StateChange += WebSocket_StateChange;
            webSocket.StateChange += WebSocket_Connected;
            webSocket.ReceiveTextMessage += WebSocket_NewTextMessage;
            webSocket.ErrorState += WebSocket_ErrorMessage;
            heartBeatCheck.Elapsed += HeartBeatTimer_Elapsed;
            if (Preferences.Default.obs_connect_launch)
            {
                btnTest.IsEnabled = false;
                await webSocket.AutoReconnectConnectAsync();
            }
        }

        private async void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            btnTest.IsEnabled = false;
            await webSocket.AutoReconnectConnectAsync();
            e.Handled = true;
        }

        private void WebSocket_StateChange(object sender, WebSocketState e)
        {
            txtStatus.Text = $"Connection Status: {e}";
            if (e == WebSocketState.Closed)
            {
                btnTest.IsEnabled = true;
            }
            else if (e == WebSocketState.None)
            {
                txtStatus.Text = $"Connection Status: {e} (Current reconnect delay: {reconnectDelay} seconds)";
            }
        }

        private void WebSocket_ErrorMessage(object sender, ObsWsClient.ErrorMessage errorMessage)
        {
            if (errorMessage.Error == null)
            {
                return;
            }
            txtOutput.Text += $"{errorMessage.Error.Message}\n{errorMessage.Error.InnerException.Message}\n\n";
            if (errorMessage.ReconnectDelay > 0)
            {
                reconnectDelay = errorMessage.ReconnectDelay;
            }
        }

        private async void OBS_EnableHeartBeat()
        {
            Dictionary<string, object> jsonDictionary = new Dictionary<string, object>
            {
                { "request-type", "SetHeartbeat" },
                { "enable", true }
            };
            await OBS_Send(jsonDictionary);
        }

        private void WebSocket_Connected(object sender, WebSocketState e)
        {
            if (e != WebSocketState.Open)
            {
                return;
            }

            webSocket.StartMessageReceiveLoop();
            OBS_EnableHeartBeat();
            OBS_GetSourcesList();

            /*
             * TODO: OBS sourceType to sourceSettings.audio_device_id represented as NAudio property
             * 
             * browser_source -> N/A
             * dshow_input -> Device.FriendlyName + ":"
             * ffmpeg_source -> N/A
             * game_capture -> N/A
             * waspi_input_capture -> Device.ID
             * waspi_output_capture -> Device.ID
             * 
            */
        }

        private async Task<Guid> OBS_Send(Dictionary<string, object> jsonDictionary)
        {
            Guid guid = Guid.NewGuid();
            jsonDictionary.TryGetValue("request-type", out object requestType);
            sentMessageGuids.Add(guid, requestType.ToString());
            jsonDictionary.Add("message-id", guid.ToString());
            await webSocket.SendMessageAsync(JsonSerializer.Serialize(jsonDictionary));
            return guid;
        }

        private Guid OBS_GetSourcesList()
        {
            Dictionary<string, object> jsonDictionary = new Dictionary<string, object>
            {
                { "request-type", "GetSourcesList" }
            };
            return OBS_Send(jsonDictionary).Result;
        }

        private Guid OBS_GetSourceSettings(string sourceName)
        {
            Dictionary<string, object> jsonDictionary = new Dictionary<string, object>
            {
                { "request-type", "GetSourceSettings" },
                { "sourceName", sourceName }
            };
            return OBS_Send(jsonDictionary).Result;
        }

        private void WebSocket_NewTextMessage(object sender, string message)
        {
            Application.Current.Dispatcher.Invoke(
                () => txtOutput.Text += $"{message}\n\n"
                );
            if (autoscroll == true)
            {
                svScroll.ScrollToBottom();
            }

            OBS_ParseJson(message);
        }

        private void OBS_ParseJson(string message)
        {
            // TODO: Move JSON Handling

            using JsonDocument document = JsonDocument.Parse(message);
            JsonElement root = document.RootElement;
            bool hasGuid = root.TryGetProperty("message-id", out JsonElement requestTypeJson);
            bool hasUpdateType = root.TryGetProperty("update-type", out JsonElement jsonUpdateType);
            // Handle responses to sent messages
            if (hasGuid)
            {
                Guid guid = Guid.Parse(requestTypeJson.GetString());
                if (sentMessageGuids.TryGetValue(guid, out string requestType))
                {
                    Trace.WriteLine($"Received response to message of type {requestType} with GUID {guid}.");
                    sentMessageGuids.Remove(guid);
                }
                switch (requestType)
                {
                    case "SetHeartbeat":
                        root.TryGetProperty("status", out JsonElement status);
                        Trace.WriteLine($"Server response to enabling HeartBeat: {status.GetString()}");
                        break;
                    case "GetSourcesList":
                        OBSWebSocketLibrary.Models.GetSourcesListReply reply = JsonSerializer.Deserialize<OBSWebSocketLibrary.Models.GetSourcesListReply>(message);
                        foreach (OBSWebSocketLibrary.Models.Source device in reply.Sources)
                        {
                            OBS_GetSourceSettings(device.Name);
                        }
                        break;
                }
                return;
            }
            else if (!hasUpdateType)
            {
                Trace.WriteLine("Unexpected JSON.");
                return;
            }
            // Handle responses with an update-type

            Trace.WriteLine($"Received a message of type {jsonUpdateType}.");
            bool isStreaming = root.TryGetProperty("stream-timecode", out JsonElement jsonStreamTimecode);
            bool isRecording = root.TryGetProperty("rec-timecode", out JsonElement jsonRecTimecode);

            switch (jsonUpdateType.GetString())
            {
                case "Heartbeat":
                    root.TryGetProperty("pulse", out JsonElement pulse);
                    heartBeatCheck.Enabled = pulse.GetBoolean();
                    heartBeatCheck.Enabled = true;
                    break;
            }
        }

        private void HeartBeatTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            heartBeatCheck.Stop();
            if (Preferences.Default.obs_auto_reconnect)
            {
                Application.Current.Dispatcher.Invoke(
                    async () => await webSocket.ReconnectAsync()
                    );
            }
            else
            {
                Application.Current.Dispatcher.Invoke(
                    async () => await webSocket.DisconnectAsync()
                    );
            }
        }

        private async void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            await webSocket.DisconnectAsync();
            e.Handled = true;
        }

        private void AutoScroll_Checked(object sender, RoutedEventArgs e)
        {
            autoscroll = true;
            svScroll.ScrollToBottom();
        }

        private void AutoScroll_Unchecked(object sender, RoutedEventArgs e)
        {
            autoscroll = false;
        }

        private void TxtOutput_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (svScroll.VerticalOffset < svScroll.ScrollableHeight)
            {
                cbAutoScroll.IsChecked = false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    heartBeatCheck.Dispose();
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~WebSocketTest()
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
    }
}
