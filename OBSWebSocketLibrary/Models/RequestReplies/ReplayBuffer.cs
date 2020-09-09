using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequestReplies
{
    public class GetReplayBufferStatusReply : ReplyBase
    {
        [JsonPropertyName("isReplayBufferActive")]
        public bool IsReplayBufferActive { get; set; }
    }

    public class StartStopReplayBufferReply : ReplyBase
    {
    }

    public class StartReplayBufferReply : ReplyBase
    {
    }

    public class StopReplayBufferReply : ReplyBase
    {
    }

    public class SaveReplayBufferReply : ReplyBase
    {
    }
}
