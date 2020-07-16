using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.SharedModels
{
    public class ApplicationDevicePreferences
    {
        [JsonPropertyName("applications")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<ApplicationDevicePreference> Applications { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class ApplicationDevicePreference
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("devices")]
        public DefaultDevicePreference Devices { get; set; }
    }

    public class DefaultDevicePreference
    {
        [JsonPropertyName("render")]
        public string RenderDeviceId { get; set; }
        [JsonPropertyName("capture")]
        public string CaptureDeviceId { get; set; }
    }
}
