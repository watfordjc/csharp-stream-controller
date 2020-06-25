using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.Events
{
    public abstract class EventBase
    {
        [JsonPropertyName("update-type")]
        public string UpdateType { get; set; }
        [JsonPropertyName("stream-timecode")]
        public string StreamTimeCode { get; set; }
        [JsonPropertyName("rec-timecode")]
        public string RecTimeCode { get; set; }
    }
}
