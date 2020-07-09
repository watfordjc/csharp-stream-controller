using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequestReplies
{
    public abstract class ReplyBase
    {
        [JsonPropertyName("message-id")]
        public string MessageId { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
}
