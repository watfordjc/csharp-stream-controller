using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.Requests
{
    public class ListOutputs : RequestBase
    {
    }

    public class GetOutputInfo : RequestBase
    {
        [JsonPropertyName("outputName")]
        public string OutputName { get; set; }
    }

    public class StartOutput : RequestBase
    {
        [JsonPropertyName("outputName")]
        public string OutputName { get; set; }
    }

    public class StopOutput : RequestBase
    {
        [JsonPropertyName("outputName")]
        public string OutputName { get; set; }
        [JsonPropertyName("force")]
        public bool Force { get; set; }
    }
}
