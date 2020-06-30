using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.TypeDefs.SourceTypes
{
    public class GainFilter
    {
        [JsonPropertyName("db")]
        public Decimal Db { get; set; }
    }

    public class NoiseSupressFilter
    {
        [JsonPropertyName("suppress_level")]
        public int SuppressLevel { get; set; }
    }

    public class InvertPolarityFilter
    {
    }

    public class NoiseGateFilter
    {
        [JsonPropertyName("close_threshold")]
        public Decimal CloseThreshold { get; set; }
        [JsonPropertyName("hold_time")]
        public int HoldTime { get; set; }
        [JsonPropertyName("open_threshold")]
        public Decimal OpenThreshold { get; set; }
    }

    public class CompressorFilter
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

    public class LimiterFilter
    {
        [JsonPropertyName("release_time")]
        public int ReleaseTime { get; set; }
        [JsonPropertyName("threshold")]
        public Decimal Threshold { get; set; }
    }

    public class ExpanderFilter
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
