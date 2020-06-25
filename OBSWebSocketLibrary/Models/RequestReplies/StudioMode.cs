using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.RequestReplies
{
    public class GetStudioModeStatus : RequestBase
    {
    }

    public class GetPreviewScene : RequestBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("sources")]
        public TypeDefs.SceneItem[] Sources { get; set; }
    }

    public class SetPreviewScene : RequestBase
    {
    }

    public class TransitionToProgram : RequestBase
    {
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
