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
using uk.JohnCook.dotnet.WebSocketLibrary;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.Data;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.TypeDefs;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequests;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequestReplies;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsEvents;
using uk.JohnCook.dotnet.StreamController.SharedModels;
using System.Resources;
using System.Globalization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary
{
    /// <summary>
    /// A WebsocketClient that handles OBS connections.
    /// </summary>
    public class ObsWsClient : GenericClient
    {
        private bool disposedValue;
        private readonly ResourceManager rm = new ResourceManager("uk.JohnCook.dotnet.OBSWebSocketLibrary.Properties.Resources", typeof(ObsWsClient).Assembly);
        private readonly SynchronizationContext context;
        public bool AutoReconnect { get; set; }
        private readonly System.Timers.Timer heartBeatCheck = new System.Timers.Timer(8000);
        private readonly Dictionary<Guid, ObsRequestMetadata> sentMessageGuids = new Dictionary<Guid, ObsRequestMetadata>();
        public string PasswordPreference { get; set; }
        public bool CanSend { get; private set; }
        public WebSocketState State { get; private set; }

        public ObsWsClient(Uri url) : base(url)
        {
            Properties.Resources.Culture = CultureInfo.CurrentCulture;
            context = SynchronizationContext.Current;
            StateChange += WebSocket_StateChange;
            OnObsEvent += FurtherProcessObsEvent;
        }

        public delegate ObsReplyObject OnNewObsReply();
        public delegate ObsEventObject OnNewObsEvent();

        public event EventHandler<ObsReplyObject> OnObsReply;
        public event EventHandler<ObsEventObject> OnObsEvent;

        protected virtual void NewObsReply(ObsReplyObject obsReply)
        {
            OnObsReply?.Invoke(this, obsReply);
        }

        protected virtual void NewObsEvent(ObsEventObject obsEvent)
        {
            OnObsEvent?.Invoke(this, obsEvent);
        }

        public bool WaitingForReplyForType(ObsRequestType requestType)
        {
            return sentMessageGuids.Values.Any(x => x.OriginalRequestType == requestType);
        }

        public bool WaitingForReply(Guid messageId)
        {
            return sentMessageGuids.ContainsKey(messageId);
        }

        private async void WebSocket_StateChange(object sender, WebSocketState state)
        {
            State = state;
            if (state != WebSocketState.Open)
            {
                ReceiveTextMessage -= WebSocket_NewTextMessage;
                return;
            }
            sentMessageGuids.Clear();
            heartBeatCheck.Elapsed += HeartBeatTimer_Elapsed;
            ReceiveTextMessage += WebSocket_NewTextMessage;
            try
            {
                _ = StartMessageReceiveLoop();
            }
            catch (Exception)
            {
                Debugger.Break();
                throw;
            }

            await OBS_GetAuthRequired().ConfigureAwait(true);
        }

        private void HeartBeatTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            heartBeatCheck.Elapsed -= HeartBeatTimer_Elapsed;
            heartBeatCheck.Stop();
            if (AutoReconnect)
            {
                context.Send(
                    async (x) => await ReconnectAsync().ConfigureAwait(false)
                    , null);
            }
            else
            {
                context.Send(
                    async (x) => await DisconnectAsync(false).ConfigureAwait(false)
                , null);
            }
        }

        public async ValueTask<Guid> ObsSend(object message)
        {
            if (message == null) { throw new ArgumentNullException(nameof(message)); }

            ObsRequestMetadata metadata = new ObsRequestMetadata()
            {
                RequestGuid = (message as RequestBase).MessageId,
                OriginalRequestType = (message as RequestBase).RequestType,
                OriginalRequestData = message
            };
            sentMessageGuids.Add(metadata.RequestGuid, metadata);
            await SendMessageAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message))).ConfigureAwait(false);
            return metadata.RequestGuid;
        }

        private async ValueTask<Guid> OBS_GetAuthRequired()
        {
            GetAuthRequiredRequest message = new GetAuthRequiredRequest();
            return await ObsSend(message).ConfigureAwait(false);
        }

        private async ValueTask<Guid> OBS_EnableHeartBeat()
        {
            SetHeartbeatRequest message = new SetHeartbeatRequest()
            {
                Enable = true
            };
            return await ObsSend(message).ConfigureAwait(false);
        }

        private void PopulateAuthenticateRequest(ref AuthenticateRequest authenticateRequest, GetAuthRequiredReply getAuth)
        {
            authenticateRequest.Auth = new String(SecurePreferences.CreateAuthResponse(PasswordPreference, getAuth.Salt, getAuth.Challenge, null));
        }

        private void FurtherProcessObsEvent(object sender, ObsEventObject obsEvent)
        {
            switch (obsEvent.EventType)
            {
                case ObsEventType.Heartbeat:
                    heartBeatCheck.Enabled = (obsEvent.MessageObject as HeartbeatObsEvent).Pulse;
                    heartBeatCheck.Enabled = true;
                    break;
                default: break;
            }
        }

        private async void WebSocket_NewTextMessage(object sender, MemoryStream message)
        {
            await OBS_ParseJson(message).ConfigureAwait(false);
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

        private async Task OBS_ParseJson(MemoryStream message)
        {
            using JsonDocument document = GetJsonDocumentFromMemoryStream(message);
            JsonElement root = document.RootElement;

            JsonElement sourceType;

            if (root.TryGetProperty("message-id", out JsonElement messageIdJson))
            {
                bool sentMessageGuidExists = false;
                ObsRequestMetadata requestMetadata = null;
                if (Guid.TryParse(messageIdJson.GetString(), out Guid guid))
                {
                    sentMessageGuidExists = sentMessageGuids.TryGetValue(guid, out requestMetadata);
                }
                root.TryGetProperty("status", out JsonElement statusJson);
                if (sentMessageGuidExists)
                {
                    ObsReplyObject obsReply = new ObsReplyObject()
                    {
                        MessageId = guid,
                        Status = statusJson.GetString()
                    };
                    if (sentMessageGuidExists)
                    {
                        obsReply.RequestMetadata = requestMetadata;
                        sentMessageGuids.Remove(guid);
                    }
                    if (Enum.TryParse(requestMetadata.OriginalRequestType.ToString(), out ObsRequestType reqType))
                    {
                        obsReply.RequestType = reqType;
                    }

                    switch (obsReply.RequestType)
                    {
                        case ObsRequestType.GetSourceSettings:
                        case ObsRequestType.SetSourceSettings:
                            root.TryGetProperty("sourceType", out sourceType);
                            if (!String.IsNullOrEmpty(sourceType.ToString()))
                            {
                                obsReply.SourceType = ObsTypes.ObsTypeNameDictionary[sourceType.ToString()];
                            }
                            else
                            {
                                Trace.WriteLine($"Unable to determine source type.");
                            }
                            break;
                        case ObsRequestType.GetSourceFilterInfo:
                            root.TryGetProperty("type", out sourceType);
                            if (!String.IsNullOrEmpty(sourceType.ToString()))
                            {
                                obsReply.SourceType = ObsTypes.ObsTypeNameDictionary[sourceType.ToString()];
                            }
                            else
                            {
                                Trace.WriteLine($"Unable to determine source type.");
                            }
                            break;
                        default:
                            break;
                    }
                    await ParseReply(message, obsReply).ConfigureAwait(false);
                }
                else
                {
                    Trace.WriteLine($"message-id {messageIdJson} received, but no matching request found.");
                }
            }
            else if (root.TryGetProperty("update-type", out JsonElement updateTypeJson))
            {
                //Trace.WriteLine($"Received a message of type {updateTypeJson}.");
                bool isStreaming = root.TryGetProperty("stream-timecode", out JsonElement jsonStreamTimecode);
                bool isRecording = root.TryGetProperty("rec-timecode", out JsonElement jsonRecTimecode);
                ObsEventObject obsEvent = new ObsEventObject();
                if (Enum.TryParse(updateTypeJson.GetString(), out ObsEventType eventType))
                {
                    obsEvent.EventType = eventType;
                }
                switch (obsEvent.EventType)
                {
                    case ObsEventType.SourceCreated:
                        root.TryGetProperty("sourceKind", out sourceType);
                        obsEvent.SourceType = ObsTypes.ObsTypeNameDictionary[sourceType.ToString()];
                        break;
                    case ObsEventType.SourceFilterAdded:
                        root.TryGetProperty("filterType", out sourceType);
                        obsEvent.SourceType = ObsTypes.ObsTypeNameDictionary[sourceType.ToString()];
                        break;
                    default:
                        break;
                }
                await ParseEvent(message, obsEvent).ConfigureAwait(false);
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

        private bool CanDeserializeSourceType(ObsSourceType sourceType, ReadOnlyMemory<char> settingsJson, out object deserialisedObject)
        {
            Type modelType = ObsWsSourceType.GetType(sourceType);
            if (modelType == null)
            {
                JsonException je = new JsonException(String.Format(CultureInfo.CurrentCulture, Properties.Resources.exception_source_type_not_implemented_inner_format, settingsJson));
                NotImplementedException ex = new NotImplementedException(String.Format(CultureInfo.CurrentCulture, Properties.Resources.exception_source_type_not_implemented_format, sourceType), je);
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

        private async Task ParseReply(MemoryStream message, ObsReplyObject obsReply)
        {
            message.Seek(0, SeekOrigin.Begin);
            if (obsReply.Status == "ok" && Enum.IsDefined(typeof(ObsRequestType), obsReply.RequestType))
            {
                obsReply.MessageObject = await JsonSerializer.DeserializeAsync(message, ObsWsRequestReply.GetType(obsReply.RequestType)).ConfigureAwait(false);

                object settingsObject;
                ReadOnlyMemory<char> settingsJson;
                ObsSourceType sourceType;

                switch (obsReply.RequestType)
                {
                    case ObsRequestType.GetAuthRequired:
                        GetAuthRequiredReply getAuthRequiredReply = obsReply.MessageObject as GetAuthRequiredReply;
                        CanSend = !getAuthRequiredReply.AuthRequired;
                        if (!CanSend)
                        {
                            AuthenticateRequest authRequest = new AuthenticateRequest();
                            if (PasswordPreference == null || PasswordPreference.Length == 0)
                            {
                                AutoReconnect = false;
                                OnErrorState(new Exception(rm.GetString("auth_required_no_password", CultureInfo.CurrentCulture)), -1);
                                break;
                            }
                            PopulateAuthenticateRequest(ref authRequest, getAuthRequiredReply);
                            await ObsSend(authRequest).ConfigureAwait(true);
                        }
                        else
                        {
                            await OBS_EnableHeartBeat().ConfigureAwait(true);
                        }
                        break;
                    case ObsRequestType.GetSourceTypesList:
                        foreach (ObsWsReplyType type in (obsReply.MessageObject as GetSourceTypesListReply).Types)
                        {
                            if (!ObsTypes.ObsTypeNameDictionary.ContainsKey(type.TypeId))
                            {
                                Trace.WriteLine($"Unknown source type: {type.DisplayName} ({type.TypeId}) is not defined but the server supports it.");
                                continue;
                            }
                            if (!CanDeserializeSourceType(ObsTypes.ObsTypeNameDictionary[type.TypeId], type.DefaultSettings.GetRawText().AsMemory(), out settingsObject))
                            {
                                Trace.WriteLine($"Unknown source type: {type.DisplayName} ({type.TypeId}) is not defined but the server supports it.");
                                continue;
                            }
                            type.DefaultSettingsObj = settingsObject;
                        }
                        break;
                    case ObsRequestType.GetSourceSettings:
                        GetJsonElementFromObjectProperty(obsReply.MessageObject, "SourceSettings", out settingsJson);
                        if (!CanDeserializeSourceType(obsReply.SourceType, settingsJson, out settingsObject)) { break; }
                        (obsReply.MessageObject as GetSourceSettingsReply).SourceSettingsObj = settingsObject;
                        break;
                    case ObsRequestType.SetSourceSettings:
                        GetJsonElementFromObjectProperty(obsReply.MessageObject, "SourceSettings", out settingsJson);
                        if (!CanDeserializeSourceType(obsReply.SourceType, settingsJson, out settingsObject)) { break; }
                        (obsReply.MessageObject as SetSourceSettingsReply).SourceSettingsObj = settingsObject;
                        break;
                    case ObsRequestType.GetSourceFilters:
                        foreach (ObsWsReplyFilter filter in (obsReply.MessageObject as GetSourceFiltersReply).Filters)
                        {
                            settingsJson = filter.Settings.GetRawText().AsMemory();
                            sourceType = ObsTypes.ObsTypeNameDictionary[filter.Type];
                            if (!CanDeserializeSourceType(sourceType, settingsJson, out settingsObject)) { continue; }
                            filter.SettingsObj = settingsObject;
                        }
                        break;
                    case ObsRequestType.GetSourceFilterInfo:
                        GetJsonElementFromObjectProperty(obsReply.MessageObject, "Settings", out settingsJson);
                        if (!CanDeserializeSourceType(obsReply.SourceType, settingsJson, out settingsObject)) { break; }
                        (obsReply.MessageObject as GetSourceFilterInfoReply).SettingsObj = settingsObject;
                        break;
                    case ObsRequestType.Authenticate:
                        CanSend = true;
                        await OBS_EnableHeartBeat().ConfigureAwait(true);
                        break;
                    default:
                        break;
                }

                NewObsReply(obsReply);
            }
            else if (obsReply.Status == "error")
            {
                ObsError replyModel = (ObsError)await JsonSerializer.DeserializeAsync(message, typeof(ObsError)).ConfigureAwait(false);
                if (obsReply.RequestType == ObsRequestType.Authenticate && !CanSend)
                {
                    AutoReconnect = false;
                }
                WsClientErrorMessage errorMessage = new WsClientErrorMessage()
                {
                    Error = new Exception(
                        replyModel.Error,
                        new Exception(String.Format(CultureInfo.CurrentCulture, Properties.Resources.exception_obs_error_format, obsReply.RequestMetadata.OriginalRequestType, obsReply.MessageId, obsReply.Status))
                        ),
                    ReconnectDelay = -1
                };
                OnErrorState(errorMessage.Error, errorMessage.ReconnectDelay);
            }
        }

        private async Task ParseEvent(MemoryStream message, ObsEventObject obsEvent)
        {
            message.Seek(0, SeekOrigin.Begin);
            if (obsEvent.EventType == ObsEventType.Unknown)
            {
                Trace.WriteLine($"EventType is Unknown, printing JSON...");
                using StreamReader sr = new StreamReader(message);
                Trace.WriteLine(sr.ReadToEnd());
                Debugger.Break();
            }
            obsEvent.MessageObject = await JsonSerializer.DeserializeAsync(message, ObsWsEvent.GetType(obsEvent.EventType)).ConfigureAwait(false);

            object settingsObject;
            ReadOnlyMemory<char> settingsJson;

            switch (obsEvent.EventType)
            {
                case ObsEventType.SourceCreated:
                    GetJsonElementFromObjectProperty(obsEvent.MessageObject, "SourceSettings", out settingsJson);
                    if (!CanDeserializeSourceType(obsEvent.SourceType, settingsJson, out settingsObject)) { Trace.WriteLine($"{obsEvent.EventType} ({obsEvent.SourceType}) -> {settingsJson}"); break; }
                    (obsEvent.MessageObject as SourceCreatedObsEvent).SourceSettingsObj = settingsObject;
                    break;
                case ObsEventType.SourceFilterAdded:
                    GetJsonElementFromObjectProperty(obsEvent.MessageObject, "FilterSettings", out settingsJson);
                    if (!CanDeserializeSourceType(obsEvent.SourceType, settingsJson, out settingsObject)) { Trace.WriteLine($"{obsEvent.EventType} ({obsEvent.SourceType}) -> {settingsJson}"); break; }
                    (obsEvent.MessageObject as SourceFilterAddedObsEvent).FilterSettingsObj = settingsObject;
                    break;
            }
            NewObsEvent(obsEvent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposedValue)
            {
                return;
            }

            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                StateChange -= WebSocket_StateChange;
                OnObsEvent -= FurtherProcessObsEvent;
                heartBeatCheck.Stop();
                heartBeatCheck.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
            base.Dispose(disposing);
        }
    }
}
