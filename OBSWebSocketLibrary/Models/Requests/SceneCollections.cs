using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.Requests
{
    public class SetCurrentSceneCollection : RequestBase
    {
        [JsonPropertyName("sc-name")]
        public string ScName { get; set; }
    }

    public class GetCurrentSceneCollection : RequestBase
    {
    }

    public class ListSceneCollections : RequestBase
    {
    }
}
