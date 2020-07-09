using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.TypeDefs
{
    public class ObsSceneItem
    {
        private string nameTmp = String.Empty;
        private bool mutedTmp = false;
        private double volumeTmp = 0;
        private BaseType source;
        private int syncOffsetTmp = 0;

        [JsonPropertyName("cy")]
        public double Cy { get; set; }
        [JsonPropertyName("cx")]
        public double Cx { get; set; }
        [JsonPropertyName("alignment")]
        public double Alignment { get; set; }
        [JsonPropertyName("name")]
        public string Name
        {
            get
            {
                return Source != null ? Source.Name : nameTmp;
            }
            set
            {
                nameTmp = value;
                if (Source != null)
                {
                    Source.Name = value;
                }
            }
        }
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
#pragma warning disable CA2227 // Collection properties should be read only
        public ObservableCollection<ObsSceneItem> GroupChildren { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
        [JsonIgnore]
        public BaseType Source
        {
            get { return source; }
            set
            {
                source = value;
                if (source != null)
                {
                    source.Muted = mutedTmp;
                    source.Volume = volumeTmp;
                    source.SyncOffset = syncOffsetTmp;
                    source.Name = nameTmp;
                }
            }
        }
        [JsonIgnore]
        public ObsSceneItemTransform Transform { get; set; }
        [JsonIgnore]
        public int SyncOffset
        {
            get
            {
                return Source != null ? Source.SyncOffset : syncOffsetTmp;
            }
            set
            {
                syncOffsetTmp = value;
                if (Source != null)
                {
                    Source.SyncOffset = value;
                }
            }
        }
    }
}
