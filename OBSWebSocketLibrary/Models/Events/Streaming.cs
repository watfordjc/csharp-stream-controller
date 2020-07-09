using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsEvents
{
    public class StreamStartingObsEvent : EventBase
    {
        [JsonPropertyName("preview-only")]
        public bool PreviewOnly { get; set; }
    }

    public class StreamStartedObsEvent : EventBase
    {
    }

    public class StreamStoppingObsEvent : EventBase
    {
        [JsonPropertyName("preview-only")]
        public bool PreviewOnly { get; set; }
    }

    public class StreamStoppedObsEvent : EventBase
    {
    }

    public class StreamStatusObsEvent : EventBase
    {
        [JsonPropertyName("streaming")]
        public bool Streaming { get; set; }
        [JsonPropertyName("recording")]
        public bool Recording { get; set; }
        [JsonPropertyName("replay-buffer-active")]
        public bool ReplayBufferActive { get; set; }
        [JsonPropertyName("bytes-per-sec")]
        public int BytesPerSec { get; set; }
        [JsonPropertyName("kbits-per-sec")]
        public int KbitsPerSec { get; set; }
        [JsonPropertyName("strain")]
        public double Strain { get; set; }
        [JsonPropertyName("total-stream-time")]
        public int TotalStreamTime { get; set; }
        [JsonPropertyName("num-total-frames")]
        public int NumTotalFrames { get; set; }
        [JsonPropertyName("num-dropped-frames")]
        public int NumDroppedFrames { get; set; }
        [JsonPropertyName("fps")]
        public double Fps { get; set; }
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
        [JsonPropertyName("preview-only")]
        public bool PreviewOnly { get; set; }
    }
}
