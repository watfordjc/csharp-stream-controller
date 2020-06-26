using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.RequestReplies
{
    public class StartStopRecording : RequestReplyBase
    {
    }

    public class StartRecording : RequestReplyBase
    {
    }

    public class StopRecording : RequestReplyBase
    {
    }

    public class PauseRecording : RequestReplyBase
    {
    }

    public class ResumeRecording : RequestReplyBase
    {
    }

    public class SetRecordingFolder : RequestReplyBase
    {
    }

    public class GetRecordingFolder : RequestReplyBase
    {
        [JsonPropertyName("rec-folder")]
        public string RecFolder { get; set; }
    }
}
