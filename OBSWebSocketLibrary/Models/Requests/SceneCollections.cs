using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequests
{
    public class SetCurrentSceneCollectionRequest : RequestBase
    {
        [JsonPropertyName("sc-name")]
        public string ScName { get; set; }
    }

    public class GetCurrentSceneCollectionRequest : RequestBase
    {
    }

    public class ListSceneCollectionsRequest : RequestBase
    {
    }
}
