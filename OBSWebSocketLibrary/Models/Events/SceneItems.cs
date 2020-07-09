using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsEvents
{
    public class SourceOrderChangedObsEvent : EventBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("scene-items")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TypeDefs.ObsWsEventSceneItem> SceneItems { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class SceneItemAddedObsEvent : EventBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("item-name")]
        public string ItemName { get; set; }
        [JsonPropertyName("item-id")]
        public int ItemId { get; set; }
    }

    public class SceneItemRemovedObsEvent : EventBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("item-name")]
        public string ItemName { get; set; }
        [JsonPropertyName("item-id")]
        public int ItemId { get; set; }
    }

    public class SceneItemVisibilityChangedObsEvent : EventBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("item-name")]
        public string ItemName { get; set; }
        [JsonPropertyName("item-id")]
        public int ItemId { get; set; }
        [JsonPropertyName("item-visible")]
        public bool ItemVisible { get; set; }
    }

    public class SceneItemLockChangedObsEvent : EventBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("item-name")]
        public string ItemName { get; set; }
        [JsonPropertyName("item-id")]
        public int ItemId { get; set; }
        [JsonPropertyName("item-locked")]
        public bool ItemLocked { get; set; }
    }

    public class SceneItemTransformChangedObsEvent : EventBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("item-name")]
        public string ItemName { get; set; }
        [JsonPropertyName("item-id")]
        public int ItemId { get; set; }
        [JsonPropertyName("transform")]
        public TypeDefs.ObsSceneItemTransform Transform { get; set; }
    }

    public class SceneItemSelectedObsEvent : EventBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("item-name")]
        public string ItemName { get; set; }
        [JsonPropertyName("item-id")]
        public int ItemId { get; set; }
    }

    public class SceneItemDeselectedObsEvent : EventBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("item-name")]
        public string ItemName { get; set; }
        [JsonPropertyName("item-id")]
        public int ItemId { get; set; }
    }
}
