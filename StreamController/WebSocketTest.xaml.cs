using Accessibility;
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
using uk.JohnCook.dotnet.StreamController.Controls;
using uk.JohnCook.dotnet.WebSocketLibrary;

namespace uk.JohnCook.dotnet.StreamController
{
    /// <summary>
    /// Interaction logic for WebSocketTest.xaml
    /// </summary>
    public partial class WebSocketTest : StyledWindow
    {
        private bool autoscroll = false;
        private int reconnectDelay;
        private readonly int SCROLL_BUFFER_MAX_CHARS = 65000;
        private readonly SynchronizationContext _Context;
        private readonly System.Timers.Timer scrollBufferTimer = new System.Timers.Timer(5000);

        public static readonly RoutedUICommand routedConnectionButtonCommand = new RoutedUICommand("ConnectionButtonCommand", "ConnectionButtonCommand", typeof(Button));

        public WebSocketTest()
        {
            InitializeComponent();
            _Context = SynchronizationContext.Current;
            InitialiseWindow();
        }

        private void InitialiseWindow()
        {
            this.Width = Preferences.Default.obs_width;
            this.Height = Preferences.Default.obs_height;
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = Preferences.Default.obs_left;
            this.Top = Preferences.Default.obs_top;
            this.cbAutoScroll.IsChecked = Preferences.Default.obs_autoscroll;
            scrollBufferTimer.Elapsed += ScrollBufferTimer_Elapsed;
            scrollBufferTimer.Start();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            ObsWebsocketConnection.Instance.Client.StateChange += WebSocket_StateChange_ContextSwitch;
            //ObsWebsocketConnection.Instance.Client.ReceiveTextMessage += WebSocket_NewTextMessage_ContextSwitch;
            ObsWebsocketConnection.Instance.Client.OnObsEvent += WebSocket_NewObsEvent_ContextSwitch;
            ObsWebsocketConnection.Instance.Client.ErrorState += WebSocket_ErrorMessage_ContextSwitch;
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
                    txtOutput.AppendText(String.Format(CultureInfo.CurrentCulture, Properties.Resources.window_obs_message_SourceFilterRemoved_format, temp1.UpdateType, temp1.FilterName, temp1.FilterType, temp1.SourceName));
                    break;
                case ObsEventType.SourceDestroyed:
                    SourceDestroyedObsEvent temp2 = (obsEvent as SourceDestroyedObsEvent);
                    txtOutput.AppendText(String.Format(CultureInfo.CurrentCulture, Properties.Resources.window_obs_message_generic_format, temp2.UpdateType, temp2.SourceName, temp2.SourceType));
                    break;
                case ObsEventType.SourceCreated:
                    SourceCreatedObsEvent temp3 = (obsEvent as SourceCreatedObsEvent);
                    txtOutput.AppendText(String.Format(CultureInfo.CurrentCulture, Properties.Resources.window_obs_message_generic_format, temp3.UpdateType, temp3.SourceName, temp3.SourceKind));
                    break;
                case ObsEventType.SourceVolumeChanged:
                    SourceVolumeChangedObsEvent temp4 = (obsEvent as SourceVolumeChangedObsEvent);
                    txtOutput.AppendText(String.Format(CultureInfo.CurrentCulture, Properties.Resources.window_obs_message_SourceVolumeChanged_format, temp4.UpdateType, temp4.SourceName, temp4.Volume));
                    break;
                case ObsEventType.SourceAudioSyncOffsetChanged:
                    SourceAudioSyncOffsetChangedObsEvent temp5 = (obsEvent as SourceAudioSyncOffsetChangedObsEvent);
                    txtOutput.AppendText(String.Format(CultureInfo.CurrentCulture, Properties.Resources.window_obs_message_SourceAudioSyncOffsetChanged_format, temp5.UpdateType, temp5.SourceName, temp5.SyncOffset));
                    break;
                case ObsEventType.SourceMuteStateChanged:
                    SourceMuteStateChangedObsEvent temp6 = (obsEvent as SourceMuteStateChangedObsEvent);
                    txtOutput.AppendText(String.Format(CultureInfo.CurrentCulture, Properties.Resources.window_obs_message_SourceMuteStateChanged_format, temp6.UpdateType, temp6.SourceName, temp6.Muted));
                    break;
                case ObsEventType.SourceFilterAdded:
                    SourceFilterAddedObsEvent temp7 = (obsEvent as SourceFilterAddedObsEvent);
                    txtOutput.AppendText(String.Format(CultureInfo.CurrentCulture, Properties.Resources.window_obs_message_SourceFilterAdded_format, temp7.UpdateType, temp7.FilterName, temp7.FilterType, temp7.SourceName));
                    break;
                case ObsEventType.SceneItemAdded:
                    SceneItemAddedObsEvent temp8 = (obsEvent as SceneItemAddedObsEvent);
                    txtOutput.AppendText(String.Format(CultureInfo.CurrentCulture, Properties.Resources.window_obs_message_SceneItemAdded_format, temp8.UpdateType, temp8.ItemName, temp8.SceneName));
                    break;
                case ObsEventType.SceneItemTransformChanged:
                    SceneItemTransformChangedObsEvent temp9 = (obsEvent as SceneItemTransformChangedObsEvent);
                    txtOutput.AppendText(String.Format(CultureInfo.CurrentCulture, Properties.Resources.window_obs_message_SceneItemTransformChanged_format, temp9.UpdateType, temp9.ItemName, temp9.SceneName));
                    break;
                case ObsEventType.SwitchScenes:
                    SwitchScenesObsEvent temp10 = (obsEvent as SwitchScenesObsEvent);
                    txtOutput.AppendText(String.Format(CultureInfo.CurrentCulture, Properties.Resources.window_obs_message_SwitchScenes_format, temp10.UpdateType, temp10.SceneName, temp10.Sources.Count));
                    break;
                case ObsEventType.SceneCollectionChanged:
                    SceneCollectionChangedObsEvent temp11 = (obsEvent as SceneCollectionChangedObsEvent);
                    txtOutput.AppendText(String.Format(CultureInfo.CurrentCulture, Properties.Resources.window_obs_message_SceneCollectionChanged_format, temp11.UpdateType));
                    break;
                case ObsEventType.TransitionListChanged:
                    TransitionListChangedObsEvent temp12 = (obsEvent as TransitionListChangedObsEvent);
                    txtOutput.AppendText(String.Format(CultureInfo.CurrentCulture, Properties.Resources.window_obs_message_TransitionListChanged_format, temp12.UpdateType));
                    break;
                case ObsEventType.SwitchTransition:
                    SwitchTransitionObsEvent temp13 = (obsEvent as SwitchTransitionObsEvent);
                    txtOutput.AppendText(String.Format(CultureInfo.CurrentCulture, Properties.Resources.window_obs_message_SwitchTransition_format, temp13.UpdateType, temp13.TransitionName));
                    break;
                case ObsEventType.ScenesChanged:
                    ScenesChangedObsEvent temp14 = (obsEvent as ScenesChangedObsEvent);
                    txtOutput.AppendText(String.Format(CultureInfo.CurrentCulture, Properties.Resources.window_obs_message_ScenesChanged_format, temp14.UpdateType));
                    break;
                default:
                    txtOutput.AppendText((obsEventObject.MessageObject as EventBase).UpdateType);
                    break;
            }
            txtOutput.AppendText("\n");

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
            ObsWebsocketConnection.Instance.Client.StateChange -= WebSocket_StateChange_ContextSwitch;
            ObsWebsocketConnection.Instance.Client.ReceiveTextMessage -= WebSocket_NewTextMessage_ContextSwitch;
            ObsWebsocketConnection.Instance.Client.OnObsEvent += WebSocket_NewObsEvent_ContextSwitch;
            ObsWebsocketConnection.Instance.Client.ErrorState -= WebSocket_ErrorMessage_ContextSwitch;
        }

        private void WebSocket_StateChange_ContextSwitch(object sender, WebSocketState e)
        {
            _Context.Send(
                x => WebSocket_StateChange(e),
                null);
        }

        private void WebSocket_StateChange(WebSocketState e)
        {
            txtStatus.Text = String.Format(CultureInfo.InvariantCulture, Properties.Resources.window_obs_connection_status_format, e);
            if (e == WebSocketState.Closed)
            {
                btnTest.IsEnabled = true;
            }
            else if (e == WebSocketState.None)
            {
                txtStatus.Text = ObsWebsocketConnection.Instance.Client.AutoReconnect
                    ? String.Format(CultureInfo.InvariantCulture, Properties.Resources.window_obs_connection_status_reconnect_format, e, reconnectDelay)
                    : String.Format(CultureInfo.InvariantCulture, Properties.Resources.window_obs_connection_status_format, e);
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
            if (autoscroll == true)
            {
                svScroll.ScrollToBottom();
            }
        }

        private void ScrollBufferTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (txtOutput.Text.Length > SCROLL_BUFFER_MAX_CHARS)
            {
                txtOutput.Text = txtOutput.Text.AsSpan(txtOutput.Text.Length - SCROLL_BUFFER_MAX_CHARS).ToString();
            }
            if (autoscroll == true)
            {
                svScroll.ScrollToBottom();
            }
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

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F4)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)
                    && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    App.Current.Shutdown();
                }
            }
            else if (e.Key == Key.R
                && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                await ObsWebsocketConnection.Instance.Reconnect().ConfigureAwait(true);
                e.Handled = true;
            }
            else if (e.Key == Key.D
                && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                await ObsWebsocketConnection.Instance.Disconnect().ConfigureAwait(true);
                e.Handled = true;
            }
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            switch ((e.OriginalSource as Button).Name)
            {
                case "btnTest":
                    e.CanExecute = ObsWebsocketConnection.Instance.Client.State != WebSocketState.Open;
                    break;
                case "btnClose":
                    e.CanExecute = ObsWebsocketConnection.Instance.Client.State == WebSocketState.Open;
                    break;
                default:
                    return;
            }
        }

        private async void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            switch ((e.OriginalSource as Button).Name)
            {
                case "btnTest":
                    await ObsWebsocketConnection.Instance.Reconnect().ConfigureAwait(true);
                    break;
                case "btnClose":
                    await ObsWebsocketConnection.Instance.Disconnect().ConfigureAwait(true);
                    break;
                default:
                    return;
            }
        }
    }
}
