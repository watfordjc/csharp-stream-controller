using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequests
{
    public class StartStopRecordingRequest : RequestBase
    {
    }

    public class StartRecordingRequest : RequestBase
    {
    }

    public class StopRecordingRequest : RequestBase
    {
    }

    public class PauseRecordingRequest : RequestBase
    {
    }

    public class ResumeRecordingRequest : RequestBase
    {
    }

    public class SetRecordingFolderRequest : RequestBase
    {
        [JsonPropertyName("rec-folder")]
        public string RecFolder { get; set; }
    }

    public class GetRecordingFolderRequest : RequestBase
    {
    }
}
