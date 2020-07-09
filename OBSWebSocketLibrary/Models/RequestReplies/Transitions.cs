using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequestReplies
{
    public class GetTransitionListReply : ReplyBase
    {
        [JsonPropertyName("current-transition")]
        public string CurrentTransition { get; set; }
        [JsonPropertyName("transitions")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TypeDefs.ObsWsTransitionName> Transitions { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class GetCurrentTransitionReply : ReplyBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("duration")]
        public int Duration { get; set; }
    }

    public class SetCurrentTransitionReply : ReplyBase
    {
    }

    public class SetTransitionDurationReply : ReplyBase
    {
    }

    public class GetTransitionDurationReply : ReplyBase
    {
        [JsonPropertyName("transition-duration")]
        public int Duration { get; set; }
    }

    public class GetTransitionPositionReply : ReplyBase
    {
        [JsonPropertyName("position")]
        public double Position { get; set; }
    }
}
