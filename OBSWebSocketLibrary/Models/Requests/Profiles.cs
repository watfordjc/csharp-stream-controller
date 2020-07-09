using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequests
{
    public class SetCurrentProfileRequest : RequestBase
    {
        [JsonPropertyName("profile-name")]
        public string ProfileName { get; set; }
    }

    public class GetCurrentProfileRequest : RequestBase
    {
    }

    public class ListProfilesRequest : RequestBase
    {
    }
}
