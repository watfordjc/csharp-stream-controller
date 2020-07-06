using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.TypeDefs
{
    public class SceneItemTransform
    {
        [JsonPropertyName("position")]
        public PositionProperties Position { get; set; }
        public class PositionProperties : TypeDefs.Coordinates
        {
            [JsonPropertyName("alignment")]
            public int Alignment { get; set; }
        }
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
        public BoundsProperties Bounds { get; set; }
        public class BoundsProperties : TypeDefs.Coordinates
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }
            [JsonPropertyName("alignment")]
            public int Alignment { get; set; }
        }
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
        public IList<SceneItemTransform> GroupChildren { get; set; }

        public static implicit operator SceneItemTransform(RequestReplies.GetSceneItemProperties v)
        {
            if (v == null) { return null; }

            return new SceneItemTransform
            {
                Position = new PositionProperties()
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
                Bounds = new BoundsProperties()
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

        public static SceneItemTransform FromGetSceneItemProperties(RequestReplies.GetSceneItemProperties v)
        {
            return v;
        }
    }
}
