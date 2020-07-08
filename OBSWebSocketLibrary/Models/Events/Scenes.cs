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
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TypeDefs.SceneItem> Sources { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
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
