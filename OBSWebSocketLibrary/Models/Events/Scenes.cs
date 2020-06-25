using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.Events
{
    public class SwitchScenes : EventBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("sources")]
        public TypeDefs.SceneItem[] Sources { get; set; }
    }

    public class ScenesChanged : EventBase
    {
    }

    public class SceneCollectionChanged : EventBase
    {
    }

    public class SceneCollectionListChanged : EventBase
    {
    }
}
