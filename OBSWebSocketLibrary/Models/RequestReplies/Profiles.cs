using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequestReplies
{
    public class SetCurrentProfileReply : ReplyBase
    {
    }

    public class GetCurrentProfileReply : ReplyBase
    {
        [JsonPropertyName("profile-name")]
        public string ProfileName { get; set; }
    }

    public class ListProfilesReply : ReplyBase
    {
        [JsonPropertyName("profiles")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TypeDefs.ObsWsProfile> Profiles { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
