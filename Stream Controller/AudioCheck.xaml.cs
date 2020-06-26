using NAudio.CoreAudioApi;
using NAudioWrapperLibrary;
using OBSWebSocketLibrary;
using OBSWebSocketLibrary.Models.RequestReplies;
using Stream_Controller.SharedModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private static readonly AudioInterfaces audioInterfaces = AudioInterfaces.Instance;
        private readonly ObservableCollection<AudioInterface> devices = audioInterfaces.Devices;
        private readonly ObsWsClient webSocket;
        private int reconnectDelay = -1;
        private bool audioDevicesEnumerate = false;
        private bool obsWebsocketConnected = false;
        private string connectionError = String.Empty;

        public AudioCheck()
        {
            InitializeComponent();
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
            ConnectionCheck();
            devices.CollectionChanged += DeviceCollectionChanged;
            audioInterfaces.DeviceCollectionEnumerated += AudioDevicesEnumerated;
            webSocket.SetExponentialBackoff(Preferences.Default.obs_reconnect_min_seconds, Preferences.Default.obs_reconnect_max_minutes);
            webSocket.StateChange += WebSocket_StateChange;
            webSocket.ErrorState += WebSocket_Error;
            ObsWebsocketConnect();
        }

        private void WebSocket_Error(object sender, GenericClient.ErrorMessage e)
        {
            if (e.ReconnectDelay > 0)
            {
                reconnectDelay = e.ReconnectDelay;
            }
            if (e.Error != null)
            {
                connectionError = $"{e.Error.Message}\n{e.Error.InnerException?.Message}";
            }
        }

        private void DeviceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ConnectionCheck();
        }

        private async void ObsWebsocketConnect()
        {
            await webSocket.AutoReconnectConnectAsync();
        }

        private void WebSocket_StateChange(object sender, WebSocketState newState)
        {
            if (newState == WebSocketState.Open)
            {
                connectionError = String.Empty;
                obsWebsocketConnected = true;
                reconnectDelay = -1;
                ConnectionCheck();
            }
            else
            {
                obsWebsocketConnected = false;
                ConnectionCheck();
            }
        }

        private void AudioDevicesEnumerated(object sender, bool e)
        {
            if (e)
            {
                audioDevicesEnumerate = true;
                ConnectionCheck();
            }
        }

        private void ConnectionCheck()
        {
            tbNaudioStatus.Dispatcher.Invoke(
                () => UpdateUIConnectStatus()
            );
        }

        private void UpdateUIConnectStatus()
        {
            tbNaudioStatus.Text = String.Format(
                "{0} audio devices enumerated\nWebsocket connection {1} established{2}",
                devices.Count,
                obsWebsocketConnected ? "is" : "is not",
                reconnectDelay > 0 ? $"\nCurrent reconnect delay: {reconnectDelay} seconds" : ""
                );
            tbWebsocketError.Text = connectionError;
            pbNaudioStatus.IsIndeterminate = !(audioDevicesEnumerate && obsWebsocketConnected);
            if (audioDevicesEnumerate && obsWebsocketConnected)
            {
                pbNaudioStatus.Value = 100;
            }
        }

    }
}
