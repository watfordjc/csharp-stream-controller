using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.Requests
{
    public class StartStopRecording : RequestBase
    {
    }

    public class StartRecording : RequestBase
    {
    }

    public class StopRecording : RequestBase
    {
    }

    public class PauseRecording : RequestBase
    {
    }

    public class ResumeRecording : RequestBase
    {
    }

    public class SetRecordingFolder : RequestBase
    {
        [JsonPropertyName("rec-folder")]
        public string RecFolder { get; set; }
    }

    public class GetRecordingFolder : RequestBase
    {
    }
}
