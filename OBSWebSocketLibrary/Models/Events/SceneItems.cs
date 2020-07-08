﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.Events
{
    public class SourceOrderChanged : EventBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("scene-items")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TypeDefs.ObsEventSceneItem> SceneItems { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class SceneItemAdded : EventBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("item-name")]
        public string ItemName { get; set; }
        [JsonPropertyName("item-id")]
        public int ItemId { get; set; }
    }

    public class SceneItemRemoved : EventBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("item-name")]
        public string ItemName { get; set; }
        [JsonPropertyName("item-id")]
        public int ItemId { get; set; }
    }

    public class SceneItemVisibilityChanged : EventBase
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

    public class SceneItemLockChanged : EventBase
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

    public class SceneItemTransformChanged : EventBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("item-name")]
        public string ItemName { get; set; }
        [JsonPropertyName("item-id")]
        public int ItemId { get; set; }
        [JsonPropertyName("transform")]
        public TypeDefs.SceneItemTransform Transform { get; set; }
    }

    public class SceneItemSelected : EventBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("item-name")]
        public string ItemName { get; set; }
        [JsonPropertyName("item-id")]
        public int ItemId { get; set; }
    }

    public class SceneItemDeselected : EventBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("item-name")]
        public string ItemName { get; set; }
        [JsonPropertyName("item-id")]
        public int ItemId { get; set; }
    }
}
