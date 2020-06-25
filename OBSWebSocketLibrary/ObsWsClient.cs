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

        private async Task<Guid> OBS_Send(Dictionary<string, object> jsonDictionary)
        {
            // TODO: Switch from Dictionary to object
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

            obsReply.MessageObject = obsReply.RequestType switch
            {
                Data.Requests.GetVersion => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetVersion>(message),
                Data.Requests.GetAuthRequired => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetAuthRequired>(message),
                Data.Requests.Authenticate => await JsonSerializer.DeserializeAsync<Models.RequestReplies.Authenticate>(message),
                Data.Requests.SetHeartbeat => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetHeartbeat>(message),
                Data.Requests.SetFilenameFormatting => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetFilenameFormatting>(message),
                Data.Requests.GetFilenameFormatting => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetFilenameFormatting>(message),
                Data.Requests.GetStats => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetStats>(message),
                Data.Requests.BroadcastCustomMessage => await JsonSerializer.DeserializeAsync<Models.RequestReplies.BroadcastCustomMessage>(message),
                Data.Requests.GetVideoInfo => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetVideoInfo>(message),
                Data.Requests.OpenProjector => await JsonSerializer.DeserializeAsync<Models.RequestReplies.OpenProjector>(message),
                Data.Requests.ListOutputs => await JsonSerializer.DeserializeAsync<Models.RequestReplies.ListOutputs>(message),
                Data.Requests.GetOutputInfo => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetOutputInfo>(message),
                Data.Requests.StartOutput => await JsonSerializer.DeserializeAsync<Models.RequestReplies.StartOutput>(message),
                Data.Requests.StopOutput => await JsonSerializer.DeserializeAsync<Models.RequestReplies.StopOutput>(message),
                Data.Requests.SetCurrentProfile => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetCurrentProfile>(message),
                Data.Requests.GetCurrentProfile => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetCurrentProfile>(message),
                Data.Requests.ListProfiles => await JsonSerializer.DeserializeAsync<Models.RequestReplies.ListProfiles>(message),
                Data.Requests.StartStopRecording => await JsonSerializer.DeserializeAsync<Models.RequestReplies.StartStopRecording>(message),
                Data.Requests.StartRecording => await JsonSerializer.DeserializeAsync<Models.RequestReplies.StartRecording>(message),
                Data.Requests.StopRecording => await JsonSerializer.DeserializeAsync<Models.RequestReplies.StopRecording>(message),
                Data.Requests.PauseRecording => await JsonSerializer.DeserializeAsync<Models.RequestReplies.PauseRecording>(message),
                Data.Requests.ResumeRecording => await JsonSerializer.DeserializeAsync<Models.RequestReplies.ResumeRecording>(message),
                Data.Requests.SetRecordingFolder => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetRecordingFolder>(message),
                Data.Requests.GetRecordingFolder => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetRecordingFolder>(message),
                Data.Requests.StartStopReplayBuffer => await JsonSerializer.DeserializeAsync<Models.RequestReplies.StartStopReplayBuffer>(message),
                Data.Requests.StartReplayBuffer => await JsonSerializer.DeserializeAsync<Models.RequestReplies.StartReplayBuffer>(message),
                Data.Requests.StopReplayBuffer => await JsonSerializer.DeserializeAsync<Models.RequestReplies.StopReplayBuffer>(message),
                Data.Requests.SaveReplayBuffer => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SaveReplayBuffer>(message),
                Data.Requests.SetCurrentSceneCollection => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetCurrentSceneCollection>(message),
                Data.Requests.GetCurrentSceneCollection => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetCurrentSceneCollection>(message),
                Data.Requests.ListSceneCollections => await JsonSerializer.DeserializeAsync<Models.RequestReplies.ListSceneCollections>(message),
                Data.Requests.GetSceneItemProperties => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetSceneItemProperties>(message),
                Data.Requests.SetSceneItemProperties => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetSceneItemProperties>(message),
                Data.Requests.ResetSceneItem => await JsonSerializer.DeserializeAsync<Models.RequestReplies.ResetSceneItem>(message),
                Data.Requests.SetSceneItemRender => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetSceneItemRender>(message),
                Data.Requests.SetSceneItemPosition => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetSceneItemPosition>(message),
                Data.Requests.SetSceneItemTransform => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetSceneItemTransform>(message),
                Data.Requests.SetSceneItemCrop => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetSceneItemCrop>(message),
                Data.Requests.DeleteSceneItem => await JsonSerializer.DeserializeAsync<Models.RequestReplies.DeleteSceneItem>(message),
                Data.Requests.DuplicateSceneItem => await JsonSerializer.DeserializeAsync<Models.RequestReplies.DuplicateSceneItem>(message),
                Data.Requests.SetCurrentScene => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetCurrentScene>(message),
                Data.Requests.GetCurrentScene => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetCurrentScene>(message),
                Data.Requests.GetSceneList => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetSceneList>(message),
                Data.Requests.ReorderSceneItems => await JsonSerializer.DeserializeAsync<Models.RequestReplies.ReorderSceneItems>(message),
                Data.Requests.SetSceneTransitionOverride => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetSceneTransitionOverride>(message),
                Data.Requests.RemoveSceneTransitionOverride => await JsonSerializer.DeserializeAsync<Models.RequestReplies.RemoveSceneTransitionOverride>(message),
                Data.Requests.GetSceneTransitionOverride => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetSceneTransitionOverride>(message),
                Data.Requests.GetSourcesList => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetSourcesList>(message),
                Data.Requests.GetSourceTypesList => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetSourceTypesList>(message),
                Data.Requests.GetVolume => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetVolume>(message),
                Data.Requests.SetVolume => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetVolume>(message),
                Data.Requests.GetMute => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetMute>(message),
                Data.Requests.SetMute => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetMute>(message),
                Data.Requests.ToggleMute => await JsonSerializer.DeserializeAsync<Models.RequestReplies.ToggleMute>(message),
                Data.Requests.SetSourceName => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetSourceName>(message),
                Data.Requests.SetSyncOffset => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetSyncOffset>(message),
                Data.Requests.GetSyncOffset => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetSyncOffset>(message),
                Data.Requests.GetSourceSettings => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetSourceSettings>(message),
                Data.Requests.SetSourceSettings => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetSourceSettings>(message),
                Data.Requests.GetTextGDIPlusProperties => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetTextGDIPlusProperties>(message),
                Data.Requests.SetTextGDIPlusProperties => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetTextGDIPlusProperties>(message),
                Data.Requests.GetTextFreetype2Properties => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetTextFreetype2Properties>(message),
                Data.Requests.SetTextFreetype2Properties => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetTextFreetype2Properties>(message),
                Data.Requests.GetBrowserSourceProperties => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetBrowserSourceProperties>(message),
                Data.Requests.SetBrowserSourceProperties => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetBrowserSourceProperties>(message),
                Data.Requests.GetSpecialSources => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetSpecialSources>(message),
                Data.Requests.GetSourceFilters => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetSourceFilters>(message),
                Data.Requests.GetSourceFilterInfo => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetSourceFilterInfo>(message),
                Data.Requests.AddFilterToSource => await JsonSerializer.DeserializeAsync<Models.RequestReplies.AddFilterToSource>(message),
                Data.Requests.RemoveFilterFromSource => await JsonSerializer.DeserializeAsync<Models.RequestReplies.RemoveFilterFromSource>(message),
                Data.Requests.ReorderSourceFilter => await JsonSerializer.DeserializeAsync<Models.RequestReplies.ReorderSourceFilter>(message),
                Data.Requests.MoveSourceFilter => await JsonSerializer.DeserializeAsync<Models.RequestReplies.MoveSourceFilter>(message),
                Data.Requests.SetSourceFilterSettings => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetSourceFilterSettings>(message),
                Data.Requests.SetSourceFilterVisibility => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetSourceFilterVisibility>(message),
                Data.Requests.GetAudioMonitorType => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetAudioMonitorType>(message),
                Data.Requests.SetAudioMonitorType => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetAudioMonitorType>(message),
                Data.Requests.TakeSourceScreenshot => await JsonSerializer.DeserializeAsync<Models.RequestReplies.TakeSourceScreenshot>(message),
                Data.Requests.GetStreamingStatus => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetStreamingStatus>(message),
                Data.Requests.StartStopStreaming => await JsonSerializer.DeserializeAsync<Models.RequestReplies.StartStopStreaming>(message),
                Data.Requests.StartStreaming => await JsonSerializer.DeserializeAsync<Models.RequestReplies.StartStreaming>(message),
                Data.Requests.StopStreaming => await JsonSerializer.DeserializeAsync<Models.RequestReplies.StopStreaming>(message),
                Data.Requests.SetStreamSettings => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetStreamSettings>(message),
                Data.Requests.GetStreamSettings => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetStreamSettings>(message),
                Data.Requests.SaveStreamSettings => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SaveStreamSettings>(message),
                Data.Requests.SendCaptions => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SendCaptions>(message),
                Data.Requests.GetStudioModeStatus => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetStudioModeStatus>(message),
                Data.Requests.GetPreviewScene => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetPreviewScene>(message),
                Data.Requests.SetPreviewScene => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetPreviewScene>(message),
                Data.Requests.TransitionToProgram => await JsonSerializer.DeserializeAsync<Models.RequestReplies.TransitionToProgram>(message),
                Data.Requests.EnableStudioMode => await JsonSerializer.DeserializeAsync<Models.RequestReplies.EnableStudioMode>(message),
                Data.Requests.DisableStudioMode => await JsonSerializer.DeserializeAsync<Models.RequestReplies.DisableStudioMode>(message),
                Data.Requests.ToggleStudioMode => await JsonSerializer.DeserializeAsync<Models.RequestReplies.ToggleStudioMode>(message),
                Data.Requests.GetTransitionList => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetTransitionList>(message),
                Data.Requests.GetCurrentTransition => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetCurrentTransition>(message),
                Data.Requests.SetCurrentTransition => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetCurrentTransition>(message),
                Data.Requests.SetTransitionDuration => await JsonSerializer.DeserializeAsync<Models.RequestReplies.SetTransitionDuration>(message),
                Data.Requests.GetTransitionDuration => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetTransitionDuration>(message),
                Data.Requests.GetTransitionPosition => await JsonSerializer.DeserializeAsync<Models.RequestReplies.GetTransitionPosition>(message),
                _ => null
            };

            NewObsReply(obsReply);
        }
        #endregion

        #region ParseEvent()
        private async void ParseEvent(MemoryStream message, ObsEvent obsEvent)
        {
            message.Seek(0, SeekOrigin.Begin);

            obsEvent.MessageObject = obsEvent.EventType switch
            {
                Data.Events.SwitchScenes => await JsonSerializer.DeserializeAsync<Models.Events.SwitchScenes>(message),
                Data.Events.ScenesChanged => await JsonSerializer.DeserializeAsync<Models.Events.ScenesChanged>(message),
                Data.Events.SceneCollectionChanged => await JsonSerializer.DeserializeAsync<Models.Events.SceneCollectionChanged>(message),
                Data.Events.SceneCollectionListChanged => await JsonSerializer.DeserializeAsync<Models.Events.SceneCollectionListChanged>(message),
                Data.Events.SwitchTransition => await JsonSerializer.DeserializeAsync<Models.Events.SwitchTransition>(message),
                Data.Events.TransitionListChanged => await JsonSerializer.DeserializeAsync<Models.Events.TransitionListChanged>(message),
                Data.Events.TransitionDurationChanged => await JsonSerializer.DeserializeAsync<Models.Events.TransitionDurationChanged>(message),
                Data.Events.TransitionBegin => await JsonSerializer.DeserializeAsync<Models.Events.TransitionBegin>(message),
                Data.Events.TransitionEnd => await JsonSerializer.DeserializeAsync<Models.Events.TransitionEnd>(message),
                Data.Events.TransitionVideoEnd => await JsonSerializer.DeserializeAsync<Models.Events.TransitionVideoEnd>(message),
                Data.Events.ProfileChanged => await JsonSerializer.DeserializeAsync<Models.Events.ProfileChanged>(message),
                Data.Events.ProfileListChanged => await JsonSerializer.DeserializeAsync<Models.Events.ProfileListChanged>(message),
                Data.Events.StreamStarting => await JsonSerializer.DeserializeAsync<Models.Events.StreamStarting>(message),
                Data.Events.StreamStarted => await JsonSerializer.DeserializeAsync<Models.Events.StreamStarted>(message),
                Data.Events.StreamStopping => await JsonSerializer.DeserializeAsync<Models.Events.StreamStopping>(message),
                Data.Events.StreamStopped => await JsonSerializer.DeserializeAsync<Models.Events.StreamStopped>(message),
                Data.Events.StreamStatus => await JsonSerializer.DeserializeAsync<Models.Events.StreamStatus>(message),
                Data.Events.RecordingStarting => await JsonSerializer.DeserializeAsync<Models.Events.RecordingStarting>(message),
                Data.Events.RecordingStarted => await JsonSerializer.DeserializeAsync<Models.Events.RecordingStarted>(message),
                Data.Events.RecordingStopping => await JsonSerializer.DeserializeAsync<Models.Events.RecordingStopping>(message),
                Data.Events.RecordingStopped => await JsonSerializer.DeserializeAsync<Models.Events.RecordingStopped>(message),
                Data.Events.RecordingPaused => await JsonSerializer.DeserializeAsync<Models.Events.RecordingPaused>(message),
                Data.Events.RecordingResumed => await JsonSerializer.DeserializeAsync<Models.Events.RecordingResumed>(message),
                Data.Events.ReplayStarting => await JsonSerializer.DeserializeAsync<Models.Events.ReplayStarting>(message),
                Data.Events.ReplayStarted => await JsonSerializer.DeserializeAsync<Models.Events.ReplayStarted>(message),
                Data.Events.ReplayStopping => await JsonSerializer.DeserializeAsync<Models.Events.ReplayStopping>(message),
                Data.Events.ReplayStopped => await JsonSerializer.DeserializeAsync<Models.Events.ReplayStopped>(message),
                Data.Events.Exiting => await JsonSerializer.DeserializeAsync<Models.Events.Exiting>(message),
                Data.Events.Heartbeat => await JsonSerializer.DeserializeAsync<Models.Events.Heartbeat>(message),
                Data.Events.BroadcastCustomMessage => await JsonSerializer.DeserializeAsync<Models.Events.BroadcastCustomMessage>(message),
                Data.Events.SourceCreated => await JsonSerializer.DeserializeAsync<Models.Events.SourceCreated>(message),
                Data.Events.SourceDestroyed => await JsonSerializer.DeserializeAsync<Models.Events.SourceDestroyed>(message),
                Data.Events.SourceVolumeChanged => await JsonSerializer.DeserializeAsync<Models.Events.SourceVolumeChanged>(message),
                Data.Events.SourceMuteStateChanged => await JsonSerializer.DeserializeAsync<Models.Events.SourceMuteStateChanged>(message),
                Data.Events.SourceAudioSyncOffsetChanged => await JsonSerializer.DeserializeAsync<Models.Events.SourceAudioSyncOffsetChanged>(message),
                Data.Events.SourceAudioMixersChanged => await JsonSerializer.DeserializeAsync<Models.Events.SourceAudioMixersChanged>(message),
                Data.Events.SourceRenamed => await JsonSerializer.DeserializeAsync<Models.Events.SourceRenamed>(message),
                Data.Events.SourceFilterAdded => await JsonSerializer.DeserializeAsync<Models.Events.SourceFilterAdded>(message),
                Data.Events.SourceFilterRemoved => await JsonSerializer.DeserializeAsync<Models.Events.SourceFilterRemoved>(message),
                Data.Events.SourceFilterVisibilityChanged => await JsonSerializer.DeserializeAsync<Models.Events.SourceFilterVisibilityChanged>(message),
                Data.Events.SourceFiltersReordered => await JsonSerializer.DeserializeAsync<Models.Events.SourceFiltersReordered>(message),
                Data.Events.SourceOrderChanged => await JsonSerializer.DeserializeAsync<Models.Events.SourceOrderChanged>(message),
                Data.Events.SceneItemAdded => await JsonSerializer.DeserializeAsync<Models.Events.SceneItemAdded>(message),
                Data.Events.SceneItemRemoved => await JsonSerializer.DeserializeAsync<Models.Events.SceneItemRemoved>(message),
                Data.Events.SceneItemVisibilityChanged => await JsonSerializer.DeserializeAsync<Models.Events.SceneItemVisibilityChanged>(message),
                Data.Events.SceneItemLockChanged => await JsonSerializer.DeserializeAsync<Models.Events.SceneItemLockChanged>(message),
                Data.Events.SceneItemTransformChanged => await JsonSerializer.DeserializeAsync<Models.Events.SceneItemTransformChanged>(message),
                Data.Events.SceneItemSelected => await JsonSerializer.DeserializeAsync<Models.Events.SceneItemSelected>(message),
                Data.Events.SceneItemDeselected => await JsonSerializer.DeserializeAsync<Models.Events.SceneItemDeselected>(message),
                Data.Events.PreviewSceneChanged => await JsonSerializer.DeserializeAsync<Models.Events.PreviewSceneChanged>(message),
                Data.Events.StudioModeSwitched => await JsonSerializer.DeserializeAsync<Models.Events.StudioModeSwitched>(message),
                _ => null
            };

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