using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.RequestReplies
{
    public class SetCurrentScene : RequestBase
    {
    }

    public class GetCurrentScene : RequestBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("sources")]
        public TypeDefs.SceneItem[] Sources { get; set; }
    }

    public class GetSceneList : RequestBase
    {
        [JsonPropertyName("current-scene")]
        public string CurrentScene { get; set; }
        [JsonPropertyName("scenes")]
        public TypeDefs.Scene[] Scenes { get; set; }
    }

    public class ReorderSceneItems : RequestBase
    {
    }

    public class SetSceneTransitionOverride : RequestBase
    {
    }

    public class RemoveSceneTransitionOverride : RequestBase
    {
    }

    public class GetSceneTransitionOverride : RequestBase
    {
        [JsonPropertyName("transitionName")]
        public string TransitionName { get; set; }
        [JsonPropertyName("transitionDuration")]
        public int TransitionDuration { get; set; }
    }
}
