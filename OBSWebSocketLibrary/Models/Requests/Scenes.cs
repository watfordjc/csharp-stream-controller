using OBSWebSocketLibrary.Models.TypeDefs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.Requests
{
    public class SetCurrentScene : RequestBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
    }

    public class GetCurrentScene : RequestBase
    {
    }

    public class GetSceneList : RequestBase
    {
    }

    public class ReorderSceneItems : RequestBase
    {
        [JsonPropertyName("scene")]
        public string Scene { get; set; }
        [JsonPropertyName("items")]
        public IList<SceneItem> Items { get; set; }
        public class SceneItem
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }
    }

    public class SetSceneTransitionOverride : RequestBase
    {
        [JsonPropertyName("sceneName")]
        public string SceneName { get; set; }
        [JsonPropertyName("transitionName")]
        public string TransitionName { get; set; }
        [JsonPropertyName("transitionDuration")]
        public int TransitionDuration { get; set; }
    }

    public class RemoveSceneTransitionOverride : RequestBase
    {
        [JsonPropertyName("sceneName")]
        public string SceneName { get; set; }
    }

    public class GetSceneTransitionOverride : RequestBase
    {
        [JsonPropertyName("sceneName")]
        public string SceneName { get; set; }
    }
}
