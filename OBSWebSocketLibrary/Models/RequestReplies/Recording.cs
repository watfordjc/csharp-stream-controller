using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequestReplies
{
    public class GetRecordingStatusReply : ReplyBase
    {
        [JsonPropertyName("isRecording")]
        public bool IsRecording { get; set; }
        [JsonPropertyName("isRecordingPaused")]
        public bool IsRecordingPaused { get; set; }
        [JsonPropertyName("recordTimecode")]
        public string RecordTimeCode { get; set; }
    }

    public class StartStopRecordingReply : ReplyBase
    {
    }

    public class StartRecordingReply : ReplyBase
    {
    }

    public class StopRecordingReply : ReplyBase
    {
    }

    public class PauseRecordingReply : ReplyBase
    {
    }

    public class ResumeRecordingReply : ReplyBase
    {
    }

    public class SetRecordingFolderReply : ReplyBase
    {
    }

    public class GetRecordingFolderReply : ReplyBase
    {
        [JsonPropertyName("rec-folder")]
        public string RecFolder { get; set; }
    }
}
