using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.Requests
{
    public class GetSourcesList
    {
        [JsonPropertyName("message-id")]
        public string MessageId { get; set; }
        [JsonPropertyName("sources")]
        public Source[] Sources { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }

        public class Source
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("type")]
            public string Type { get; set; }
            [JsonPropertyName("typeId")]
            public string TypeId { get; set; }
        }
    }
}
