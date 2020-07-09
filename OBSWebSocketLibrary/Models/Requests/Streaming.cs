using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequests
{
    public class GetStreamingStatusRequest : RequestBase
    {
    }

    public class StartStopStreamingRequest : RequestBase
    {
    }

    public class StartStreamingRequest : RequestBase
    {
        [JsonPropertyName("stream")]
        public TypeDefs.ObsWsStream Stream { get; set; }
    }

    public class StopStreamingRequest : RequestBase
    {
    }

    public class SetStreamSettingsRequest : RequestBase
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("settings")]
        public TypeDefs.ObsWsStreamSettings Settings { get; set; }
        [JsonPropertyName("save")]
        public bool Save { get; set; }
    }

    public class GetStreamSettingsRequest : RequestBase
    {
    }

    public class SaveStreamSettingsRequest : RequestBase
    {
    }

    public class SendCaptionsRequest : RequestBase
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
