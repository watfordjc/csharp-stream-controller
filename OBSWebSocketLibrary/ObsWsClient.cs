using System;
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
        public Dictionary<Guid, string> sentMessageGuids = new Dictionary<Guid, string>();
        private Models.Events.Heartbeat heartBeatMessage;

        public ObsWsClient(Uri url) : base(url)
        {
            context = SynchronizationContext.Current;
            StateChange += WebSocket_Connected;
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

        private async Task<Guid> OBS_Send(Dictionary<string, object> jsonDictionary)
        {
            Guid guid = Guid.NewGuid();
            jsonDictionary.TryGetValue("request-type", out object requestType);
            sentMessageGuids.Add(guid, requestType.ToString());
            jsonDictionary.Add("message-id", guid.ToString());
            await SendMessageAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(jsonDictionary)).AsMemory());
            return guid;
        }

        private Guid OBS_EnableHeartBeat()
        {
            Dictionary<string, object> jsonDictionary = new Dictionary<string, object>
            {
                { "request-type", "SetHeartbeat" },
                { "enable", true }
            };
            return OBS_Send(jsonDictionary).Result;
        }

        public Guid OBS_GetSourcesList()
        {
            Dictionary<string, object> jsonDictionary = new Dictionary<string, object>
            {
                { "request-type", "GetSourcesList" }
            };
            return OBS_Send(jsonDictionary).Result;
        }

        public Guid OBS_GetSourceSettings(string sourceName)
        {
            Dictionary<string, object> jsonDictionary = new Dictionary<string, object>
            {
                { "request-type", "GetSourceSettings" },
                { "sourceName", sourceName }
            };
            return OBS_Send(jsonDictionary).Result;
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
                if (sentMessageGuids.TryGetValue(guid, out string requestType))
                {
                    Trace.WriteLine($"Received response to message of type {requestType} with GUID {guid} - {statusJson.GetString()}.");
                    sentMessageGuids.Remove(guid);
                }
                Enum.TryParse(requestType, out Data.Requests reqType);
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

        #region ParseReply()
        private async void ParseReply(MemoryStream message, ObsReply obsReply)
        {
            message.Seek(0, SeekOrigin.Begin);
            // TODO: Handle responses to sent messages
            switch (obsReply.RequestType)
            {
                case Data.Requests.SetHeartbeat:
                    Trace.WriteLine($"Server response to enabling HeartBeat: {obsReply.Status}");
                    break;
                case Data.Requests.GetSourcesList:
                    obsReply.MessageObject = await JsonSerializer.DeserializeAsync<Models.Requests.GetSourcesList>(message);
                    foreach (Models.Requests.GetSourcesList.Source device in (obsReply.MessageObject as Models.Requests.GetSourcesList).Sources)
                    {
                        OBS_GetSourceSettings(device.Name);
                    }
                    break;
                case Data.Requests.GetSourceSettings:
                    obsReply.MessageObject = await JsonSerializer.DeserializeAsync<Models.Requests.GetSourceSettings>(message);
                    break;
            }
            NewObsReply(obsReply);
        }
        #endregion

        #region ParseEvent()
        private async void ParseEvent(MemoryStream message, ObsEvent obsEvent)
        {
            message.Seek(0, SeekOrigin.Begin);
            switch (obsEvent.EventType)
            {
                case Data.Events.SwitchScenes:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SwitchScenes>(message);
                    break;
                case Data.Events.ScenesChanged:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.ScenesChanged>(message);
                    break;
                case Data.Events.SceneCollectionChanged:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SceneCollectionChanged>(message);
                    break;
                case Data.Events.SceneCollectionListChanged:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SceneCollectionListChanged>(message);
                    break;
                case Data.Events.SwitchTransition:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SwitchTransition>(message);
                    break;
                case Data.Events.TransitionListChanged:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.TransitionListChanged>(message);
                    break;
                case Data.Events.TransitionDurationChanged:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.TransitionDurationChanged>(message);
                    break;
                case Data.Events.TransitionBegin:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.TransitionBegin>(message);
                    break;
                case Data.Events.TransitionEnd:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.TransitionEnd>(message);
                    break;
                case Data.Events.TransitionVideoEnd:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.TransitionVideoEnd>(message);
                    break;
                case Data.Events.ProfileChanged:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.ProfileChanged>(message);
                    break;
                case Data.Events.ProfileListChanged:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.ProfileListChanged>(message);
                    break;
                case Data.Events.StreamStarting:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.StreamStarting>(message);
                    break;
                case Data.Events.StreamStarted:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.StreamStarted>(message);
                    break;
                case Data.Events.StreamStopping:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.StreamStopping>(message);
                    break;
                case Data.Events.StreamStopped:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.StreamStopped>(message);
                    break;
                case Data.Events.StreamStatus:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.StreamStatus>(message);
                    break;
                case Data.Events.RecordingStarting:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.RecordingStarting>(message);
                    break;
                case Data.Events.RecordingStarted:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.RecordingStarted>(message);
                    break;
                case Data.Events.RecordingStopping:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.RecordingStopping>(message);
                    break;
                case Data.Events.RecordingStopped:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.RecordingStopped>(message);
                    break;
                case Data.Events.RecordingPaused:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.RecordingPaused>(message);
                    break;
                case Data.Events.RecordingResumed:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.RecordingResumed>(message);
                    break;
                case Data.Events.ReplayStarting:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.ReplayStarting>(message);
                    break;
                case Data.Events.ReplayStarted:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.ReplayStarted>(message);
                    break;
                case Data.Events.ReplayStopping:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.ReplayStopping>(message);
                    break;
                case Data.Events.ReplayStopped:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.ReplayStopped>(message);
                    break;
                case Data.Events.Exiting:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.Exiting>(message);
                    break;
                case Data.Events.Heartbeat:
                    heartBeatMessage = await JsonSerializer.DeserializeAsync<Models.Events.Heartbeat>(message);
                    obsEvent.MessageObject = heartBeatMessage;
                    heartBeatCheck.Enabled = heartBeatMessage.Pulse;
                    heartBeatCheck.Enabled = true;
                    break;
                case Data.Events.BroadcastCustomMessage:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.BroadcastCustomMessage>(message);
                    break;
                case Data.Events.SourceCreated:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SourceCreated>(message);
                    break;
                case Data.Events.SourceDestroyed:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SourceDestroyed>(message);
                    break;
                case Data.Events.SourceVolumeChanged:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SourceVolumeChanged>(message);
                    break;
                case Data.Events.SourceMuteStateChanged:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SourceMuteStateChanged>(message);
                    break;
                case Data.Events.SourceAudioSyncOffsetChanged:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SourceAudioSyncOffsetChanged>(message);
                    break;
                case Data.Events.SourceAudioMixersChanged:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SourceAudioMixersChanged>(message);
                    break;
                case Data.Events.SourceRenamed:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SourceRenamed>(message);
                    break;
                case Data.Events.SourceFilterAdded:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SourceFilterAdded>(message);
                    break;
                case Data.Events.SourceFilterRemoved:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SourceFilterRemoved>(message);
                    break;
                case Data.Events.SourceFilterVisibilityChanged:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SourceFilterVisibilityChanged>(message);
                    break;
                case Data.Events.SourceFiltersReordered:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SourceFiltersReordered>(message);
                    break;
                case Data.Events.SourceOrderChanged:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SourceOrderChanged>(message);
                    break;
                case Data.Events.SceneItemAdded:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SceneItemAdded>(message);
                    break;
                case Data.Events.SceneItemRemoved:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SceneItemRemoved>(message);
                    break;
                case Data.Events.SceneItemVisibilityChanged:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SceneItemVisibilityChanged>(message);
                    break;
                case Data.Events.SceneItemLockChanged:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SceneItemLockChanged>(message);
                    break;
                case Data.Events.SceneItemTransformChanged:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SceneItemTransformChanged>(message);
                    break;
                case Data.Events.SceneItemSelected:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SceneItemSelected>(message);
                    break;
                case Data.Events.SceneItemDeselected:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.SceneItemDeselected>(message);
                    break;
                case Data.Events.PreviewSceneChanged:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.PreviewSceneChanged>(message);
                    break;
                case Data.Events.StudioModeSwitched:
                    obsEvent.MessageObject = await JsonSerializer.DeserializeAsync<Models.Events.StudioModeSwitched>(message);
                    break;
            }
            NewObsEvent(obsEvent);
        }
        #endregion

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