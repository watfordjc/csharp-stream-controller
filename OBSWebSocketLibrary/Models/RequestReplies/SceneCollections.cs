using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.RequestReplies
{
    public class SetCurrentSceneCollection : RequestReplyBase
    {
    }

    public class GetCurrentSceneCollection : RequestReplyBase
    {
        [JsonPropertyName("sc-name")]
        public string ScName { get; set; }
    }

    public class ListSceneCollections : RequestReplyBase
    {
        [JsonPropertyName("scene-collections")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<string> SceneCollections { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
