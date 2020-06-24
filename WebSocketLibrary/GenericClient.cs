using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Buffers;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketLibrary
{
    /// <summary>
    /// A generic WebSocketClient object with genericised handling
    /// </summary>
    public class GenericClient : IDisposable
    {
        private ClientWebSocket _Client = new ClientWebSocket();
        private readonly Uri _ServerUrl = null;
        private WebSocketState _Status;
        private CancellationTokenSource connectionCancellation;
        private readonly SemaphoreSlim sendAsyncSemaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim receiveAsyncSemaphore = new SemaphoreSlim(1);
        private int _RetrySeconds = 5;
        private int _MaximumRetryMinutes = 10;
        private ErrorMessage errorMessage = new ErrorMessage();
        private bool disposedValue;

        /// <summary>
        /// Object containing a WebSocketReceiveResult and a received byte[].
        /// </summary>
        private struct ReceivedMessage
        {
            public WebSocketReceiveResult Result { get; set; }
            public byte[] Message { get; set; }
        }

        /// <summary>
        /// Object containing an Exception and current reconnect delay.
        /// </summary>
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

        /// <summary>
        /// Calls correct event invocation method depending on MessageType.
        /// </summary>
        /// <param name="receivedMessage">A ReceivedMessage instance.</param>
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

        /// <summary>
        /// Invoke event for received binary messages.
        /// </summary>
        /// <param name="message">The message as a byte[].</param>
        protected virtual void OnReceiveBinaryMessage(byte[] message)
        {
            ReceiveBinaryMessage?.Invoke(this, message);
        }

        /// <summary>
        /// Invoke event for received text messages.
        /// </summary>
        /// <param name="message">The message as a string.</param>
        protected virtual void OnReceiveTextMessage(string message)
        {
            ReceiveTextMessage?.Invoke(this, message);
        }

        /// <summary>
        /// Invoke event for connection state changes.
        /// </summary>
        /// <param name="newState">The new WebSocketState.</param>
        protected virtual void OnStateChange(WebSocketState newState)
        {
            if (_Status != newState)
            {
                _Status = newState;
                StateChange?.Invoke(this, newState);
            }
        }

        /// <summary>
        /// Invoke event for exceptions.
        /// </summary>
        /// <param name="e">The exception to propagate.</param>
        /// <param name="reconnectDelay">The current reconnection attempt delay.</param>
        protected virtual void OnErrorState(Exception e, int reconnectDelay)
        {
            errorMessage = new ErrorMessage
            {
                Error = e,
                ReconnectDelay = reconnectDelay
            };
            ErrorState?.Invoke(this, errorMessage);
        }

        /// <summary>
        /// Instantiation of a new instance.
        /// </summary>
        /// <param name="url">The WebSocket server URI to connect to.</param>
        public GenericClient(Uri url)
        {
            _Client.Options.SetBuffer(8192, 8192);
            _ServerUrl = url;
        }

        /// <summary>
        /// Configure exponential backoff for reconnection attempts.
        /// </summary>
        /// <param name="initialRetrySeconds">Delay between connection error and first reconnection attempt (seconds).</param>
        /// <param name="maximumRetryMinutes">Maximum delay between connection attempts (minutes).</param>
        public void SetExponentialBackoff(int initialRetrySeconds, int maximumRetryMinutes)
        {
            _RetrySeconds = initialRetrySeconds;
            _MaximumRetryMinutes = maximumRetryMinutes;
        }

        /// <summary>
        /// Try to connect (automatic retries) until success or cancellation.
        /// </summary>
        /// <returns>True on successful connection, false on task cancellation.</returns>
        public async Task<bool> AutoReconnectConnectAsync()
        {
            bool connected = false;
            connectionCancellation = new CancellationTokenSource();
            int retryMs = (int)TimeSpan.FromSeconds(_RetrySeconds).TotalMilliseconds;
            OnErrorState(null, _RetrySeconds);
            while (!connected)
            {
                OnStateChange(WebSocketState.Connecting);
                try
                {
                    connected = await ConnectAsync();
                }
                catch (WebSocketException e)
                {
                    OnErrorState(e, retryMs / 1000);
                    OnStateChange(_Client.State);
                }
                if (!connected)
                {
                    try
                    {
                        _Client = new ClientWebSocket();
                        OnStateChange(_Client.State);
                        await Task.Delay(retryMs, connectionCancellation.Token);
                        if (retryMs < TimeSpan.FromMinutes(_MaximumRetryMinutes).TotalMilliseconds)
                        {
                            retryMs *= 2;
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        OnStateChange(WebSocketState.Closed);
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Try to connect once (no retries).
        /// </summary>
        /// <returns>True if connected.</returns>
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
            await _Client.ConnectAsync(_ServerUrl, connectionCancellation.Token);
            OnStateChange(_Client.State);
            return true;
        }

        /// <summary>
        /// Disconnect and cleanup.
        /// </summary>
        /// <returns>True when completed.</returns>
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
            }
            else
            {
                OnStateChange(WebSocketState.Closed);
            }
            return true;
        }

        /// <summary>
        /// Disconnect and cleanup, then reconnect (automatic retries).
        /// </summary>
        /// <returns>True if connected, false if cancelled.</returns>
        public async Task ReconnectAsync()
        {
            await DisconnectAsync();
            _Client = new ClientWebSocket();
            await AutoReconnectConnectAsync();
        }

        /// <summary>
        /// Send a WebSocket message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>True if message sent.</returns>
        public async Task<bool> SendMessageAsync(string message)
        {
            if (_Client.State != WebSocketState.Open)
            {
                return false;
            }
            ArraySegment<byte> sendBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            await sendAsyncSemaphore.WaitAsync();
            await _Client.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
            sendAsyncSemaphore.Release();
            return true;
        }

        // TODO: Licensing - function derived from https://stackoverflow.com/questions/23773407/a-websockets-receiveasync-method-does-not-await-the-entire-message
        /// <summary>
        /// Receive a WebSocket message.
        /// </summary>
        /// <returns>A ReceivedMessage object containing Result and Message.</returns>
        private async Task<ReceivedMessage> ReceiveMessageAsync()
        {
            ReceivedMessage receivedMessage = new ReceivedMessage();
            ArraySegment<byte> receiveBuffer = new ArraySegment<byte>(new Byte[8192]);

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

        /// <summary>
        /// Starts an asynchronous loop for receiving WebSocket messages.
        /// </summary>
        public async void StartMessageReceiveLoop()
        {
            await receiveAsyncSemaphore.WaitAsync();
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
            receiveAsyncSemaphore.Release();
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
        ~GenericClient()
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
