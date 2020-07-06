using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.Requests
{
    public abstract class RequestBase
    {
        private Guid _guid = Guid.NewGuid();

        [JsonPropertyName("message-id")]
        public Guid MessageId { get { return _guid; } }
        [JsonPropertyName("request-type")]
        public string RequestTypeName { get { return GetType().Name; } }
        [JsonIgnoreAttribute]
        public Data.RequestType RequestType { get { return Data.Request.GetRequestEnum(GetType()); } }
    }
}
