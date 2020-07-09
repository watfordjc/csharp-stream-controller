using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequestReplies
{
    public class ListOutputsReply : ReplyBase
    {
        [JsonPropertyName("outputs")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TypeDefs.ObsOutput> Outputs { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class GetOutputInfoReply : ReplyBase
    {
        [JsonPropertyName("outputInfo")]
        public TypeDefs.ObsOutput OutputInfo { get; set; }
    }

    public class StartOutputReply : ReplyBase
    {
    }

    public class StopOutputReply : ReplyBase
    {
    }
}
