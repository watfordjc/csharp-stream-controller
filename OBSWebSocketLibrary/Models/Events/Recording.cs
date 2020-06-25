using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.Events
{
    public class RecordingStarting : EventBase
    {
    }

    public class RecordingStarted : EventBase
    {
    }

    public class RecordingStopping : EventBase
    {
    }

    public class RecordingStopped : EventBase
    {
    }

    public class RecordingPaused : EventBase
    {
    }

    public class RecordingResumed : EventBase
    {
    }


}
