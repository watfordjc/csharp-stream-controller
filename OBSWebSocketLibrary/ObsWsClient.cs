using System;
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
            await SendMessageAsync(JsonSerializer.Serialize(jsonDictionary));
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

        private void WebSocket_NewTextMessage(object sender, string message)
        {
            OBS_ParseJson(message);
        }

        private async void OBS_ParseJson(string message)
        {
            using JsonDocument document = JsonDocument.Parse(message);
            JsonElement root = document.RootElement;
            bool hasGuid = root.TryGetProperty("message-id", out JsonElement requestTypeJson);
            bool hasUpdateType = root.TryGetProperty("update-type", out JsonElement jsonUpdateType);
            // TODO: Handle responses to sent messages
            if (hasGuid)
            {
                Guid guid = Guid.Parse(requestTypeJson.GetString());
                if (sentMessageGuids.TryGetValue(guid, out string requestType))
                {
                    Trace.WriteLine($"Received response to message of type {requestType} with GUID {guid}.");
                    sentMessageGuids.Remove(guid);
                }
                Enum.TryParse(requestType, out Data.Requests reqType);
                switch (reqType)
                {
                    case Data.Requests.SetHeartbeat:
                        root.TryGetProperty("status", out JsonElement status);
                        Trace.WriteLine($"Server response to enabling HeartBeat: {status.GetString()}");
                        break;
                    case Data.Requests.GetSourcesList:
                        Models.GetSourcesListReply reply = JsonSerializer.Deserialize<Models.GetSourcesListReply>(message);
                        foreach (Models.Source device in reply.Sources)
                        {
                            OBS_GetSourceSettings(device.Name);
                        }
                        break;
                }
                return;
            }
            else if (!hasUpdateType)
            {
                Trace.WriteLine("Unexpected JSON.");
                return;
            }
            // Handle responses with an update-type

            Trace.WriteLine($"Received a message of type {jsonUpdateType}.");
            bool isStreaming = root.TryGetProperty("stream-timecode", out JsonElement jsonStreamTimecode);
            bool isRecording = root.TryGetProperty("rec-timecode", out JsonElement jsonRecTimecode);
            Enum.TryParse(jsonUpdateType.GetString(), out Data.Events eventType);

            switch (eventType)
            {
                case Data.Events.Heartbeat:
                    root.TryGetProperty("pulse", out JsonElement pulse);
                    heartBeatCheck.Enabled = pulse.GetBoolean();
                    heartBeatCheck.Enabled = true;
                    break;
                case Data.Events.Exiting:
                    await DisconnectAsync();
                    await Task.Delay(10000);
                    if (AutoReconnect)
                    {
                        await AutoReconnectConnectAsync();
                    }
                    break;
            }
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