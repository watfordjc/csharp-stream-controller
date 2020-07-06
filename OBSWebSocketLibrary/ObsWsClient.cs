using OBSWebSocketLibrary.Data;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    public class ObsWsClient : WebSocketLibrary.GenericClient
    {
        private readonly SynchronizationContext context;
        public bool AutoReconnect { get; set; }
        private readonly System.Timers.Timer heartBeatCheck = new System.Timers.Timer(8000);
        private readonly Dictionary<Guid, Models.TypeDefs.ObsRequestMetadata> sentMessageGuids = new Dictionary<Guid, Models.TypeDefs.ObsRequestMetadata>();

        public ObsWsClient(Uri url) : base(url)
        {
            context = SynchronizationContext.Current;
            StateChange += WebSocket_Connected;
            OnObsEvent += FurtherProcessObsEvent;
        }

        public delegate Models.TypeDefs.ObsReply OnNewObsReply();
        public delegate Models.TypeDefs.ObsEvent OnNewObsEvent();

        public event EventHandler<Models.TypeDefs.ObsReply> OnObsReply;
        public event EventHandler<Models.TypeDefs.ObsEvent> OnObsEvent;

        protected virtual void NewObsReply(Models.TypeDefs.ObsReply obsReply)
        {
            OnObsReply?.Invoke(this, obsReply);
        }

        protected virtual void NewObsEvent(Models.TypeDefs.ObsEvent obsEvent)
        {
            OnObsEvent?.Invoke(this, obsEvent);
        }

        public bool WaitingForReplyForType(OBSWebSocketLibrary.Data.RequestType requestType)
        {
            return sentMessageGuids.Values.Any(x => x.OriginalRequestType == requestType);
        }

        public bool WaitingForReply(Guid messageId)
        {
            return sentMessageGuids.ContainsKey(messageId);
        }

        private void WebSocket_Connected(object sender, WebSocketState state)
        {
            if (state != WebSocketState.Open) { return; }
            heartBeatCheck.Elapsed += HeartBeatTimer_Elapsed;
            ReceiveTextMessage += WebSocket_NewTextMessage;
            StartMessageReceiveLoop();
            OBS_EnableHeartBeat();
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

        public async ValueTask<Guid> ObsSend(object message)
        {
            if (message == null) { throw new ArgumentNullException(nameof(message)); }

            Models.TypeDefs.ObsRequestMetadata metadata = new Models.TypeDefs.ObsRequestMetadata()
            {
                RequestGuid = (message as Models.Requests.RequestBase).MessageId,
                OriginalRequestType = (message as Models.Requests.RequestBase).RequestType,
                OriginalRequestData = message
            };
            sentMessageGuids.Add(metadata.RequestGuid, metadata);
            await SendMessageAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)));
            return metadata.RequestGuid;
        }

        private Guid OBS_EnableHeartBeat()
        {
            Models.Requests.SetHeartbeat message = new Models.Requests.SetHeartbeat()
            {
                Enable = true
            };
            return ObsSend(message).Result;
        }

        private void FurtherProcessObsEvent(object sender, Models.TypeDefs.ObsEvent obsEvent)
        {
            switch (obsEvent.EventType)
            {
                case Data.EventType.Heartbeat:
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

        private static JsonDocument GetJsonDocumentFromMemoryStream(MemoryStream stream)
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

            JsonElement sourceType;

            if (root.TryGetProperty("message-id", out JsonElement messageIdJson))
            {
                Guid.TryParse(messageIdJson.GetString(), out Guid guid);
                root.TryGetProperty("status", out JsonElement statusJson);
                bool sentMessageGuidExists = sentMessageGuids.TryGetValue(guid, out Models.TypeDefs.ObsRequestMetadata requestMetadata);
                if (sentMessageGuidExists)
                {
                    Enum.TryParse(requestMetadata.OriginalRequestType.ToString(), out Data.RequestType reqType);
                    Models.TypeDefs.ObsReply obsReply = new Models.TypeDefs.ObsReply()
                    {
                        MessageId = guid,
                        RequestType = reqType,
                        Status = statusJson.GetString()
                    };
                    if (sentMessageGuidExists)
                    {
                        obsReply.RequestMetadata = requestMetadata;
                        sentMessageGuids.Remove(guid);
                    }
                    switch (reqType)
                    {
                        case Data.RequestType.GetSourceSettings:
                        case Data.RequestType.SetSourceSettings:
                            root.TryGetProperty("sourceType", out sourceType);
                            obsReply.SourceType = ObsTypes.ObsTypeNameDictionary[sourceType.ToString()];
                            break;
                        case Data.RequestType.GetSourceFilterInfo:
                            root.TryGetProperty("type", out sourceType);
                            obsReply.SourceType = ObsTypes.ObsTypeNameDictionary[sourceType.ToString()];
                            break;
                        default:
                            break;
                    }
                    ParseReply(message, obsReply);
                }
                else
                {
                    Trace.WriteLine($"message-id {messageIdJson} received, but no matching request found.");
                }
            }
            else if (root.TryGetProperty("update-type", out JsonElement updateTypeJson))
            {
                Trace.WriteLine($"Received a message of type {updateTypeJson}.");
                bool isStreaming = root.TryGetProperty("stream-timecode", out JsonElement jsonStreamTimecode);
                bool isRecording = root.TryGetProperty("rec-timecode", out JsonElement jsonRecTimecode);
                Enum.TryParse(updateTypeJson.GetString(), out Data.EventType eventType);
                Models.TypeDefs.ObsEvent obsEvent = new Models.TypeDefs.ObsEvent()
                {
                    EventType = eventType
                };
                switch (eventType)
                {
                    case Data.EventType.SourceCreated:
                        root.TryGetProperty("sourceKind", out sourceType);
                        obsEvent.SourceType = ObsTypes.ObsTypeNameDictionary[sourceType.ToString()];
                        break;
                    case Data.EventType.SourceFilterAdded:
                        root.TryGetProperty("filterType", out sourceType);
                        obsEvent.SourceType = ObsTypes.ObsTypeNameDictionary[sourceType.ToString()];
                        break;
                    default:
                        break;
                }
                ParseEvent(message, obsEvent);
            }
            else
            {
                Trace.WriteLine("Unexpected JSON.");
            }
        }

        private static void GetJsonElementFromObjectProperty(object messageObject, string propertyName, out ReadOnlyMemory<char> json)
        {
            json = ((JsonElement)messageObject.GetType().GetProperty(propertyName).GetValue(messageObject, null)).GetRawText().AsMemory();
        }

        private bool CanDeserializeSourceType(Data.SourceType sourceType, ReadOnlyMemory<char> settingsJson, out object deserialisedObject)
        {
            Type modelType = Data.SourceTypeSettings.GetType(sourceType);
            if (modelType == null)
            {
                NotImplementedException ex = new NotImplementedException($"Source type {sourceType} has not yet been implemented.", new JsonException($"Unable to parse: {settingsJson}"));
                OnErrorState(ex, -1);
                deserialisedObject = null;
                return false;
            }
            else
            {
                Span<byte> span = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetByteCount(settingsJson.Span));
                int length = Encoding.UTF8.GetBytes(settingsJson.Span, span);
                deserialisedObject = JsonSerializer.Deserialize(span.Slice(0, length), modelType);
                ArrayPool<byte>.Shared.Return(span.ToArray());
                return true;
            }
        }

        private async void ParseReply(MemoryStream message, Models.TypeDefs.ObsReply obsReply)
        {
            message.Seek(0, SeekOrigin.Begin);
            if (obsReply.Status == "ok" && Enum.IsDefined(typeof(Data.RequestType), obsReply.RequestType))
            {
                obsReply.MessageObject = await JsonSerializer.DeserializeAsync(message, Data.RequestReply.GetType(obsReply.RequestType));

                object settingsObject;
                ReadOnlyMemory<char> settingsJson;
                Data.SourceType sourceType;

                switch (obsReply.RequestType)
                {
                    case Data.RequestType.GetSourceSettings:
                        GetJsonElementFromObjectProperty(obsReply.MessageObject, "SourceSettings", out settingsJson);
                        if (!CanDeserializeSourceType(obsReply.SourceType, settingsJson, out settingsObject)) { break; }
                        (obsReply.MessageObject as Models.RequestReplies.GetSourceSettings).SourceSettingsObj = settingsObject;
                        break;
                    case Data.RequestType.SetSourceSettings:
                        GetJsonElementFromObjectProperty(obsReply.MessageObject, "SourceSettings", out settingsJson);
                        if (!CanDeserializeSourceType(obsReply.SourceType, settingsJson, out settingsObject)) { break; }
                        (obsReply.MessageObject as Models.RequestReplies.SetSourceSettings).SourceSettingsObj = settingsObject;
                        break;
                    case Data.RequestType.GetSourceFilters:
                        foreach (Models.TypeDefs.ObsReplyFilter filter in (obsReply.MessageObject as Models.RequestReplies.GetSourceFilters).Filters)
                        {
                            settingsJson = filter.Settings.GetRawText().AsMemory();
                            sourceType = ObsTypes.ObsTypeNameDictionary[filter.Type];
                            if (!CanDeserializeSourceType(sourceType, settingsJson, out settingsObject)) { continue; }
                            filter.SettingsObj = settingsObject;
                        }
                        break;
                    case Data.RequestType.GetSourceFilterInfo:
                        GetJsonElementFromObjectProperty(obsReply.MessageObject, "Settings", out settingsJson);
                        if (!CanDeserializeSourceType(obsReply.SourceType, settingsJson, out settingsObject)) { break; }
                        (obsReply.MessageObject as Models.RequestReplies.GetSourceFilterInfo).SettingsObj = settingsObject;
                        break;
                    default:
                        break;
                }

                NewObsReply(obsReply);
            }
            else if (obsReply.Status == "error")
            {
                Models.RequestReplies.ObsError replyModel = (Models.RequestReplies.ObsError)await JsonSerializer.DeserializeAsync(message, typeof(Models.RequestReplies.ObsError));
                WebSocketLibrary.Models.ErrorMessage errorMessage = new WebSocketLibrary.Models.ErrorMessage()
                {
                    Error = new Exception(
                        $"The {obsReply.RequestMetadata.OriginalRequestType} request {obsReply.MessageId} was responded to by a status of {obsReply.Status}.",
                        new Exception(replyModel.Error)
                        ),
                    ReconnectDelay = -1
                };
                base.OnErrorState(errorMessage.Error, errorMessage.ReconnectDelay);
            }
        }

        private async void ParseEvent(MemoryStream message, Models.TypeDefs.ObsEvent obsEvent)
        {
            message.Seek(0, SeekOrigin.Begin);
            obsEvent.MessageObject = await JsonSerializer.DeserializeAsync(message, Data.ObsEvent.GetType(obsEvent.EventType));

            object settingsObject;
            ReadOnlyMemory<char> settingsJson;

            switch (obsEvent.EventType)
            {
                case Data.EventType.SourceCreated:
                    GetJsonElementFromObjectProperty(obsEvent.MessageObject, "SourceSettings", out settingsJson);
                    if (!CanDeserializeSourceType(obsEvent.SourceType, settingsJson, out settingsObject)) { Trace.WriteLine($"{obsEvent.EventType} ({obsEvent.SourceType}) -> {settingsJson}"); break; }
                    (obsEvent.MessageObject as Models.Events.SourceCreated).SourceSettingsObj = settingsObject;
                    break;
                case Data.EventType.SourceFilterAdded:
                    GetJsonElementFromObjectProperty(obsEvent.MessageObject, "FilterSettings", out settingsJson);
                    if (!CanDeserializeSourceType(obsEvent.SourceType, settingsJson, out settingsObject)) { Trace.WriteLine($"{obsEvent.EventType} ({obsEvent.SourceType}) -> {settingsJson}"); break; }
                    (obsEvent.MessageObject as Models.Events.SourceFilterAdded).FilterSettingsObj = settingsObject;
                    break;
            }
            NewObsEvent(obsEvent);
        }


    }
}
