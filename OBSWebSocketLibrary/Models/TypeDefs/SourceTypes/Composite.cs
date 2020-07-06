using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.TypeDefs.SourceTypes
{
    public class Scene : BaseType
    {
        [JsonPropertyName("custom_size")]
        public bool CustomSize { get; set; }
        [JsonPropertyName("id_counter")]
        public int IdCounter { get; set; }
        [JsonPropertyName("items")]
        public IList<ObsSceneItem> Items { get; set; }
    }

    public class Group : BaseType
    {
        [JsonPropertyName("custom_size")]
        public bool CustomSize { get; set; }
        [JsonPropertyName("cx")]
        public int Cx { get; set; }
        [JsonPropertyName("cy")]
        public int Cy { get; set; }
        [JsonPropertyName("id_counter")]
        public int IdCounter { get; set; }
        [JsonPropertyName("items")]
        public IList<ObsSceneItem> Items { get; set; }
    }
}
