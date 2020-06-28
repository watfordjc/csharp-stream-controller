using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.TypeDefs.SourceTypes
{
    public class WasapiOutputCapture
    {
        [JsonPropertyName("device_id")]
        public string DeviceID { get; set; }
    }

    public class WasapiInputCapture
    {
        [JsonPropertyName("device_id")]
        public string DeviceID { get; set; }
        [JsonPropertyName("use_device_timing")]
        public bool UseDeviceTiming { get; set; }
    }
}
