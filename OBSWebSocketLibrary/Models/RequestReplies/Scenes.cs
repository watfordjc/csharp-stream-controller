using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequestReplies
{
    public class SetCurrentSceneReply : ReplyBase
    {
    }

    public class GetCurrentSceneReply : ReplyBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("sources")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TypeDefs.ObsWsSceneItem> Sources { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class GetSceneListReply : ReplyBase
    {
        [JsonPropertyName("current-scene")]
        public string CurrentScene { get; set; }
        [JsonPropertyName("scenes")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TypeDefs.ObsScene> Scenes { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class ReorderSceneItemsReply : ReplyBase
    {
    }

    public class SetSceneTransitionOverrideReply : ReplyBase
    {
    }

    public class RemoveSceneTransitionOverrideReply : ReplyBase
    {
    }

    public class GetSceneTransitionOverrideReply : ReplyBase
    {
        [JsonPropertyName("transitionName")]
        public string TransitionName { get; set; }
        [JsonPropertyName("transitionDuration")]
        public int TransitionDuration { get; set; }
    }
}
