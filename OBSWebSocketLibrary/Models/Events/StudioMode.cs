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
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TypeDefs.SceneItem> GetCurrentScene { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class StudioModeSwitched : EventBase
    {
        [JsonPropertyName("new-state")]
        public bool NewState { get; set; }
    }
}
