using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.SharedModels
{
    public class DeviceApplicationPreferences
    {
        [JsonPropertyName("devices")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<DeviceApplicationPreference> Devices { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class DeviceApplicationPreference
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("applications")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<string> Applications { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
