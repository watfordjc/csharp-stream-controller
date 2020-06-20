using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace OBSWebSocketLibrary
{
    public class ObsWsClient : IDisposable
    {
        private ClientWebSocket _Client = new ClientWebSocket();
        private readonly Uri _ServerUrl = null;
        private WebSocketState _Status;
        private CancellationTokenSource connectionCancellation;
        private bool disposedValue;
        private int _RetrySeconds = 5;
        private int _MaximumRetryMinutes = 10;
        private ErrorMessage errorMessage = new ErrorMessage();

        private struct ReceivedMessage
        {
            public WebSocketReceiveResult Result { get; set; }
            public byte[] Message { get; set; }
        }

        public struct ErrorMessage
        {
            public Exception Error { get; set; }
            public int ReconnectDelay { get; set; }
        }

        public delegate byte[] ReceivedBinaryMessage();
        public delegate string ReceivedTextMessage();
        public delegate WebSocketState StateChanged();
        public delegate string Error();

        public event EventHandler<byte[]> ReceiveBinaryMessage;
        public event EventHandler<string> ReceiveTextMessage;
        public event EventHandler<WebSocketState> StateChange;
        public event EventHandler<ErrorMessage> ErrorState;

        private void ParseMessage(ReceivedMessage receivedMessage)
        {
            if (receivedMessage.Result.MessageType == WebSocketMessageType.Text)
            {
                OnReceiveTextMessage(Encoding.UTF8.GetString(receivedMessage.Message));
            }
            else if (receivedMessage.Result.MessageType == WebSocketMessageType.Binary)
            {
                OnReceiveBinaryMessage(receivedMessage.Message);
            }
        }

        protected virtual void OnReceiveBinaryMessage(byte[] message)
        {
            ReceiveBinaryMessage?.Invoke(this, message);
        }

        protected virtual void OnReceiveTextMessage(string message)
        {
            ReceiveTextMessage?.Invoke(this, message);
        }

        protected virtual void OnStateChange(WebSocketState newState)
        {
            if (_Status != newState)
            {
                _Status = newState;
                StateChange?.Invoke(this, newState);
            }
        }

        protected virtual void OnErrorState(Exception e, int reconnectDelay)
        {
            errorMessage = new ErrorMessage
            {
                Error = e,
                ReconnectDelay = reconnectDelay
            };
            ErrorState?.Invoke(this, errorMessage);
        }

        public ObsWsClient(Uri url)
        {
            _Client.Options.SetBuffer(8192, 8192);
            _ServerUrl = url;
        }

        public void SetExponentialBackoff(int initialRetrySeconds, int maximumRetryMinutes)
        {
            _RetrySeconds = initialRetrySeconds;
            _MaximumRetryMinutes = maximumRetryMinutes;
        }

        public async Task<bool> AutoReconnectConnectAsync()
        {
            bool connected = false;
            connectionCancellation = new CancellationTokenSource();
            int retryMs = (int)TimeSpan.FromSeconds(_RetrySeconds).TotalMilliseconds;
            while (!connected)
            {
                try
                {
                    OnStateChange(WebSocketState.Connecting);
                    connected = await ConnectAsync();
                    if (!connected)
                    {
                        OnErrorState(errorMessage.Error, retryMs / 1000);
                        _Client = new ClientWebSocket();
                        OnStateChange(_Client.State);
                        await Task.Delay(retryMs, connectionCancellation.Token);
                        if (retryMs < TimeSpan.FromMinutes(_MaximumRetryMinutes).TotalMilliseconds)
                        {
                            retryMs *= 2;
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    OnStateChange(WebSocketState.Closed);
                    return false;
                }
            }
            return true;
        }

        private async Task<bool> ConnectAsync()
        {
            if (_Client == null)
            {
                _Client = new ClientWebSocket();
                OnStateChange(_Client.State);
            }
            if (_Status == WebSocketState.Open)
            {
                return true;
            }
            try
            {
                await _Client.ConnectAsync(_ServerUrl, connectionCancellation.Token);
                OnStateChange(_Client.State);
                return true;
            }
            catch (WebSocketException e)
            {
                errorMessage.Error = e;
                return false;
            }
        }

        public async Task<bool> DisconnectAsync()
        {
            connectionCancellation.Cancel(true);
            if (_Status == WebSocketState.Open)
            {
                await _Client.CloseAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None);
            }
            if (_Client != null)
            {
                OnStateChange(_Client.State);
                _Client.Abort();
                _Client.Dispose();
                _Client = null;
            } else
            {
                OnStateChange(WebSocketState.Closed);
            }
            return true;
        }

        public async Task ReconnectAsync()
        {
            await DisconnectAsync();
            _Client = new ClientWebSocket();
            await AutoReconnectConnectAsync();
        }

        public async Task<bool> SendMessageAsync(String message)
        {
            if (_Client.State != WebSocketState.Open)
            {
                return false;
            }
            ArraySegment<Byte> sendBuffer = new ArraySegment<Byte>(Encoding.UTF8.GetBytes(message));
            await _Client.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
            return true;
        }

        // TODO: Licensing - function derived from https://stackoverflow.com/questions/23773407/a-websockets-receiveasync-method-does-not-await-the-entire-message
        private async Task<ReceivedMessage> ReceiveMessageAsync()
        {
            ReceivedMessage receivedMessage = new ReceivedMessage();
            ArraySegment<Byte> receiveBuffer = new ArraySegment<Byte>(new Byte[8192]);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                do
                {
                    receivedMessage.Result = await _Client.ReceiveAsync(receiveBuffer, CancellationToken.None);
                    memoryStream.Write(receiveBuffer.Array, receiveBuffer.Offset, receivedMessage.Result.Count);
                } while (!receivedMessage.Result.EndOfMessage);

                memoryStream.Seek(0, SeekOrigin.Begin);
                receivedMessage.Message = memoryStream.ToArray();
            }
            return receivedMessage;
        }

        public async void StartMessageReceiveLoop()
        {
            while (_Status == WebSocketState.Open || _Status == WebSocketState.CloseReceived)
            {
                ReceivedMessage receivedMessage;
                try
                {
                    receivedMessage = await ReceiveMessageAsync();
                    if (receivedMessage.Result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                    else if (receivedMessage.Result.MessageType == WebSocketMessageType.Text || receivedMessage.Result.MessageType == WebSocketMessageType.Binary)
                    {
                        ParseMessage(receivedMessage);
                    }
                }
                catch (WebSocketException e)
                {
                    OnErrorState(e, -1);
                }
            }
        }

        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _Client.CloseAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None).GetAwaiter().GetResult();
                _Client.Abort();
                _Client.Dispose();
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~ObsWsClient()
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

        #endregion

    }
}
