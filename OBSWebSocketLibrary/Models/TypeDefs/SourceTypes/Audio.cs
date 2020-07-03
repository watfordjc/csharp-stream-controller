using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.TypeDefs.SourceTypes
{
    public class AudioLine
    {
    }

    public class GainFilter : BaseType
    {
        [JsonPropertyName("db")]
        public Decimal Db { get; set; }
    }

    public class NoiseSupressFilter : BaseType
    {
        [JsonPropertyName("suppress_level")]
        public int SuppressLevel { get; set; }
    }

    public class InvertPolarityFilter : BaseType
    {
    }

    public class NoiseGateFilter : BaseType
    {
        [JsonPropertyName("close_threshold")]
        public Decimal CloseThreshold { get; set; }
        [JsonPropertyName("hold_time")]
        public int HoldTime { get; set; }
        [JsonPropertyName("open_threshold")]
        public Decimal OpenThreshold { get; set; }
    }

    public class CompressorFilter : BaseType
    {
        [JsonPropertyName("attack_time")]
        public int AttackTime { get; set; }
        [JsonPropertyName("output_gain")]
        public Decimal OutputGain { get; set; }
        [JsonPropertyName("ratio")]
        public Decimal Ratio { get; set; }
        [JsonPropertyName("release_time")]
        public int ReleaseTime { get; set; }
        [JsonPropertyName("sidechain_source")]
        public string SidechainSource { get; set; }
        [JsonPropertyName("threshold")]
        public Decimal Threshold { get; set; }
    }

    public class LimiterFilter : BaseType
    {
        [JsonPropertyName("release_time")]
        public int ReleaseTime { get; set; }
        [JsonPropertyName("threshold")]
        public Decimal Threshold { get; set; }
    }

    public class ExpanderFilter : BaseType
    {
        [JsonPropertyName("attack_time")]
        public int AttackTime { get; set; }
        [JsonPropertyName("detector")]
        public string Detector { get; set; }
        [JsonPropertyName("output_gain")]
        public Decimal OutputGain { get; set; }
        [JsonPropertyName("presets")]
        public string Presets { get; set; }
        [JsonPropertyName("ratio")]
        public Decimal Ratio { get; set; }
        [JsonPropertyName("release_time")]
        public int ReleaseTime { get; set; }
        [JsonPropertyName("threshold")]
        public Decimal Threshold { get; set; }
    }

    public class VstFilter : BaseType
    {
        [JsonPropertyName("chunk_data")]
        public string ChunkDataBase64 { get; set; }
        [JsonPropertyName("plugin_path")]
        public string PluginPath { get; set; }
    }

    public class WasapiOutputCapture : BaseType
    {
        public new BaseType.Dependencies Dependencies = new BaseType.Dependencies()
        {
            HasAudioInterface = true
        };
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
        public new BaseType.Dependencies Dependencies = new BaseType.Dependencies()
        {
            HasAudioInterface = true
        };
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
