using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.RequestReplies
{
    public class SetCurrentProfile : RequestBase
    {
    }

    public class GetCurrentProfile : RequestBase
    {
        [JsonPropertyName("profile-name")]
        public string ProfileName { get; set; }
    }

    public class ListProfiles : RequestBase
    {
        [JsonPropertyName("profiles")]
        public object[] Profiles { get; set; }
    }
}
