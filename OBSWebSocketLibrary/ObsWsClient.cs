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
        private static readonly ClientWebSocket _Client = new ClientWebSocket();
        private readonly Uri _ServerUrl = null;
        private WebSocketState _Status;
        private System.Timers.Timer _ReconnectTimer;
        private bool reconnect;
        private bool disposedValue;

        public struct ReceivedMessage
        {
            public WebSocketReceiveResult Result { get; set; }
            public byte[] Message { get; set; }
        }

        public ObsWsClient(Uri url)
        {
            _Client.Options.SetBuffer(8192, 8192);
            _ServerUrl = url;
        }

        public async Task<bool> ConnectAsync()
        {
            if (_Status == WebSocketState.Open)
            {
                return false;
            }
            try
            {
                await _Client.ConnectAsync(_ServerUrl, CancellationToken.None);
                _Status = _Client.State;
                return true;
            } catch (Exception e)
            {
                Trace.WriteLine("Connect error: " + e.GetBaseException() + " - " + e.InnerException.GetBaseException());
                return false;
            }
        }

        public async Task<bool> DisconnectAsync()
        {
            if (_Status == WebSocketState.Closed)
            {
                return false;
            }
            await _Client.CloseAsync(WebSocketCloseStatus.Empty, "Closing connection", CancellationToken.None);
            _Status = _Client.State;
            return true;
        }

        public async Task ReconnectAsync()
        {
            await DisconnectAsync();
            await ConnectAsync();
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
                ReceivedMessage receivedMessage = await ReceiveMessageAsync();
                if (receivedMessage.Result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                } else if (receivedMessage.Result.MessageType == WebSocketMessageType.Text)
                {
                    // Handle text message
                } else if (receivedMessage.Result.MessageType == WebSocketMessageType.Binary)
                {
                    throw new NotImplementedException("Binary WebSocket messages not implemented.");
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
                _Client.CloseAsync(WebSocketCloseStatus.Empty, "Closing connection", CancellationToken.None).GetAwaiter().GetResult();
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
