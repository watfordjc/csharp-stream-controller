using uk.JohnCook.dotnet.OBSWebSocketLibrary.TypeDefs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequests
{
    public class SetCurrentSceneRequest : RequestBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
    }

    public class GetCurrentSceneRequest : RequestBase
    {
    }

    public class GetSceneListRequest : RequestBase
    {
    }

    public class ReorderSceneItemsRequest : RequestBase
    {
        [JsonPropertyName("scene")]
        public string Scene { get; set; }
        [JsonPropertyName("items")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TypeDefs.ObsWsItemObject> Items { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class SetSceneTransitionOverrideRequest : RequestBase
    {
        [JsonPropertyName("sceneName")]
        public string SceneName { get; set; }
        [JsonPropertyName("transitionName")]
        public string TransitionName { get; set; }
        [JsonPropertyName("transitionDuration")]
        public int TransitionDuration { get; set; }
    }

    public class RemoveSceneTransitionOverrideRequest : RequestBase
    {
        [JsonPropertyName("sceneName")]
        public string SceneName { get; set; }
    }

    public class GetSceneTransitionOverrideRequest : RequestBase
    {
        [JsonPropertyName("sceneName")]
        public string SceneName { get; set; }
    }
}
