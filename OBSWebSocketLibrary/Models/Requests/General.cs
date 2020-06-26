using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.Requests
{
    public class GetVersion : RequestBase
    {
    }

    public class GetAuthRequired : RequestBase
    {
    }

    public class Authenticate : RequestBase
    {
        [JsonPropertyName("auth")]
        public string Auth { get; set; }
    }

    public class SetHeartbeat : RequestBase
    {
        [JsonPropertyName("enable")]
        public bool Enable { get; set; }
    }

    public class SetFilenameFormatting : RequestBase
    {
    }

    public class GetFilenameFormatting : RequestBase
    {
    }

    public class GetStats : RequestBase
    {
    }

    public class BroadcastCustomMessage : RequestBase
    {
        [JsonPropertyName("realm")]
        public string Realm { get; set; }
        [JsonPropertyName("data")]
        public JsonElement Data { get; set; }
    }

    public class GetVideoInfo : RequestBase
    {
    }

    public class OpenProjector : RequestBase
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("monitor")]
        public int Monitor { get; set; }
        [JsonPropertyName("geometry")]
        public string Geometry { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
