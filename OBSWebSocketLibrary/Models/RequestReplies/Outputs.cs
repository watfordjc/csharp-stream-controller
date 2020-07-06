using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.RequestReplies
{
    public class ListOutputs : RequestReplyBase
    {
        [JsonPropertyName("outputs")]
        public IList<TypeDefs.Output> Outputs { get; set; }
    }

    public class GetOutputInfo : RequestReplyBase
    {
        [JsonPropertyName("outputInfo")]
        public TypeDefs.Output OutputInfo { get; set; }
    }

    public class StartOutput : RequestReplyBase
    {
    }

    public class StopOutput : RequestReplyBase
    {
    }
}
