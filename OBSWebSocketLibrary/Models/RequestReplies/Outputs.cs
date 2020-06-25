using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.RequestReplies
{
    public class ListOutputs : RequestBase
    {
        [JsonPropertyName("outputs")]
        public TypeDefs.Output[] Outputs { get; set; }
    }

    public class GetOutputInfo : RequestBase
    {
        [JsonPropertyName("outputInfo")]
        public TypeDefs.Output OutputInfo { get; set; }
    }

    public class StartOutput : RequestBase
    {
    }

    public class StopOutput : RequestBase
    {
    }
}
