using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.TypeDefs
{
    public class AudioLine
    {
    }

    public class WasapiOutputCapture : BaseType
    {
        [JsonPropertyName("device_id")]
        public string DeviceID
        {
            get
            {
                return Dependencies.AudioDeviceId;
            }
            set
            {
                Dependencies.AudioDeviceId = value;
            }
        }
    }

    public class WasapiInputCapture : BaseType
    {
        [JsonPropertyName("device_id")]
        public string DeviceID {
            get
            {
                return Dependencies.AudioDeviceId;
            }
            set
            {
                Dependencies.AudioDeviceId = value;
            }
        }
        [JsonPropertyName("use_device_timing")]
        public bool UseDeviceTiming { get; set; }
    }
}
