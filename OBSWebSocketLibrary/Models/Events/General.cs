using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsEvents
{
    public class HeartbeatObsEvent : EventBase
    {
        [JsonPropertyName("pulse")]
        public bool Pulse { get; set; }
        [JsonPropertyName("current-profile")]
        public string CurrentProfile { get; set; }
        [JsonPropertyName("current-scene")]
        public string CurrentScene { get; set; }
        [JsonPropertyName("streaming")]
        public bool Streaming { get; set; }
        [JsonPropertyName("total-stream-time")]
        public int TotalStreamTime { get; set; }
        [JsonPropertyName("total-stream-bytes")]
        public int TotalStreamBytes { get; set; }
        [JsonPropertyName("total-stream-frames")]
        public int TotalStreamFrames { get; set; }
        [JsonPropertyName("recording")]
        public bool Recording { get; set; }
        [JsonPropertyName("total-record-time")]
        public int TotalRecordTime { get; set; }
        [JsonPropertyName("total-record-bytes")]
        public int TotalRecordBytes { get; set; }
        [JsonPropertyName("total-record-frames")]
        public int TotalRecordFrames { get; set; }
        [JsonPropertyName("stats")]
        public TypeDefs.ObsStats Stats { get; set; }
    }

    public class BroadcastCustomMessageObsEvent : EventBase
    {
        [JsonPropertyName("realm")]
        public string Realm { get; set; }
        [JsonPropertyName("data")]
        public JsonElement Data { get; set; }
        public object DataObj { get; set; }
    }
}
