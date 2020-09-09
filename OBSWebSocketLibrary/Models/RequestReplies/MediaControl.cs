using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequestReplies
{
    public class PlayPauseMediaReply : ReplyBase
    {
    }

    public class RestartMediaReply : ReplyBase
    {
    }

    public class StopMediaReply : ReplyBase
    {
    }

    public class NextMediaReply : ReplyBase
    {
    }

    public class PreviousMediaReply : ReplyBase
    {
    }

    public class GetMediaDurationReply : ReplyBase
    {
        [JsonPropertyName("mediaDuration")]
        public int MediaDuration { get; set; }
    }

    public class GetMediaTimeReply : ReplyBase
    {
        [JsonPropertyName("timestamp")]
        public int Timestamp { get; set; }
    }

    public class SetMediaTimeReply : ReplyBase
    {
    }

    public class ScrubMediaReply : ReplyBase
    {
    }

    public class GetMediaStateReply : ReplyBase
    {
        [JsonPropertyName("mediaState")]
        public string MediaState { get; set; }
    }
}
