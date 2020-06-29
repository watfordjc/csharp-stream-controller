using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.TypeDefs
{
    public class SceneItem
    {
        [JsonPropertyName("cy")]
        public double Cy { get; set; }
        [JsonPropertyName("cx")]
        public double Cx { get; set; }
        [JsonPropertyName("alignment")]
        public double Alignment { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("id")]
        public double Id { get; set; }
        [JsonPropertyName("render")]
        public bool Render { get; set; }
        [JsonPropertyName("muted")]
        public bool Muted { get; set; }
        [JsonPropertyName("locked")]
        public bool Locked { get; set; }
        [JsonPropertyName("source_cx")]
        public double SourceCx { get; set; }
        [JsonPropertyName("source_cy")]
        public double SourceCy { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("volume")]
        public double Volume { get; set; }
        [JsonPropertyName("x")]
        public double X { get; set; }
        [JsonPropertyName("y")]
        public double Y { get; set; }
        [JsonPropertyName("parentGroupName")]
        public string ParentGroupName { get; set; }
        [JsonPropertyName("groupChildren")]
        public SceneItem[] GroupChildren { get; set; }
    }
}
