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
        public SceneItemTransform[] GroupChildren { get; set; }
    }
}
