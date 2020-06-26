using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.Requests
{
    public class GetStudioModeStatus : RequestBase
    {
    }

    public class GetPreviewScene : RequestBase
    {
    }

    public class SetPreviewScene : RequestBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
    }

    public class TransitionToProgram : RequestBase
    {
        [JsonPropertyName("with-transition")]
        public Transition WithTransition { get; set; }
        public class Transition
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("duration")]
            public int Duration { get; set; }
        }
    }

    public class EnableStudioMode : RequestBase
    {
    }

    public class DisableStudioMode : RequestBase
    {
    }

    public class ToggleStudioMode : RequestBase
    {
    }
}
