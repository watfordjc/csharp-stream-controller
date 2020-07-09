using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsEvents
{
    public class SwitchTransitionObsEvent : EventBase
    {
        [JsonPropertyName("transition-name")]
        public string TransitionName { get; set; }
    }

    public class TransitionListChangedObsEvent : EventBase
    {
    }

    public class TransitionDurationChangedObsEvent : EventBase
    {
        [JsonPropertyName("new-duration")]
        public int NewDuration { get; set; }
    }

    public class TransitionBeginObsEvent : EventBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("duration")]
        public int Duration { get; set; }
        [JsonPropertyName("from-scene")]
        public string FromScene { get; set; }
        [JsonPropertyName("to-scene")]
        public string ToScene { get; set; }
    }

    public class TransitionEndObsEvent : EventBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("duration")]
        public int Duration { get; set; }
        [JsonPropertyName("to-scene")]
        public string ToScene { get; set; }
    }

    public class TransitionVideoEndObsEvent : EventBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("duration")]
        public int Duration { get; set; }
        [JsonPropertyName("from-scene")]
        public string FromScene { get; set; }
        [JsonPropertyName("to-scene")]
        public string ToScene { get; set; }
    }
}
