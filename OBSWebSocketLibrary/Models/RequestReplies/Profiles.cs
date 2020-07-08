using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.RequestReplies
{
    public class SetCurrentProfile : RequestReplyBase
    {
    }

    public class GetCurrentProfile : RequestReplyBase
    {
        [JsonPropertyName("profile-name")]
        public string ProfileName { get; set; }
    }

    public class ListProfiles : RequestReplyBase
    {
        [JsonPropertyName("profiles")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<OBSWebSocketLibrary.Models.TypeDefs.ObsProfile> Profiles { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
