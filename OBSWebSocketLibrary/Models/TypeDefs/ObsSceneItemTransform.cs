using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.TypeDefs
{
    public class ObsSceneItemTransform
    {
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
        [JsonPropertyName("sourceWidth")]
        public int SourceWidth { get; set; }
        [JsonPropertyName("sourceHeight")]
        public int SourceHeight { get; set; }
        [JsonPropertyName("width")]
        public double Width { get; set; }
        [JsonPropertyName("height")]
        public double Height { get; set; }
        [JsonPropertyName("parentGroupName")]
        public string ParentGroupName { get; set; }
        [JsonPropertyName("groupChildren")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<ObsSceneItemTransform> GroupChildren { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        public static implicit operator ObsSceneItemTransform(ObsRequestReplies.GetSceneItemPropertiesReply v)
        {
            if (v == null) { return null; }

            return new ObsSceneItemTransform
            {
                Position = new ObsWsPositionProperties()
                {
                    Alignment = v.Position.Alignment,
                    X = v.Position.X,
                    Y = v.Position.Y
                },
                Rotation = v.Rotation,
                Scale = v.Scale,
                Crop = v.Crop,
                Visible = v.Visible,
                Locked = v.Locked,
                Bounds = new ObsWsBoundsProperties()
                {
                    Alignment = v.Bounds.Alignment,
                    Type = v.Bounds.Type,
                    X = v.Bounds.X,
                    Y = v.Bounds.Y
                },
                SourceWidth = v.SourceWidth,
                SourceHeight = v.SourceHeight,
                Width = v.Width,
                Height = v.Height,
                ParentGroupName = v.ParentGroupName,
                GroupChildren = v.GroupChildren
            };
        }

        public static ObsSceneItemTransform FromGetSceneItemPropertiesReply(ObsRequestReplies.GetSceneItemPropertiesReply v)
        {
            return v;
        }
    }
}
