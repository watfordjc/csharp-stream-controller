using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.Requests
{
    public class GetStreamingStatus : RequestBase
    {
    }

    public class StartStopStreaming : RequestBase
    {
    }

    public class StartStreaming : RequestBase
    {
        [JsonPropertyName("stream")]
        public TypeDefs.ObsStream Stream { get; set; }
    }

    public class StopStreaming : RequestBase
    {
    }

    public class SetStreamSettings : RequestBase
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("settings")]
        public TypeDefs.ObsStreamSettings Settings { get; set; }
        [JsonPropertyName("save")]
        public bool Save { get; set; }
    }

    public class GetStreamSettings : RequestBase
    {
    }

    public class SaveStreamSettings : RequestBase
    {
    }

    public class SendCaptions : RequestBase
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
