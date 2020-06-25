using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.Events
{
    public class SwitchTransition : EventBase
    {
        [JsonPropertyName("transition-name")]
        public string TransitionName { get; set; }
    }

    public class TransitionListChanged : EventBase
    {
    }

    public class TransitionDurationChanged : EventBase
    {
        [JsonPropertyName("new-duration")]
        public int NewDuration { get; set; }
    }

    public class TransitionBegin : EventBase
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

    public class TransitionEnd : EventBase
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

    public class TransitionVideoEnd : EventBase
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
