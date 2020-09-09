using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsEvents
{
    public class MediaPlayingObsEvent : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("sourceKind")]
        public string SourceKind { get; set; }
    }

    public class MediaPausedObsEvent : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("sourceKind")]
        public string SourceKind { get; set; }
    }

    public class MediaRestartedObsEvent : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("sourceKind")]
        public string SourceKind { get; set; }
    }

    public class MediaStoppedObsEvent : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("sourceKind")]
        public string SourceKind { get; set; }
    }

    public class MediaNextObsEvent : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("sourceKind")]
        public string SourceKind { get; set; }
    }

    public class MediaPreviousObsEvent : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("sourceKind")]
        public string SourceKind { get; set; }
    }

    public class MediaStartedObsEvent : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("sourceKind")]
        public string SourceKind { get; set; }
    }

    public class MediaEndedObsEvent : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("sourceKind")]
        public string SourceKind { get; set; }
    }
}
