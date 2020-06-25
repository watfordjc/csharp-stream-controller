using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.RequestReplies
{
    public class GetTransitionList : RequestBase
    {
        [JsonPropertyName("current-transition")]
        public string CurrentTransition { get; set; }
        [JsonPropertyName("transitions")]
        public Transition[] Transitions { get; set; }
        public class Transition
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }
    }

    public class GetCurrentTransition : RequestBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("duration")]
        public int Duration { get; set; }
    }

    public class SetCurrentTransition : RequestBase
    {
    }

    public class SetTransitionDuration : RequestBase
    {
    }

    public class GetTransitionDuration : RequestBase
    {
        [JsonPropertyName("transition-duration")]
        public int Duration { get; set; }
    }

    public class GetTransitionPosition : RequestBase
    {
        [JsonPropertyName("position")]
        public double Position { get; set; }
    }
}
