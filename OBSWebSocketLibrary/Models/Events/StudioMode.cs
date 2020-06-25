using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.Events
{
    public class PreviewSceneChanged : EventBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("GetCurrentScene")]
        public TypeDefs.SceneItem[] GetCurrentScene { get; set; }
    }

    public class StudioModeSwitched : EventBase
    {
        [JsonPropertyName("new-state")]
        public bool NewState { get; set; }
    }
}
