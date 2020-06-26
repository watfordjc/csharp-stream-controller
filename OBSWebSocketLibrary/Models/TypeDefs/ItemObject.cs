using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.TypeDefs
{
    public class ItemObject
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }
}
