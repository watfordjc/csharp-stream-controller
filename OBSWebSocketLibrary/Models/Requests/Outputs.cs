using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequests
{
    public class ListOutputsRequest : RequestBase
    {
    }

    public class GetOutputInfoRequest : RequestBase
    {
        [JsonPropertyName("outputName")]
        public string OutputName { get; set; }
    }

    public class StartOutputRequest : RequestBase
    {
        [JsonPropertyName("outputName")]
        public string OutputName { get; set; }
    }

    public class StopOutputRequest : RequestBase
    {
        [JsonPropertyName("outputName")]
        public string OutputName { get; set; }
        [JsonPropertyName("force")]
        public bool Force { get; set; }
    }
}
