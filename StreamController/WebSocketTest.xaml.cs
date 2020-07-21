using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
using uk.JohnCook.dotnet.OBSWebSocketLibrary;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.Data;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsEvents;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.TypeDefs;
using uk.JohnCook.dotnet.WebSocketLibrary;

namespace uk.JohnCook.dotnet.StreamController
{
    /// <summary>
    /// Interaction logic for WebSocketTest.xaml
    /// </summary>
    public partial class WebSocketTest : Window, IDisposable
    {
        private readonly ObsWsClient webSocket;
        private bool autoscroll = false;
        private int reconnectDelay;
        private bool disposedValue;
        private readonly int SCROLL_BUFFER_MAX_CHARS = 65000;
        private readonly SynchronizationContext _Context;

        public WebSocketTest()
        {
            InitializeComponent();
            _Context = SynchronizationContext.Current;
            InitialiseWindow();
            Uri obs_uri = new UriBuilder(
                Preferences.Default.obs_uri_scheme,
                Preferences.Default.obs_uri_host,
                int.Parse(Preferences.Default.obs_uri_port, CultureInfo.InvariantCulture)
                ).Uri;
            webSocket = new ObsWsClient(obs_uri)
            {
                AutoReconnect = Preferences.Default.obs_auto_reconnect
            };
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

        private async void Window_ContentRendered(object sender, EventArgs e)
        {
            webSocket.SetExponentialBackoff(Preferences.Default.obs_reconnect_min_seconds, Preferences.Default.obs_reconnect_max_minutes);
            webSocket.StateChange += WebSocket_StateChange_ContextSwitch;
            webSocket.StateChange += WebSocket_Connected_ContextSwitch;
            webSocket.ReceiveTextMessage += WebSocket_NewTextMessage_ContextSwitch;
            //webSocket.OnObsEvent += WebSocket_NewObsEvent_ContextSwitch;
            webSocket.ErrorState += WebSocket_ErrorMessage_ContextSwitch;
            if (Preferences.Default.obs_connect_launch)
            {
                btnTest.IsEnabled = false;
                await webSocket.AutoReconnectConnectAsync().ConfigureAwait(true);
            }
        }

        private void WebSocket_NewObsEvent_ContextSwitch(object sender, ObsEventObject e)
        {
            _Context.Send(
                x => WebSocket_NewObsEvent(e),
                null);
        }

        private void WebSocket_NewObsEvent(ObsEventObject obsEventObject)
        {
            EventBase obsEvent = obsEventObject.MessageObject as EventBase;
            switch (obsEventObject.EventType)
            {
                case ObsEventType.SourceFilterRemoved:
                    SourceFilterRemovedObsEvent temp1 = (obsEvent as SourceFilterRemovedObsEvent);
                    txtOutput.AppendText($"{temp1.UpdateType}: {temp1.FilterName} ({temp1.FilterType}) removed from {temp1.SourceName}\n");
                    break;
                case ObsEventType.SourceDestroyed:
                    SourceDestroyedObsEvent temp2 = (obsEvent as SourceDestroyedObsEvent);
                    txtOutput.AppendText($"{temp2.UpdateType}: {temp2.SourceName} ({temp2.SourceType})\n");
                    break;
                case ObsEventType.SourceCreated:
                    SourceCreatedObsEvent temp3 = (obsEvent as SourceCreatedObsEvent);
                    txtOutput.AppendText($"{temp3.UpdateType}: {temp3.SourceName} ({temp3.SourceKind})\n");
                    break;
                case ObsEventType.SourceVolumeChanged:
                    SourceVolumeChangedObsEvent temp4 = (obsEvent as SourceVolumeChangedObsEvent);
                    txtOutput.AppendText($"{temp4.UpdateType}: {temp4.SourceName}'s volume now at {temp4.Volume}\n");
                    break;
                case ObsEventType.SourceAudioSyncOffsetChanged:
                    SourceAudioSyncOffsetChangedObsEvent temp5 = (obsEvent as SourceAudioSyncOffsetChangedObsEvent);
                    txtOutput.AppendText($"{temp5.UpdateType}: {temp5.SourceName}'s audio offset is now {temp5.SyncOffset}\n");
                    break;
                case ObsEventType.SourceMuteStateChanged:
                    SourceMuteStateChangedObsEvent temp6 = (obsEvent as SourceMuteStateChangedObsEvent);
                    txtOutput.AppendText($"{temp6.UpdateType}: {temp6.SourceName} now has a muted state of {temp6.Muted}\n");
                    break;
                case ObsEventType.SourceFilterAdded:
                    SourceFilterAddedObsEvent temp7 = (obsEvent as SourceFilterAddedObsEvent);
                    txtOutput.AppendText($"{temp7.UpdateType}: {temp7.FilterName} ({temp7.FilterType}) added to {temp7.SourceName}\n");
                    break;
                case ObsEventType.SceneItemAdded:
                    SceneItemAddedObsEvent temp8 = (obsEvent as SceneItemAddedObsEvent);
                    txtOutput.AppendText($"{temp8.UpdateType}: {temp8.ItemName} added to {temp8.SceneName}\n");
                    break;
                case ObsEventType.SceneItemTransformChanged:
                    SceneItemTransformChangedObsEvent temp9 = (obsEvent as SceneItemTransformChangedObsEvent);
                    txtOutput.AppendText($"{temp9.UpdateType}: Transform changed on {temp9.ItemName} in {temp9.SceneName}\n");
                    break;
                case ObsEventType.SwitchScenes:
                    SwitchScenesObsEvent temp10 = (obsEvent as SwitchScenesObsEvent);
                    txtOutput.AppendText($"{temp10.UpdateType}: Switching to {temp10.SceneName}; {temp10.Sources.Count} sources in the scene.\n");
                    break;
                case ObsEventType.SceneCollectionChanged:
                    SceneCollectionChangedObsEvent temp11 = (obsEvent as SceneCollectionChangedObsEvent);
                    txtOutput.AppendText($"{temp11.UpdateType}: Scene collection has changed.\n");
                    break;
                case ObsEventType.TransitionListChanged:
                    TransitionListChangedObsEvent temp12 = (obsEvent as TransitionListChangedObsEvent);
                    txtOutput.AppendText($"{temp12.UpdateType}: Transition list has changed.\n");
                    break;
                case ObsEventType.SwitchTransition:
                    SwitchTransitionObsEvent temp13 = (obsEvent as SwitchTransitionObsEvent);
                    txtOutput.AppendText($"{temp13.UpdateType}: Using transition {temp13.TransitionName}\n");
                    break;
                case ObsEventType.ScenesChanged:
                    ScenesChangedObsEvent temp14 = (obsEvent as ScenesChangedObsEvent);
                    txtOutput.AppendText($"{temp14.UpdateType}: The scene list has been modified.\n");
                    break;
                default:
                    txtOutput.AppendText((obsEventObject.MessageObject as EventBase).UpdateType);
                    txtOutput.AppendText("\n");
                    break;
            }

            if (autoscroll == true)
            {
                svScroll.ScrollToBottom();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Preferences.Default.obs_width = this.Width;
            Preferences.Default.obs_height = this.Height;
            Preferences.Default.obs_left = this.Left;
            Preferences.Default.obs_top = this.Top;
            webSocket.StateChange -= WebSocket_StateChange_ContextSwitch;
            webSocket.StateChange -= WebSocket_Connected_ContextSwitch;
            webSocket.ReceiveTextMessage -= WebSocket_NewTextMessage_ContextSwitch;
            webSocket.ErrorState -= WebSocket_ErrorMessage_ContextSwitch;
            webSocket.AutoReconnect = false;
            webSocket.DisconnectAsync(true).ConfigureAwait(true);
        }

        private async void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            btnTest.IsEnabled = false;
            webSocket.AutoReconnect = Preferences.Default.obs_auto_reconnect;
            await webSocket.AutoReconnectConnectAsync().ConfigureAwait(true);
            e.Handled = true;
        }

        private void WebSocket_StateChange_ContextSwitch(object sender, WebSocketState e)
        {
            _Context.Send(
                x => WebSocket_StateChange(e),
                null);
        }

        private void WebSocket_StateChange(WebSocketState e)
        {
            txtStatus.Text = $"Connection Status: {e}";
            if (e == WebSocketState.Closed)
            {
                btnTest.IsEnabled = true;
            }
            else if (e == WebSocketState.None)
            {
                txtStatus.Text = webSocket.AutoReconnect
                    ? $"Connection Status: {e} (Current reconnect delay: {reconnectDelay} seconds)"
                    : $"Connection Status: {e}";
            }
        }

        private void WebSocket_ErrorMessage_ContextSwitch(object sender, WsClientErrorMessage errorMessage)
        {
            _Context.Send(
                x => WebSocket_ErrorMessage(errorMessage),
                null);
        }

        private void WebSocket_ErrorMessage(WsClientErrorMessage errorMessage)
        {
            if (errorMessage.Error == null) { return; }
            txtOutput.Text += $"{errorMessage.Error.Message}\n{errorMessage.Error.InnerException?.Message}\n\n";
            if (errorMessage.ReconnectDelay > 0)
            {
                reconnectDelay = errorMessage.ReconnectDelay;
            }
        }

        private void WebSocket_Connected_ContextSwitch(object sender, WebSocketState e)
        {
            _Context.Send(
                x => WebSocket_Connected(e),
                null);
        }

        private static void WebSocket_Connected(WebSocketState e)
        {
            if (e != WebSocketState.Open) { return; }
        }

        private void WebSocket_NewTextMessage_ContextSwitch(object sender, MemoryStream message)
        {
            _Context.Send(
                x => WebSocket_NewTextMessage(message),
                null);
        }

        private void WebSocket_NewTextMessage(MemoryStream message)
        {
            txtOutput.AppendText(Encoding.UTF8.GetString(message.ToArray()));
            txtOutput.AppendText("\n\n");
            if (txtOutput.Text.Length > SCROLL_BUFFER_MAX_CHARS)
            {
                txtOutput.Text = txtOutput.Text.AsSpan(txtOutput.Text.Length - SCROLL_BUFFER_MAX_CHARS).ToString();
            }
            if (autoscroll == true)
            {
                svScroll.ScrollToBottom();
            }
        }

        private async void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            webSocket.AutoReconnect = false;
            await webSocket.DisconnectAsync(true).ConfigureAwait(true);
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F4)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)
                    && (Keyboard.IsKeyDown(Key.LeftAlt)))
                {
                    App.Current.Shutdown();
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    webSocket.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~WebSocketTest()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
