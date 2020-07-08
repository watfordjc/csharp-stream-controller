using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.RequestReplies
{
    public class ListOutputs : RequestReplyBase
    {
        [JsonPropertyName("outputs")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TypeDefs.Output> Outputs { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
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
