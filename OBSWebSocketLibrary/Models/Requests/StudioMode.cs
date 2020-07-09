using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequests
{
    public class GetStudioModeStatusRequest : RequestBase
    {
    }

    public class GetPreviewSceneRequest : RequestBase
    {
    }

    public class SetPreviewSceneRequest : RequestBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
    }

    public class TransitionToProgramRequest : RequestBase
    {
        [JsonPropertyName("with-transition")]
        public TypeDefs.ObsWsRequestTransition WithTransition { get; set; }
    }

    public class EnableStudioModeRequest : RequestBase
    {
    }

    public class DisableStudioModeRequest : RequestBase
    {
    }

    public class ToggleStudioModeRequest : RequestBase
    {
    }
}
