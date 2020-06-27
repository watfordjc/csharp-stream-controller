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
    public partial class WebSocketTest : Window
    {
        private readonly ObsWsClient webSocket;
        private bool autoscroll = false;
        private int reconnectDelay;
        private readonly int SCROLL_BUFFER_MAX_CHARS = 65000;

        public WebSocketTest()
        {
            InitializeComponent();
            InitialiseWindow();
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
            WindowUtilityLibrary.MakeWindowActive(WindowUtilityLibrary.WindowType.PreferencesWindow);
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItemAudioInterfaces_Click(object sender, RoutedEventArgs e)
        {
            WindowUtilityLibrary.MakeWindowActive(WindowUtilityLibrary.WindowType.MainWindow);
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
                txtStatus.Text = webSocket.AutoReconnect
                    ? $"Connection Status: {e} (Current reconnect delay: {reconnectDelay} seconds)"
                    : $"Connection Status: {e}";
            }
        }

        private void WebSocket_ErrorMessage(object sender, ObsWsClient.ErrorMessage errorMessage)
        {
            if (errorMessage.Error == null) { return; }
            txtOutput.Text += $"{errorMessage.Error.Message}\n{errorMessage.Error.InnerException?.Message}\n\n";
            if (errorMessage.ReconnectDelay > 0)
            {
                reconnectDelay = errorMessage.ReconnectDelay;
            }
        }

        private void WebSocket_Connected(object sender, WebSocketState e)
        {
            if (e != WebSocketState.Open) { return; }
        }

        private void WebSocket_NewTextMessage(object sender, MemoryStream message)
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


    }
}
