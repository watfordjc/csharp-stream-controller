using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequestReplies
{
    public class GetStreamingStatusReply : ReplyBase
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

    public class StartStopStreamingReply : ReplyBase
    {
    }

    public class StartStreamingReply : ReplyBase
    {
    }

    public class StopStreamingReply : ReplyBase
    {
    }

    public class SetStreamSettingsReply : ReplyBase
    {
    }

    public class GetStreamSettingsReply : ReplyBase
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("settings")]
        public TypeDefs.ObsWsStreamSettings Settings { get; set; }
    }

    public class SaveStreamSettingsReply : ReplyBase
    {
    }

    public class SendCaptionsReply : ReplyBase
    {
    }
}
