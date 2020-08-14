using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using uk.JohnCook.dotnet.OBSWebSocketLibrary;
using uk.JohnCook.dotnet.StreamController.Properties;
using uk.JohnCook.dotnet.WebSocketLibrary;
using Windows.UI;

namespace uk.JohnCook.dotnet.StreamController
{
    public class ObsWebsocketConnection : IDisposable, INotifyPropertyChanged
    {
        #region Instantiation

        #region variables

        public static ObsWebsocketConnection Instance { get { return lazySingleton.Value; } }
        public ObsWsClient Client { get; private set; }
        public string ConnectionStatus { get; private set; } = Properties.Resources.text_disconnected;
        public string ConnectionError { get; private set; } = Properties.Resources.text_disconnected;
        public string ExtendedConnectionError { get; private set; } = String.Empty;
        public Brush ConnectionStatusBrush { get; private set; } = Brushes.Gray;
        public static readonly Brush PrimaryBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xE2, 0xC1, 0xEA));
        public static readonly Brush SecondaryBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xC5, 0xC0, 0xEB));
        private readonly System.Timers.Timer ReconnectCountdownTimer = new System.Timers.Timer(1000);
        private readonly SemaphoreSlim iconSemaphore = new SemaphoreSlim(1);
        private int ReconnectTimeRemaining;
        private bool disposedValue;

        private static readonly Lazy<ObsWebsocketConnection> lazySingleton = new Lazy<ObsWebsocketConnection>(
            () => new ObsWebsocketConnection()
            );

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private ObsWebsocketConnection()
        {
            Preferences.Default.PropertyChanged += Default_PropertyChanged;
            ReconnectCountdownTimer.Elapsed += ReconnectCountdownTimer_Elapsed;
        }

        #endregion

        #region Preference changes

        private async void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(Preferences.Default.obs_settings_changed) || Preferences.Default.obs_settings_changed == false)
            {
                return;
            }

            if (ReconnectCountdownTimer.Enabled || Client.CanSend)
            {
                ReconnectCountdownTimer.Enabled = false;
            }

            CreateClient();
            SystemTrayIcon.Instance.UpdateTrayIcon();
            if (Client.AutoReconnect)
            {
                await Connect().ConfigureAwait(false);
            }
        }

        #endregion

        #region Set up connection

        /// <summary>
        /// Create a new ObsWsClient as Instance.Client
        /// </summary>
        public static void CreateClient()
        {
            if (Instance.Client != null)
            {
                Instance.Client.StateChange -= Instance.Client_StateChange;
                Instance.Client.ErrorState -= Instance.Client_Error;
                Instance.Client.Dispose();
            }
            Instance.Client = new ObsWsClient(new UriBuilder(
                                Preferences.Default.obs_uri_scheme,
                                Preferences.Default.obs_uri_host,
                                int.Parse(Preferences.Default.obs_uri_port, CultureInfo.InvariantCulture)
                                ).Uri)
            {
                PasswordPreference = Preferences.Default.obs_password,
                AutoReconnect = Preferences.Default.obs_auto_reconnect
            };
            Instance.Client.SetExponentialBackoff(Preferences.Default.obs_reconnect_min_seconds, Preferences.Default.obs_reconnect_max_minutes);
            Instance.Client.StateChange += Instance.Client_StateChange;
            Instance.Client.ErrorState += Instance.Client_Error;
        }

        /// <summary>
        /// Establish an obs-websocket connection
        /// </summary>
        public async Task Connect()
        {
            if (Client == null)
            {
                CreateClient();
            }
            if (Client.State == WebSocketState.Open)
            {
                return;
            }
            Client.AutoReconnect = Preferences.Default.obs_auto_reconnect;
            Client.SetExponentialBackoff(Preferences.Default.obs_reconnect_min_seconds, Preferences.Default.obs_reconnect_max_minutes);
            if (Client.AutoReconnect)
            {
                await Client.AutoReconnectConnectAsync().ConfigureAwait(true);
            } else
            {
                await Client.ConnectAsync().ConfigureAwait(true);
            }
        }

        public async Task Disconnect()
        {
            if (Client == null)
            {
                return;
            }
            Client.AutoReconnect = false;
            ReconnectCountdownTimer.Stop();
            await Client.DisconnectAsync(true).ConfigureAwait(false);
            ConnectionStatus = Properties.Resources.text_disconnected;
            NotifyPropertyChanged(nameof(ConnectionStatus));
            ConnectionError = Properties.Resources.window_audio_check_successfully_disconnected;
            NotifyPropertyChanged(nameof(ConnectionError));
            ExtendedConnectionError = String.Empty;
            NotifyPropertyChanged(nameof(ExtendedConnectionError));
        }

        public async Task Reconnect()
        {
            await Disconnect().ConfigureAwait(false);
            await Connect().ConfigureAwait(false);
        }

        /// <summary>
        /// React to Instance.ReconnectCountdownTimer Elapsed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReconnectCountdownTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ReconnectTimeRemaining--;
            if (ReconnectTimeRemaining > 0)
            {
                ConnectionStatus = String.Format(CultureInfo.CurrentCulture, Properties.Resources.window_audio_check_reconnect_delay_format, TimeSpan.FromSeconds(ReconnectTimeRemaining).ToString("c", CultureInfo.CurrentCulture));
                NotifyPropertyChanged(nameof(ConnectionStatus));
            }
        }

        /// <summary>
        /// React to websocket connection state changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="newState">New websocket connection state</param>
        private void Client_StateChange(object sender, WebSocketState newState)
        {
            if (newState == WebSocketState.Open)
            {
                ConnectionError = Properties.Resources.text_aok;
                NotifyPropertyChanged(nameof(ConnectionError));
                ExtendedConnectionError = String.Empty;
                NotifyPropertyChanged(nameof(ExtendedConnectionError));
                ReconnectCountdownTimer.Stop();
                ConnectionStatus = Properties.Resources.text_connected;
                NotifyPropertyChanged(nameof(ConnectionStatus));
                _ = ChangeStatusColor(newState).ConfigureAwait(false);
            }
            else if (newState != WebSocketState.Connecting && Client.AutoReconnect)
            {
                ReconnectCountdownTimer.Start();
                ConnectionStatus = String.Format(CultureInfo.CurrentCulture, Properties.Resources.window_audio_check_reconnect_delay_format, TimeSpan.FromSeconds(ReconnectTimeRemaining).ToString("c", CultureInfo.CurrentCulture));
                NotifyPropertyChanged(nameof(ConnectionStatus));
            }
            else if (newState == WebSocketState.Closed)
            {
                ConnectionStatus = Properties.Resources.text_disconnected;
                NotifyPropertyChanged(nameof(ConnectionStatus));
                ConnectionError = Properties.Resources.window_audio_check_successfully_disconnected;
                NotifyPropertyChanged(nameof(ConnectionError));
            }
            else if (newState == WebSocketState.None)
            {
                _ = ChangeStatusColor(newState).ConfigureAwait(false);
            }
            else if (newState == WebSocketState.Connecting)
            {
                ReconnectCountdownTimer.Stop();
                ConnectionStatus = Properties.Resources.window_audio_check_connecting;
                NotifyPropertyChanged(nameof(ConnectionStatus));
                if (Client.PasswordPreference != Preferences.Default.obs_password)
                {
                    Client.PasswordPreference = Preferences.Default.obs_password;
                }
                ConnectionError = Properties.Resources.window_audio_check_error_state_cleared;
                NotifyPropertyChanged(nameof(ConnectionError));
                ExtendedConnectionError = String.Empty;
                NotifyPropertyChanged(nameof(ExtendedConnectionError));
                _ = ChangeStatusColor(newState).ConfigureAwait(false);
            }
        }

        private async void Client_Error(object sender, WsClientErrorMessage e)
        {
            if (e.ReconnectDelay > 0)
            {
                ReconnectTimeRemaining = e.ReconnectDelay;
            }
            else
            {
                ReconnectCountdownTimer.Stop();
                await Client.DisconnectAsync(false).ConfigureAwait(false);
            }
            if (e.Error != null)
            {
                ConnectionError = e.Error.Message;
                NotifyPropertyChanged(nameof(ConnectionError));
                ExtendedConnectionError = e.Error.InnerException?.Message;
                NotifyPropertyChanged(nameof(ExtendedConnectionError));
                _ = ChangeStatusColor(WebSocketState.Closed).ConfigureAwait(false);
            }
        }

        #endregion

        #region Status colour changes

        private static readonly Dictionary<Brush, System.Drawing.Icon> brushToIconDictionary = new Dictionary<Brush, System.Drawing.Icon>() {
            { Brushes.Gray, Properties.Resources.icon_neutral },
            { PrimaryBrush, Properties.Resources.icon },
            { SecondaryBrush, Properties.Resources.icon_secondary },
            { Brushes.DarkGreen, Properties.Resources.icon_dark_green },
            { Brushes.DarkGoldenrod, Properties.Resources.icon_dark_golden_rod },
            { Brushes.Red, Properties.Resources.icon_red }
        };

        public async Task ChangeStatusColor(Brush brush1, bool returnToNeutral = true)
        {
            // Low priority changes are out of date after 250ms.
            bool haveSemaphore = await iconSemaphore.WaitAsync(250).ConfigureAwait(true);

            // If we haven't entered semaphore after 250ms, give up
            if (!haveSemaphore)
            {
                return;
            }

            // Use first colour for 0.25 seconds
            if (brush1 != null)
            {
                ConnectionStatusBrush = brush1;
                NotifyPropertyChanged(nameof(ConnectionStatusBrush));
                SystemTrayIcon.Instance.NotifyIcon.Icon = brushToIconDictionary[brush1];
                await Task.Delay(250).ConfigureAwait(true);
            }
            // Use second colour for 0.25 seconds
            if (returnToNeutral)
            {
                ConnectionStatusBrush = Brushes.Gray;
                NotifyPropertyChanged(nameof(ConnectionStatusBrush));
                SystemTrayIcon.Instance.NotifyIcon.Icon = Properties.Resources.icon_neutral;
                await Task.Delay(250).ConfigureAwait(true);
            }

            // Release semaphore to allow colour change
            iconSemaphore.Release();
        }

        private async Task ChangeStatusColor(WebSocketState state)
        {
            // High priority changes can wait forever.
            bool haveSemaphore = await iconSemaphore.WaitAsync(-1).ConfigureAwait(true);
            Debug.Assert(haveSemaphore, "haveSemaphore should be true at this point.");

            // Change status colour based on connection status
            ConnectionStatusBrush = state switch
            {
                WebSocketState.Open => Brushes.DarkGreen,
                WebSocketState.Connecting => Brushes.DarkGoldenrod,
                _ => Brushes.Red
            };
            NotifyPropertyChanged(nameof(ConnectionStatusBrush));

            // Change system tray icon based on connection status
            SystemTrayIcon.Instance.NotifyIcon.Icon = brushToIconDictionary[ConnectionStatusBrush];
            // Maintain colour for one second
            await Task.Delay(1000).ConfigureAwait(true);

            // Release semaphore to allow colour change
            iconSemaphore.Release();
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
                    ReconnectCountdownTimer.Dispose();
                    iconSemaphore.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ObsWebsocketConnection()
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


