using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.RequestReplies
{
    public class GetSceneItemProperties : RequestReplyBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("itemId")]
        public int ItemId { get; set; }
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
        [JsonPropertyName("crop.right")]
        public int CropRight { get; set; }
        [JsonPropertyName("crop.bottom")]
        public int CropBottom { get; set; }
        [JsonPropertyName("crop.left")]
        public int CropLeft { get; set; }
        [JsonPropertyName("visible")]
        public bool Visible { get; set; }
        [JsonPropertyName("muted")]
        public bool Muted { get; set; }
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
        [JsonPropertyName("sourceWidth")]
        public int SourceWidth { get; set; }
        [JsonPropertyName("sourceHeight")]
        public int SourceHeight { get; set; }
        [JsonPropertyName("width")]
        public double Width { get; set; }
        [JsonPropertyName("height")]
        public double Height { get; set; }
        [JsonPropertyName("alignment")]
        public int Alignment { get; set; }
        [JsonPropertyName("parentGroupName")]
        public string ParentGroupName { get; set; }
        [JsonPropertyName("groupChildren")]
        public TypeDefs.SceneItemTransform[] GroupChildren { get; set; }
    }

    public class SetSceneItemProperties : RequestReplyBase
    {
    }

    public class ResetSceneItem : RequestReplyBase
    {
    }

    public class SetSceneItemRender : RequestReplyBase
    {
    }

    public class SetSceneItemPosition : RequestReplyBase
    {
    }

    public class SetSceneItemTransform : RequestReplyBase
    {
    }

    public class SetSceneItemCrop : RequestReplyBase
    {
    }

    public class DeleteSceneItem : RequestReplyBase
    {
    }

    public class DuplicateSceneItem : RequestReplyBase
    {
        [JsonPropertyName("scene")]
        public string Scene { get; set; }
        [JsonPropertyName("item")]
        public SceneItem Item { get; set; }
        public class SceneItem
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }
    }
}