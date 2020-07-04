using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.TypeDefs
{
    public class SceneItem
    {
        private bool mutedTmp = false;
        private double volumeTmp = 0;
        private SourceTypes.BaseType source;
        [JsonPropertyName("cy")]
        public double Cy { get; set; }
        [JsonPropertyName("cx")]
        public double Cx { get; set; }
        [JsonPropertyName("alignment")]
        public double Alignment { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("render")]
        public bool Render { get; set; }
        [JsonPropertyName("muted")]
        public bool Muted
        {
            get
            {
                return Source != null ? Source.Muted : mutedTmp;
            }
            set
            {
                mutedTmp = value;
                if (Source != null)
                {
                    Source.Muted = value;
                }
            }
        }
        [JsonPropertyName("locked")]
        public bool Locked { get; set; }
        [JsonPropertyName("source_cx")]
        public double SourceCx { get; set; }
        [JsonPropertyName("source_cy")]
        public double SourceCy { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("volume")]
        public double Volume
        {
            get
            {
                return Source != null ? Source.Volume : volumeTmp;
            }
            set
            {
                volumeTmp = value;
                if (Source != null)
                {
                    Source.Volume = value;
                }
            }
        }
        [JsonPropertyName("x")]
        public double X { get; set; }
        [JsonPropertyName("y")]
        public double Y { get; set; }
        [JsonPropertyName("parentGroupName")]
        public string ParentGroupName { get; set; }
        [JsonPropertyName("groupChildren")]
        public ObservableCollection<SceneItem> GroupChildren { get; set; }
        [JsonIgnore]
        public SourceTypes.BaseType Source
        {
            get { return source; }
            set
            {
                source = value;
                if (source != null)
                {
                    source.Muted = mutedTmp;
                    source.Volume = volumeTmp;
                }
            }
        }
        [JsonIgnore]
        public SceneItemTransform Transform { get; set; }
    }
}
