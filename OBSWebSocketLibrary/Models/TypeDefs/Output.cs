using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.TypeDefs
{
    public class Output
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("width")]
        public int Width { get; set; }
        [JsonPropertyName("height")]
        public int Height { get; set; }
        [JsonPropertyName("flags")]
        public FlagsProperties Flags { get; set; }
        public class FlagsProperties
        {
            [JsonPropertyName("rawValue")]
            public int RawValue { get; set; }
            [JsonPropertyName("audio")]
            public bool Audio { get; set; }
            [JsonPropertyName("video")]
            public bool Video { get; set; }
            [JsonPropertyName("encoded")]
            public bool Encoded { get; set; }
            [JsonPropertyName("multiTrack")]
            public bool MultiTrack { get; set; }
            [JsonPropertyName("service")]
            public bool Service { get; set; }
        }
        public class Settings
        {
            [JsonPropertyName("active")]
            public bool Active { get; set; }
            [JsonPropertyName("reconnecting")]
            public bool Reconnecting { get; set; }
            [JsonPropertyName("congestion")]
            public double Congestion { get; set; }
        }
        [JsonPropertyName("totalFrames")]
        public int TotalFrames { get; set; }
        [JsonPropertyName("droppedFrames")]
        public int DroppedFrames { get; set; }
        [JsonPropertyName("totalBytes")]
        public int TotalBytes { get; set; }
    }
}
