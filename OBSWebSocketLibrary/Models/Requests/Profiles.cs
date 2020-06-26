using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.Requests
{
    public class SetCurrentProfile : RequestBase
    {
        [JsonPropertyName("profile-name")]
        public string ProfileName { get; set; }
    }

    public class GetCurrentProfile : RequestBase
    {
    }

    public class ListProfiles : RequestBase
    {
    }
}
