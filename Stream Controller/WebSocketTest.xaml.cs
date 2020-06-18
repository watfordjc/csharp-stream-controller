using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
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

namespace Stream_Controller
{
    /// <summary>
    /// Interaction logic for WebSocketTest.xaml
    /// </summary>
    public partial class WebSocketTest : Window, IDisposable
    {
        private ClientWebSocket webSocket = null;
        private bool autoscroll = false;
        private bool disposedValue;

        public WebSocketTest()
        {
            InitializeComponent();
        }

        private async void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            webSocket = ClientWebSocket();
            if (webSocket == null)
            {
                e.Handled = true;
                return;
            }
            txtStatus.Text = webSocket.State.ToString();
            if (webSocket.State == WebSocketState.Open)
            btnTest.IsEnabled = false;
            e.Handled = true;
            await Task.WhenAll(new[] { ReceiveMessages() });
            btnTest.IsEnabled = true;
        }

        private async void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            if (webSocket != null && webSocket.State != WebSocketState.Closed)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Requested by user", CancellationToken.None);
                txtStatus.Text = webSocket.State.ToString();
            }
            e.Handled = true;
        }

        private void AutoScroll_Checked(object sender, RoutedEventArgs e)
        {
            autoscroll = true;
            svScroll.ScrollToBottom();
            svScroll.ScrollChanged += OnScrollChange;
        }

        private void AutoScroll_Unchecked(object sender, RoutedEventArgs e)
        {
            autoscroll = false;
        }

        private void AutoScroll_Scrolled(object sender, RoutedEventArgs e)
        {
            cbAutoScroll.IsChecked = false;
        }

        private void OnScrollChange(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalOffset < 0 && autoscroll == true)
            {
                cbAutoScroll.IsChecked = false;
            }
        }

        private ClientWebSocket ClientWebSocket()
        {
            ClientWebSocket clientWebSocket = new ClientWebSocket();
            try
            {
                clientWebSocket.Options.SetBuffer(8192, 8192);
                clientWebSocket.ConnectAsync(new Uri("ws://localhost:4444"), CancellationToken.None).GetAwaiter().GetResult();
            } catch (WebSocketException e)
            {
                txtOutput.Text = "An error occurred: " + e.Message + "\n\n" + e.InnerException.Message;
                return null;
            }
            
            return clientWebSocket;
        }

        private async Task ReceiveMessages()
        {
            while (webSocket.State == WebSocketState.Open)
            {
                await ReceiveAsync();
            }
        }

        private async Task SendAsync(string message)
        {
            if (webSocket.State != WebSocketState.Open)
            {
                return;
            }
            ArraySegment<Byte> sendBuffer = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(message));
            await webSocket.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        // TODO: Licensing - function derived from https://stackoverflow.com/questions/23773407/a-websockets-receiveasync-method-does-not-await-the-entire-message
        private async Task<WebSocketReceiveResult> ReceiveAsync()
        {
            ArraySegment<Byte> receiveBuffer = new ArraySegment<Byte>(new Byte[8192]);
            WebSocketReceiveResult result = null;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                try
                {
                    do
                    {
                        result = await webSocket.ReceiveAsync(receiveBuffer, CancellationToken.None);
                        memoryStream.Write(receiveBuffer.Array, receiveBuffer.Offset, result.Count);
                    } while (!result.EndOfMessage);
                } catch (OutOfMemoryException e)
                {
                    txtOutput.Text += "Unable to receive a WebSocket message due to a buffer overflow.\n" + e.Message + "\n" + e.InnerException.Message + "\n";
                }

                memoryStream.Seek(0, SeekOrigin.Begin);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    using StreamReader streamReader = new StreamReader(memoryStream, Encoding.UTF8);
                    txtStatus.Text = webSocket.State.ToString();
                    txtOutput.Text += streamReader.ReadToEnd() + "\n\n";
                    if (autoscroll == true)
                    {
                        svScroll.ScrollToBottom();
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Binary)
                {
                    throw new NotImplementedException("Binary WebSocket messages not implemented.");
                }
            }
            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Program exiting", CancellationToken.None).GetAwaiter().GetResult();
                webSocket.Dispose();

                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
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
