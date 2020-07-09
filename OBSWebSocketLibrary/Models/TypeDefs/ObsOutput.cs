using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.TypeDefs
{
    public class ObsOutput
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
        public TypeDefs.ObsWsOutputFlags Flags { get; set; }
        [JsonPropertyName("settings")]
        public TypeDefs.ObsWsOutputSettings Settings { get; set; }
        [JsonPropertyName("totalFrames")]
        public int TotalFrames { get; set; }
        [JsonPropertyName("droppedFrames")]
        public int DroppedFrames { get; set; }
        [JsonPropertyName("totalBytes")]
        public int TotalBytes { get; set; }
    }
}
