using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequestReplies
{
    public class GetStudioModeStatusReply : ReplyBase
    {
    }

    public class GetPreviewSceneReply : ReplyBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("sources")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TypeDefs.ObsWsSceneItem> Sources { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class SetPreviewSceneReply : ReplyBase
    {
    }

    public class TransitionToProgramReply : ReplyBase
    {
    }

    public class EnableStudioModeReply : ReplyBase
    {
    }

    public class DisableStudioModeReply : ReplyBase
    {
    }

    public class ToggleStudioModeReply : ReplyBase
    {
    }
}
