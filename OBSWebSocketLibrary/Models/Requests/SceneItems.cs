using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequests
{
    public class GetSceneItemPropertiesRequest : RequestBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("item")]
        public object Item { get; set; }
    }

    public class SetSceneItemPropertiesRequest : RequestBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("item")]
        public string Item { get; set; }
        [JsonPropertyName("item")]
        public TypeDefs.ObsWsItemObject ItemObj { get; set; }
        [JsonPropertyName("position")]
        public TypeDefs.ObsWsPositionProperties Position { get; set; }
        [JsonPropertyName("rotation")]
        public double Rotation { get; set; }
        [JsonPropertyName("scale")]
        public TypeDefs.ObsWsCoordinates Scale { get; set; }
        [JsonPropertyName("crop")]
        public TypeDefs.ObsWsDirections Crop { get; set; }
        [JsonPropertyName("visible")]
        public bool Visible { get; set; }
        [JsonPropertyName("locked")]
        public bool Locked { get; set; }
        [JsonPropertyName("bounds")]
        public TypeDefs.ObsWsBoundsProperties Bounds { get; set; }
    }

    public class ResetSceneItemRequest : RequestBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("item")]
        public string Item { get; set; }
        [JsonPropertyName("item")]
        public TypeDefs.ObsWsItemObject ItemObj { get; set; }
    }

    public class SetSceneItemRenderRequest : RequestBase
    {
        [JsonPropertyName("scene-name")]
        public string SceneName { get; set; }
        [JsonPropertyName("source")]
        public string Source { get; set; }
        [JsonPropertyName("render")]
        public bool Render { get; set; }
    }

    public class SetSceneItemPositionRequest : RequestBase
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

    public class SetSceneItemTransformRequest : RequestBase
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

    public class SetSceneItemCropRequest : RequestBase
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

    public class DeleteSceneItemRequest : RequestBase
    {
        [JsonPropertyName("scene")]
        public string Scene { get; set; }
        [JsonPropertyName("item")]
        public TypeDefs.ObsWsItemObject ItemObj { get; set; }
    }

    public class DuplicateSceneItemRequest : RequestBase
    {
        [JsonPropertyName("fromScene")]
        public string FromScene { get; set; }
        [JsonPropertyName("toScene")]
        public string ToScene { get; set; }
        [JsonPropertyName("item")]
        public TypeDefs.ObsWsItemObject ItemObj { get; set; }
    }
}
