using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.Requests
{
    public class GetTransitionList : RequestBase
    {
    }

    public class GetCurrentTransition : RequestBase
    {
    }

    public class SetCurrentTransition : RequestBase
    {
        [JsonPropertyName("transition-name")]
        public string TransitionName { get; set; }
    }

    public class SetTransitionDuration : RequestBase
    {
        [JsonPropertyName("duration")]
        public int Duration { get; set; }
    }

    public class GetTransitionDuration : RequestBase
    {
    }

    public class GetTransitionPosition : RequestBase
    {
    }
}
