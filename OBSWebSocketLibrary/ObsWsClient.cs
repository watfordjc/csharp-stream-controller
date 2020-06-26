﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace OBSWebSocketLibrary
{
    /// <summary>
    /// A WebsocketClient that handles OBS connections.
    /// </summary>
    public class ObsWsClient : WebSocketLibrary.GenericClient, IDisposable
    {
        private readonly SynchronizationContext context;
        private bool disposedValue;
        public bool AutoReconnect { get; set; }
        private readonly System.Timers.Timer heartBeatCheck = new System.Timers.Timer(8000);
        public Dictionary<Guid, Data.Requests> sentMessageGuids = new Dictionary<Guid, Data.Requests>();

        public ObsWsClient(Uri url) : base(url)
        {
            context = SynchronizationContext.Current;
            StateChange += WebSocket_Connected;
            OnObsReply += FurtherProcessObsReply;
            OnObsEvent += FurtherProcessObsEvent;
        }

        public class ObsReply
        {
            public Guid MessageId { get; set; }
            public Data.Requests RequestType { get; set; }
            public object MessageObject { get; set; }
            public string Status { get; set; }
        }

        public class ObsEvent
        {
            public Data.Events EventType { get; set; }
            public object MessageObject { get; set; }
        }

        public delegate ObsReply OnNewObsReply();
        public delegate ObsEvent OnNewObsEvent();

        public event EventHandler<ObsReply> OnObsReply;
        public event EventHandler<ObsEvent> OnObsEvent;

        protected virtual void NewObsReply(ObsReply obsReply)
        {
            OnObsReply?.Invoke(this, obsReply);
        }

        protected virtual void NewObsEvent(ObsEvent obsEvent)
        {
            OnObsEvent?.Invoke(this, obsEvent);
        }


        private void WebSocket_Connected(object sender, WebSocketState state)
        {
            if (state != WebSocketState.Open) { return; }
            heartBeatCheck.Elapsed += HeartBeatTimer_Elapsed;
            ReceiveTextMessage += WebSocket_NewTextMessage;
            StartMessageReceiveLoop();
            OBS_EnableHeartBeat();

            /*
             * TODO: OBS sourceType to sourceSettings.audio_device_id represented as NAudio property
             * 
             * browser_source -> N/A
             * dshow_input -> Device.FriendlyName + ":"
             * ffmpeg_source -> N/A
             * game_capture -> N/A
             * waspi_input_capture -> Device.ID
             * waspi_output_capture -> Device.ID
             * 
            */
        }

        private void HeartBeatTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            heartBeatCheck.Stop();
            if (AutoReconnect)
            {
                context.Send(
                    async (x) => await ReconnectAsync()
                    , null);
            }
            else
            {
                context.Send(
                    async (x) => await DisconnectAsync()
                , null);
            }
        }

        private async Task<Guid> OBS_Send(object message)
        {
            Guid guid = (message as Models.Requests.RequestBase).MessageId;
            Data.Requests messageType = (message as Models.Requests.RequestBase).RequestType;
            sentMessageGuids.Add(guid, messageType);
            await SendMessageAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)));
            return guid;
        }

        private Guid OBS_EnableHeartBeat()
        {
            Models.Requests.SetHeartbeat message = new Models.Requests.SetHeartbeat()
            {
                Enable = true
            };
            return OBS_Send(message).Result;
        }

        public Guid OBS_GetSourcesList()
        {
            return OBS_Send(new Models.Requests.GetSourcesList()).Result;
        }

        public Guid OBS_GetSourceSettings(string sourceName)
        {
            Models.Requests.GetSourceSettings message = new Models.Requests.GetSourceSettings()
            {
                SourceName = sourceName
            };
            return OBS_Send(message).Result;
        }

        private void FurtherProcessObsReply(object sender, ObsReply obsReply)
        {
            switch (obsReply.RequestType)
            {
                case Data.Requests.SetHeartbeat:
                    Trace.WriteLine($"Server response to enabling HeartBeat: {obsReply.Status}");
                    break;
                case Data.Requests.GetSourcesList:
                    foreach (Models.RequestReplies.GetSourcesList.Source device in (obsReply.MessageObject as Models.RequestReplies.GetSourcesList).Sources)
                    {
                        OBS_GetSourceSettings(device.Name);
                    }
                    break;
                default: break;
            }
        }

        private void FurtherProcessObsEvent(object sender, ObsEvent obsEvent)
        {
            switch (obsEvent.EventType)
            {
                case Data.Events.Heartbeat:
                    heartBeatCheck.Enabled = (obsEvent.MessageObject as Models.Events.Heartbeat).Pulse;
                    heartBeatCheck.Enabled = true;
                    break;
                default: break;
            }
        }

        private void WebSocket_NewTextMessage(object sender, MemoryStream message)
        {
            OBS_ParseJson(message);
        }

        private JsonDocument GetJsonDocumentFromMemoryStream(MemoryStream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            Span<byte> messageBytes = ArrayPool<byte>.Shared.Rent((int)stream.Length);
            stream.Read(messageBytes);
            JsonDocument document = JsonDocument.Parse(messageBytes.Slice(0, (int)stream.Length).ToArray());
            ArrayPool<byte>.Shared.Return(messageBytes.ToArray());
            return document;
        }

        private void OBS_ParseJson(MemoryStream message)
        {
            using JsonDocument document = GetJsonDocumentFromMemoryStream(message);
            JsonElement root = document.RootElement;

            if (root.TryGetProperty("message-id", out JsonElement messageIdJson))
            {
                Guid.TryParse(messageIdJson.GetString(), out Guid guid);
                root.TryGetProperty("status", out JsonElement statusJson);
                if (sentMessageGuids.TryGetValue(guid, out Data.Requests requestType))
                {
                    Trace.WriteLine($"Received response to message of type {requestType} with GUID {guid} - {statusJson.GetString()}.");
                    sentMessageGuids.Remove(guid);
                }
                Enum.TryParse(requestType.ToString(), out Data.Requests reqType);
                ObsReply obsReply = new ObsReply()
                {
                    MessageId = guid,
                    RequestType = reqType,
                    Status = statusJson.GetString()
                };
                ParseReply(message, obsReply);
            }
            else if (root.TryGetProperty("update-type", out JsonElement updateTypeJson))
            {
                Trace.WriteLine($"Received a message of type {updateTypeJson}.");
                bool isStreaming = root.TryGetProperty("stream-timecode", out JsonElement jsonStreamTimecode);
                bool isRecording = root.TryGetProperty("rec-timecode", out JsonElement jsonRecTimecode);
                Enum.TryParse(updateTypeJson.GetString(), out Data.Events eventType);
                ObsEvent obsEvent = new ObsEvent()
                {
                    EventType = eventType
                };

                ParseEvent(message, obsEvent);
            }
            else
            {
                Trace.WriteLine("Unexpected JSON.");
            }
        }

        private async void ParseReply(MemoryStream message, ObsReply obsReply)
        {
            message.Seek(0, SeekOrigin.Begin);
            if (obsReply.Status == "ok" && Enum.IsDefined(typeof(Data.Requests), obsReply.RequestType))
            {
                obsReply.MessageObject = await JsonSerializer.DeserializeAsync(message, Data.RequestReply.GetType(obsReply.RequestType));
                NewObsReply(obsReply);
            }
            else if (obsReply.Status == "error")
            {
                Models.RequestReplies.Error replyModel = (Models.RequestReplies.Error)await JsonSerializer.DeserializeAsync(message, typeof(Models.RequestReplies.Error));
                ErrorMessage errorMessage = new ErrorMessage()
                {
                    Error = new Exception(
                        $"The {sentMessageGuids.GetValueOrDefault(obsReply.MessageId)} request {obsReply.MessageId} was responded to by a status of {obsReply.Status}.",
                        new Exception(replyModel.Error)
                        ),
                    ReconnectDelay = -1
                };
                base.OnErrorState(errorMessage.Error, errorMessage.ReconnectDelay);
            }
        }

        private async void ParseEvent(MemoryStream message, ObsEvent obsEvent)
        {
            message.Seek(0, SeekOrigin.Begin);
            obsEvent.MessageObject = await JsonSerializer.DeserializeAsync(message, Data.Event.GetType(obsEvent.EventType));
            NewObsEvent(obsEvent);
        }

        protected new virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    heartBeatCheck.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~WebSocketTest()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public new void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}