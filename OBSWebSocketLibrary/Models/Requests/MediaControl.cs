using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequests
{
    public class PlayPauseMediaRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("playPause")]
        public bool PlayPause { get; set; }
    }

    public class RestartMediaRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
    }

    public class StopMediaRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
    }

    public class NextMediaRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
    }

    public class PreviousMediaRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("playPause")]
        public bool PlayPause { get; set; }
    }

    public class GetMediaDurationRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
    }

    public class GetMediaTimeRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
    }

    public class SetMediaTimeRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("timestamp")]
        public int Timestamp { get; set; }
    }

    public class ScrubMediaRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("timeOffset")]
        public int TimeOffset { get; set; }
    }

    public class GetMediaStateRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
    }
}
