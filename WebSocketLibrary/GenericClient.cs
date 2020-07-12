using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace uk.JohnCook.dotnet.WebSocketLibrary
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
        private WsClientErrorMessage errorMessage = new WsClientErrorMessage();
        private bool disposedValue;
        private const int RECV_BUFFER_SIZE = 8192;
        private const int SEND_BUFFER_SIZE = 8192;
        private readonly ArraySegment<byte> receiveBuffer;

        public delegate byte[] ReceivedBinaryMessage();
        public delegate string ReceivedTextMessage();
        public delegate WebSocketState StateChanged();
        public delegate string GenericWebsocketClientError();

        public event EventHandler<MemoryStream> ReceiveBinaryMessage;
        public event EventHandler<MemoryStream> ReceiveTextMessage;
        public event EventHandler<WebSocketState> StateChange;
        public event EventHandler<WsClientErrorMessage> ErrorState;

        /// <summary>
        /// Calls correct event invocation method depending on MessageType.
        /// </summary>
        /// <param name="receivedMessage">A ReceivedMessage instance.</param>
        private void ParseMessage(WsClientReceivedMessage receivedMessage)
        {
            receivedMessage.Message.Seek(0, SeekOrigin.Begin);
            if (receivedMessage.Result.MessageType == WebSocketMessageType.Text)
            {
                OnReceiveTextMessage(receivedMessage.Message);
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
        protected virtual void OnReceiveBinaryMessage(MemoryStream message)
        {
            ReceiveBinaryMessage?.Invoke(this, message);
        }

        /// <summary>
        /// Invoke event for received text messages.
        /// </summary>
        /// <param name="message">The message as a string.</param>
        protected virtual void OnReceiveTextMessage(MemoryStream message)
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
            errorMessage = new WsClientErrorMessage
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
            receiveBuffer = ArrayPool<byte>.Shared.Rent(RECV_BUFFER_SIZE);
            _Client.Options.SetBuffer(RECV_BUFFER_SIZE, SEND_BUFFER_SIZE, receiveBuffer);
            _ServerUrl = url;
        }

        /// <summary>
        /// Configure exponential back-off for reconnection attempts.
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
                    connected = await ConnectAsync().ConfigureAwait(false);
                }
                catch (WebSocketException e)
                {
                    OnErrorState(e, retryMs / 1000);
                }
                if (!connected)
                {
                    try
                    {
                        await DisconnectAsync(false).ConfigureAwait(false);
                        OnStateChange(WebSocketState.None);
                        await Task.Delay(retryMs, connectionCancellation.Token).ConfigureAwait(false);
                        retryMs = (int)(
                            retryMs * 2 < TimeSpan.FromMinutes(_MaximumRetryMinutes).TotalMilliseconds
                            ? retryMs * 2
                            : TimeSpan.FromMinutes(_MaximumRetryMinutes).TotalMilliseconds
                        );
                        _Client = new ClientWebSocket();
                    }
                    catch (TaskCanceledException)
                    {
                        OnStateChange(WebSocketState.Closed);
                        return false;
                    }
                }
            }
            Debug.Assert(_Client.State == WebSocketState.Open, $"AutoReconnectConnectAsync is about to say a connection is established, but _Client.State is {_Client.State}");
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
            try
            {
                await _Client.ConnectAsync(_ServerUrl, connectionCancellation.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException e)
            {
                OnErrorState(e, -1);
            }
            catch (WebSocketException)
            {
                throw;
            }
            finally
            {
                if (_Client != null)
                {
                    OnStateChange(_Client.State);
                }
            }
            return _Client != null && _Status == WebSocketState.Open;
        }

        /// <summary>
        /// Disconnect and clean-up.
        /// </summary>
        /// <returns>True when completed.</returns>
        public async Task<bool> DisconnectAsync(bool cancelExisting = true)
        {
            if (cancelExisting)
            {
                connectionCancellation?.Cancel(true);
            }
            if (_Client != null && _Client.State == WebSocketState.Open)
            {
                try
                {
                    await _Client.CloseAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None).ConfigureAwait(false);
                }
                catch (WebSocketException e)
                {
                    OnErrorState(e, -1);
                }
            }
            if (_Client != null)
            {
                OnStateChange(_Client.State);
                _Client?.Abort();
                _Client?.Dispose();
                _Client = null;
            }
            else
            {
                OnStateChange(WebSocketState.None);
            }
            return true;
        }

        /// <summary>
        /// Disconnect and clean-up, then reconnect (automatic retries).
        /// </summary>
        /// <returns>True if connected, false if cancelled.</returns>
        public async Task ReconnectAsync()
        {
            await DisconnectAsync(false).ConfigureAwait(false);
            _Client = new ClientWebSocket();
            try
            {
                await AutoReconnectConnectAsync().ConfigureAwait(false);
            }
            catch (WebSocketException e)
            {
                Debugger.Break();
                OnErrorState(e, -1);
            }
            catch (OperationCanceledException e)
            {
                Debugger.Break();
                OnErrorState(e, -1);
            }
        }

        /// <summary>
        /// Send a WebSocket message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>True if message sent.</returns>
        public async Task<bool> SendMessageAsync(ReadOnlyMemory<byte> message)
        {
            if (_Client.State != WebSocketState.Open)
            {
                return false;
            }
            // Only one message can be sent at a time. Wait until a message can be sent.
            await sendAsyncSemaphore.WaitAsync().ConfigureAwait(false);
            // We can now send a message. Our thread has entered sendAsyncSemaphore, so we should be the one to release it.
            try
            {
                await _Client.SendAsync(message, WebSocketMessageType.Text, true, connectionCancellation.Token).ConfigureAwait(true);
            }
            catch (TaskCanceledException)
            {
                sendAsyncSemaphore.Release();
                return false;
            }
            // Our thread can now release the sendAsyncSemaphore so another message can be sent.
            sendAsyncSemaphore.Release();
            return true;
        }

        /// <summary>
        /// Receive a WebSocket message.
        /// </summary>
        /// <returns>A ReceivedMessage object containing Result and Message.</returns>
        private async Task<WsClientReceivedMessage> ReceiveMessageAsync()
        {
            WsClientReceivedMessage receivedMessage = new WsClientReceivedMessage
            {
                Message = new MemoryStream()
            };

            do
            {
                try
                {
                    receivedMessage.Result = await _Client.ReceiveAsync(receiveBuffer, CancellationToken.None).ConfigureAwait(false);
                }
                catch (WebSocketException e)
                {
                    receivedMessage.Message.Dispose();
                    receivedMessage.Message = null;
                    OnStateChange(_Client.State);
                    OnErrorState(e, -1);
                    break;
                }
                receivedMessage.Message.Write(receiveBuffer.Array, receiveBuffer.Offset, receivedMessage.Result.Count);
            } while (!receivedMessage.Result.EndOfMessage);

            return receivedMessage;
        }

        /// <summary>
        /// Starts an asynchronous loop for receiving WebSocket messages.
        /// </summary>
        public async Task StartMessageReceiveLoop()
        {
            // Only one message can be received at a time. Wait until we can control the receive queue.
            await receiveAsyncSemaphore.WaitAsync().ConfigureAwait(false);
            // Store the context of our thread so we can release receiveAsyncSemaphore later.
            SynchronizationContext receiveLoopContext = SynchronizationContext.Current ?? new SynchronizationContext();
            // The long-lived while loop should run synchronously.
            while (_Status == WebSocketState.Open || _Status == WebSocketState.CloseReceived)
            {
                try
                {
                    // There can be a long time between messages received... does it matter which thread picks up the next one?
                    WsClientReceivedMessage receivedMessage = await ReceiveMessageAsync().ConfigureAwait(false);
                    // There should currently only be three MessageType: Close, Text, and Binary.
                    const int expectedMessageTypeCount = 3;
                    Debug.Assert(Enum.GetNames(typeof(WebSocketMessageType)).Length == expectedMessageTypeCount, $"StartMessageReceiveLoop expected {expectedMessageTypeCount} MessageType, there are {Enum.GetNames(typeof(WebSocketMessageType)).Length}");
                    // Result is null if a connection issue occurred. MessageType==Close if server is closing connection.
                    if (receivedMessage.Result == null || receivedMessage.Result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                    // The two other MessageType are Text and Binary.
                    else if (receivedMessage.Result.MessageType == WebSocketMessageType.Text || receivedMessage.Result.MessageType == WebSocketMessageType.Binary)
                    {
                        ParseMessage(receivedMessage);
                    }
                    else
                    {
                        /* Impossible condition unless:
                         * (a) RFC 6455 is updated,
                         * (b) ClientWebSocket includes a new websocket protocol OpCode,
                         * (c) This library hasn't been debugged since (b) made it into a new .NET Core version.
                         * In such a case, the above if/elseif needs updating as GenericClient is potentially no longer fit for purpose.
                         */
                        WebSocketException e = new WebSocketException(
                            WebSocketError.InvalidMessageType,
                            $"This version of {typeof(GenericClient).Name} does not support the following ClientWebSocket MessageType: {receivedMessage.Result.MessageType}."
                           );
                        OnErrorState(e, -1);
                        throw e;
                    }
                }
                catch (WebSocketException e)
                {
                    Debugger.Break();
                    OnStateChange(_Client.State);
                    OnErrorState(e, -1);
                }
            }
            // No more messages can be received on this connection. Get the receiveLoopContext thread to release the receiveAsyncSemaphore.
            receiveLoopContext.Send(
                x => receiveAsyncSemaphore.Release(),
                null);
        }

        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    connectionCancellation.Dispose();
                    sendAsyncSemaphore.Dispose();
                    receiveAsyncSemaphore.Dispose();
                    ArrayPool<byte>.Shared.Return(receiveBuffer.Array);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _Client?.CloseAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None).GetAwaiter().GetResult();
                _Client?.Abort();
                _Client?.Dispose();
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
