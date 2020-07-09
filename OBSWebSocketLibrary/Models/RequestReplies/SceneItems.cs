using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequestReplies
{
    public class GetSceneItemPropertiesReply : ReplyBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("itemId")]
        public int ItemId { get; set; }
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
        [JsonPropertyName("muted")]
        public bool Muted { get; set; }
        [JsonPropertyName("locked")]
        public bool Locked { get; set; }
        [JsonPropertyName("bounds")]
        public TypeDefs.ObsWsBoundsProperties Bounds { get; set; }
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
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TypeDefs.ObsSceneItemTransform> GroupChildren { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class SetSceneItemPropertiesReply : ReplyBase
    {
    }

    public class ResetSceneItemReply : ReplyBase
    {
    }

    public class SetSceneItemRenderReply : ReplyBase
    {
    }

    public class SetSceneItemPositionReply : ReplyBase
    {
    }

    public class SetSceneItemTransformReply : ReplyBase
    {
    }

    public class SetSceneItemCropReply : ReplyBase
    {
    }

    public class DeleteSceneItemReply : ReplyBase
    {
    }

    public class DuplicateSceneItemReply : ReplyBase
    {
        [JsonPropertyName("scene")]
        public string Scene { get; set; }
        [JsonPropertyName("item")]
        public TypeDefs.ObsWsItemObject Item { get; set; }
    }
}
