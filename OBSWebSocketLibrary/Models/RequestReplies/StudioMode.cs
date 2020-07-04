using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.RequestReplies
{
    public class GetStudioModeStatus : RequestReplyBase
    {
    }

    public class GetPreviewScene : RequestReplyBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("sources")]
        public IList<TypeDefs.SceneItem> Sources { get; set; }
    }

    public class SetPreviewScene : RequestReplyBase
    {
    }

    public class TransitionToProgram : RequestReplyBase
    {
    }

    public class EnableStudioMode : RequestReplyBase
    {
    }

    public class DisableStudioMode : RequestReplyBase
    {
    }

    public class ToggleStudioMode : RequestReplyBase
    {
    }
}
