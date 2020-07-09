using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequestReplies
{
    public class SetCurrentSceneCollectionReply : ReplyBase
    {
    }

    public class GetCurrentSceneCollectionReply : ReplyBase
    {
        [JsonPropertyName("sc-name")]
        public string ScName { get; set; }
    }

    public class ListSceneCollectionsReply : ReplyBase
    {
        [JsonPropertyName("scene-collections")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<string> SceneCollections { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
