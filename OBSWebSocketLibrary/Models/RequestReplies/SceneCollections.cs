using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.RequestReplies
{
    public class SetCurrentSceneCollection : RequestBase
    {
    }

    public class GetCurrentSceneCollection : RequestBase
    {
        [JsonPropertyName("sc-name")]
        public string ScName { get; set; }
    }

    public class ListSceneCollections : RequestBase
    {
        [JsonPropertyName("scene-collections")]
        public string[] SceneCollections { get; set; }
    }
}
