using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequests
{
    public class GetTransitionListRequest : RequestBase
    {
    }

    public class GetCurrentTransitionRequest : RequestBase
    {
    }

    public class SetCurrentTransitionRequest : RequestBase
    {
        [JsonPropertyName("transition-name")]
        public string TransitionName { get; set; }
    }

    public class SetTransitionDurationRequest : RequestBase
    {
        [JsonPropertyName("duration")]
        public int Duration { get; set; }
    }

    public class GetTransitionDurationRequest : RequestBase
    {
    }

    public class GetTransitionPositionRequest : RequestBase
    {
    }
}
