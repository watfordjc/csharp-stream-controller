using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.TypeDefs
{
    public class ObsStats
    {
        [JsonPropertyName("fps")]
        public double Fps { get; set; }
        [JsonPropertyName("render-total-frames")]
        public Decimal RenderTotalFrames { get; set; }
        [JsonPropertyName("render-missed-frames")]
        public Decimal RenderMissedFrames { get; set; }
        [JsonPropertyName("output-total-frames")]
        public Decimal OutputTotalFrames { get; set; }
        [JsonPropertyName("output-skipped-frames")]
        public Decimal OutputSkippedFrames { get; set; }
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
