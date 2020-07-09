using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsEvents
{
    public class SwitchScenesObsEvent : EventBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("sources")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TypeDefs.ObsWsSceneItem> Sources { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class ScenesChangedObsEvent : EventBase
    {
    }

    public class SceneCollectionChangedObsEvent : EventBase
    {
    }

    public class SceneCollectionListChangedObsEvent : EventBase
    {
    }
}
