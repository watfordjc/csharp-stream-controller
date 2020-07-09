using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequests
{
    public abstract class RequestBase
    {
        private Guid _guid = Guid.NewGuid();

        [JsonPropertyName("message-id")]
        public Guid MessageId { get { return _guid; } }
        [JsonPropertyName("request-type")]
        public string RequestTypeName
        {
            get
            {
                return Data.ObsWsRequest.GetRequestEnum(GetType()).ToString();
            }
        }
        [JsonIgnoreAttribute]
        public Data.ObsRequestType RequestType
        {
            get
            {
                return Data.ObsWsRequest.GetRequestEnum(GetType());
            }
        }
    }
}
