using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.RequestReplies
{
    public abstract class RequestReplyBase
    {
        [JsonPropertyName("message-id")]
        public string MessageId { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
}
