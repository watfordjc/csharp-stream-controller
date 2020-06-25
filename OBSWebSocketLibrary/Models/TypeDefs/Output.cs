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
        public class Flags
        {
            [JsonPropertyName("flags.rawValue")]
            public int FlagsRawValue { get; set; }
            [JsonPropertyName("flags.audio")]
            public bool FlagsAudio { get; set; }
            [JsonPropertyName("flags.video")]
            public bool FlagsVideo { get; set; }
            [JsonPropertyName("flags.encoded")]
            public bool FlagsEncoded { get; set; }
            [JsonPropertyName("flags.multiTrack")]
            public bool FlagsMultiTrack { get; set; }
            [JsonPropertyName("flags.service")]
            public bool FlagsService { get; set; }
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
