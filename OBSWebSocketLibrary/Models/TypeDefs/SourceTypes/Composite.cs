using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.TypeDefs
{
    public class Scene : BaseType
    {
        [JsonPropertyName("custom_size")]
        public bool CustomSize { get; set; }
        [JsonPropertyName("id_counter")]
        public int IdCounter { get; set; }
        [JsonPropertyName("items")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<ObsWsSceneItem> Items { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
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
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<ObsWsSceneItem> Items { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
