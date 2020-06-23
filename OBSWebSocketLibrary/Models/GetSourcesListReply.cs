using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models
{
    public class GetSourcesListReply
    {
        [JsonPropertyName("message-id")]
        public string MessageId { get; set; }
        [JsonPropertyName("sources")]
        public Source[] Sources { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
