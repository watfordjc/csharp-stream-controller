using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.RequestReplies
{
    public class GetStreamingStatus : RequestReplyBase
    {
        [JsonPropertyName("streaming")]
        public bool Streaming { get; set; }
        [JsonPropertyName("recording")]
        public bool Recording { get; set; }
        [JsonPropertyName("stream-timecode")]
        public string StreamTimecode { get; set; }
        [JsonPropertyName("rec-timecode")]
        public string RecTimecode { get; set; }
        [JsonPropertyName("preview-only")]
        public bool PreviewOnly { get; set; }
    }

    public class StartStopStreaming : RequestReplyBase
    {
    }

    public class StartStreaming : RequestReplyBase
    {
    }

    public class StopStreaming : RequestReplyBase
    {
    }

    public class SetStreamSettings : RequestReplyBase
    {
    }

    public class GetStreamSettings : RequestReplyBase
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("settings")]
        public TypeDefs.ObsStreamSettings Settings { get; set; }
    }

    public class SaveStreamSettings : RequestReplyBase
    {
    }

    public class SendCaptions : RequestReplyBase
    {
    }
}
