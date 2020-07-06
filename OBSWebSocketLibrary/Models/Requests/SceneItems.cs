using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.Requests
{
    public class GetSceneItemProperties : RequestBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("item")]
        public object Item { get; set; }
    }

    public class SetSceneItemProperties : RequestBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("item")]
        public string Item { get; set; }
        [JsonPropertyName("item")]
        public TypeDefs.ItemObject ItemObj { get; set; }
        [JsonPropertyName("position")]
        public TypeDefs.PositionProperties Position { get; set; }
        [JsonPropertyName("rotation")]
        public double Rotation { get; set; }
        [JsonPropertyName("scale")]
        public TypeDefs.Coordinates Scale { get; set; }
        [JsonPropertyName("crop")]
        public TypeDefs.Directions Crop { get; set; }
        [JsonPropertyName("visible")]
        public bool Visible { get; set; }
        [JsonPropertyName("locked")]
        public bool Locked { get; set; }
        [JsonPropertyName("bounds")]
        public TypeDefs.BoundsProperties Bounds { get; set; }
    }

    public class ResetSceneItem : RequestBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("item")]
        public string Item { get; set; }
        [JsonPropertyName("item")]
        public TypeDefs.ItemObject ItemObj { get; set; }
    }

    public class SetSceneItemRender : RequestBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("source")]
        public string Source { get; set; }
        [JsonPropertyName("render")]
        public bool Render { get; set; }
    }

    public class SetSceneItemPosition : RequestBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("item")]
        public string Item { get; set; }
        [JsonPropertyName("x")]
        public double X { get; set; }
        [JsonPropertyName("y")]
        public double Y { get; set; }
    }

    public class SetSceneItemTransform : RequestBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("item")]
        public string Item { get; set; }
        [JsonPropertyName("x-scale")]
        public double XScale { get; set; }
        [JsonPropertyName("y-scale")]
        public double YScale { get; set; }
        [JsonPropertyName("rotation")]
        public double Rotation { get; set; }
    }

    public class SetSceneItemCrop : RequestBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("item")]
        public string Item { get; set; }
        [JsonPropertyName("top")]
        public int Top { get; set; }
        [JsonPropertyName("bottom")]
        public int Bottom { get; set; }
        [JsonPropertyName("left")]
        public int Left { get; set; }
        [JsonPropertyName("right")]
        public int Right { get; set; }
    }

    public class DeleteSceneItem : RequestBase
    {
        [JsonPropertyName("scene")]
        public string Scene { get; set; }
        [JsonPropertyName("item")]
        public TypeDefs.ItemObject ItemObj { get; set; }
    }

    public class DuplicateSceneItem : RequestBase
    {
        [JsonPropertyName("fromScene")]
        public string FromScene { get; set; }
        [JsonPropertyName("toScene")]
        public string ToScene { get; set; }
        [JsonPropertyName("item")]
        public TypeDefs.ItemObject ItemObj { get; set; }
    }
}
