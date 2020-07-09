using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequests
{
    public class GetVersionRequest : RequestBase
    {
    }

    public class GetAuthRequiredRequest : RequestBase
    {
    }

    public class AuthenticateRequest : RequestBase
    {
        [JsonPropertyName("auth")]
        public string Auth { get; set; }
    }

    public class SetHeartbeatRequest : RequestBase
    {
        [JsonPropertyName("enable")]
        public bool Enable { get; set; }
    }

    public class SetFilenameFormattingRequest : RequestBase
    {
    }

    public class GetFilenameFormattingRequest : RequestBase
    {
    }

    public class GetStatsRequest : RequestBase
    {
    }

    public class BroadcastCustomMessageRequest : RequestBase
    {
        [JsonPropertyName("realm")]
        public string Realm { get; set; }
        [JsonPropertyName("data")]
        public object Data { get; set; }
    }

    public class GetVideoInfoRequest : RequestBase
    {
    }

    public class OpenProjectorRequest : RequestBase
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
