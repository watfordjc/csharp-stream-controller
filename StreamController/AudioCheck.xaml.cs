using NAudio.CoreAudioApi;
using NAudio.Wave;
using uk.JohnCook.dotnet.NAudioWrapperLibrary;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.TypeDefs;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequests;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.Data;
using uk.JohnCook.dotnet.WebSocketLibrary;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Automation.Peers;
using uk.JohnCook.dotnet.StreamController.Controls;

namespace uk.JohnCook.dotnet.StreamController
{
    /// <summary>
    /// Interaction logic for AudioCheck.xaml
    /// </summary>
    public partial class AudioCheck : StyledWindow
    {
        private readonly SynchronizationContext _Context;

        #region Instantiation and initialisation

        public AudioCheck()
        {
            InitializeComponent();
            _Context = SynchronizationContext.Current;
        }

        private async void Window_ContentRendered(object sender, EventArgs e)
        {
            SystemTrayIcon.Instance.UpdateTrayIcon().ConfigureAwait(true).GetAwaiter();
            if (ObsWebsocketConnection.Instance.Client == null)
            {
                ObsWebsocketConnection.CreateClient();
                ObsWebsocketConnection.Instance.Client.ErrorState += WebSocket_Error_ContextSwitch;
                ObsWebsocketConnection.Instance.PropertyChanged += Instance_PropertyChanged;
                if (Preferences.Default.obs_connect_launch)
                {
                    await ObsWebsocketConnection.Instance.Connect().ConfigureAwait(true);
                }
                else
                {
                    await ObsWebsocketConnection.Instance.ChangeStatusColor(Brushes.Gray, false).ConfigureAwait(true);

                }
            }
            else
            {
                ObsWebsocketConnection.Instance.Client.ErrorState += WebSocket_Error_ContextSwitch;
                ObsWebsocketConnection.Instance.PropertyChanged += Instance_PropertyChanged;
            }
        }

        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ObsWebsocketConnection.ConnectionStatus):
                    switch (ObsWebsocketConnection.Instance.Client.State)
                    {
                        case WebSocketState.Open:
                        case WebSocketState.Connecting:
                        case WebSocketState.Closed:
                            TextBlock_AnnounceChanged(tbReconnectCountdown);
                            break;
                        default:
                            break;
                    }
                    break;
                case nameof(ObsWebsocketConnection.NextScene):
                    if (!string.IsNullOrEmpty(ObsWebsocketConnection.Instance.NextScene))
                    {
                        TextBlock_AnnounceChanged(tbTransitioning);
                    }
                    break;
                case nameof(ObsWebsocketConnection.CurrentScene):
                    if (ObsWebsocketConnection.Instance.CurrentScene != null)
                    {
                        TextBlock_AnnounceChanged(current_scene);
                    }
                    break;
                case nameof(ObsWebsocketConnection.SourceOrderList):
                    lbSourceList.Dispatcher.Invoke(
                        () => UpdateSceneSourceListOrder()
                        );
                    break;
            }
        }

        private void UpdateSceneSourceListOrder()
        {
            ListCollectionView listCollection = (ListCollectionView)CollectionViewSource.GetDefaultView(lbSourceList.ItemsSource);
            if (listCollection != null)
            {
                listCollection.CustomSort = new Utils.OrderSceneItemsByListOfIds(ObsWebsocketConnection.Instance.SourceOrderList);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ObsWebsocketConnection.Instance.PropertyChanged -= Instance_PropertyChanged;
            ObsWebsocketConnection.Instance.Client.ErrorState -= WebSocket_Error_ContextSwitch;
        }

        #endregion

        #region obs-websocket

        private void WebSocket_Error_ContextSwitch(object sender, WsClientErrorMessage e)
        {
            _Context.Send(
                x => WebSocket_Error(e),
                null);
        }

        private void WebSocket_Error(WsClientErrorMessage e)
        {
            bool announceExtendedError = !ObsWebsocketConnection.Instance.Client.AutoReconnect;
            if (e.ReconnectDelay > 0 && ObsWebsocketConnection.Instance.Client.AutoReconnect && e.Error != null)
            {
                TextBlock_AnnounceChanged(tbReconnectCountdown);
                // Announce extended error for connection failure if
                //  exponential back-off minimum is greater than or equal to 20 seconds
                //  or if reconnect delay is between 20 and 40 seconds.
                if ((Preferences.Default.obs_reconnect_min_seconds >= 20 && e.ReconnectDelay == Preferences.Default.obs_reconnect_min_seconds) ||
                    (e.ReconnectDelay >= 20 && e.ReconnectDelay < 40))
                {
                    announceExtendedError = true;
                }
            }
            if (e.Error != null)
            {
                TextBlock_AnnounceChanged(tbStatus);
                if (announceExtendedError && e.Error.InnerException != null)
                {
                    TextBlock_AnnounceChanged(tbStatusExtended);
                }
            }
        }

        #endregion

        #region User Interface

        private async void CbScenes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) { return; }
            ObsScene selectedScene = (e.AddedItems[0] as ObsScene);
            if (selectedScene == e.AddedItems) { return; }
            SetCurrentSceneRequest request = ObsWsRequest.GetInstanceOfType(ObsRequestType.SetCurrentScene) as SetCurrentSceneRequest;
            request.SceneName = selectedScene.Name;
            await ObsWebsocketConnection.Instance.Client.ObsSend(request).ConfigureAwait(true);
        }

        private void TextBlock_AnnounceChanged(object sender)
        {
            AutomationPeer peer = (sender as UIElement).Dispatcher.Invoke(
                () => UIElementAutomationPeer.FromElement(sender as UIElement));
            if (peer == null) { return; }
            _Context.Send(
                x => peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged),
                null);
        }

        private void Menu_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            switch ((e.OriginalSource as MenuItem).Name)
            {
                case "Reconnect":
                    e.CanExecute = ObsWebsocketConnection.Instance.Client.State != WebSocketState.Connecting;
                    break;
                case "Disconnect":
                    e.CanExecute = ObsWebsocketConnection.Instance.Client.State == WebSocketState.Open;
                    break;
                default:
                    return;
            }
        }

        private async void Menu_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            switch ((e.OriginalSource as MenuItem).Name)
            {
                case "Reconnect":
                    await ObsWebsocketConnection.Instance.Reconnect().ConfigureAwait(true);
                    break;
                case "Disconnect":
                    await ObsWebsocketConnection.Instance.Disconnect().ConfigureAwait(true);
                    break;
                default:
                    return;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F4)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)
                    && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    App.Current.Shutdown();
                }
            }
            else if (e.Key == Key.F12)
            {
                if (!string.IsNullOrEmpty(ObsWebsocketConnection.Instance.ExtendedConnectionError))
                {
                    TextBlock_AnnounceChanged(tbStatusExtended);
                }
                else
                {
                    TextBlock_AnnounceChanged(tbStatus);
                }
            }
        }

        #endregion

    }
}
