using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.TypeDefs
{
    public class ObsStats
    {
        [JsonPropertyName("fps")]
        public double FPS { get; set; }
        [JsonPropertyName("render-total-frames")]
        public int RenderTotalFrames { get; set; }
        [JsonPropertyName("render-missed-frames")]
        public int RenderMissedFrames { get; set; }
        [JsonPropertyName("output-total-frames")]
        public int OutputTotalFrames { get; set; }
        [JsonPropertyName("output-skipped-frames")]
        public int OutputSkippedFrames { get; set; }
        [JsonPropertyName("average-frame-time")]
        public double AverageFrameTime { get; set; }
        [JsonPropertyName("cpu-usage")]
        public double CpuUsage { get; set; }
        [JsonPropertyName("memory-usage")]
        public double MemoryUsage { get; set; }
        [JsonPropertyName("free-disk-space")]
        public double FreeDiskSpace { get; set; }
    }
}
