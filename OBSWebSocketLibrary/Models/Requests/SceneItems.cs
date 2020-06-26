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
        public string Item { get; set; }
        [JsonPropertyName("item")]
        public TypeDefs.ItemObject ItemObj { get; set; }
    }

    public class SetSceneItemProperties : RequestBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("item")]
        public string Item { get; set; }
        [JsonPropertyName("item")]
        public TypeDefs.ItemObject ItemObj { get; set; }
        [JsonPropertyName("position.x")]
        public int PositionX { get; set; }
        [JsonPropertyName("position.y")]
        public int PositionY { get; set; }
        [JsonPropertyName("position.alignment")]
        public int PositionAlignment { get; set; }
        [JsonPropertyName("rotation")]
        public double Rotation { get; set; }
        [JsonPropertyName("scale.x")]
        public double ScaleX { get; set; }
        [JsonPropertyName("scale.y")]
        public double ScaleY { get; set; }
        [JsonPropertyName("crop.top")]
        public int CropTop { get; set; }
        [JsonPropertyName("crop.bottom")]
        public int CropBottom { get; set; }
        [JsonPropertyName("crop.left")]
        public int CropLeft { get; set; }
        [JsonPropertyName("crop.right")]
        public int CropRight { get; set; }
        [JsonPropertyName("visible")]
        public bool Visible { get; set; }
        [JsonPropertyName("locked")]
        public bool Locked { get; set; }
        [JsonPropertyName("bounds.type")]
        public string BoundsType { get; set; }
        [JsonPropertyName("bounds.alignment")]
        public int BoundsAlignment { get; set; }
        [JsonPropertyName("bounds.x")]
        public double BoundsX { get; set; }
        [JsonPropertyName("bounds.y")]
        public double BoundsY { get; set; }
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
