using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.TypeDefs.FilterTypes
{
    public class GainFilter : BaseFilter
    {
        [JsonPropertyName("db")]
        public Decimal Db { get; set; }
    }

    public class NoiseSupressFilter : BaseFilter
    {
        [JsonPropertyName("suppress_level")]
        public int SuppressLevel { get; set; }
    }

    public class InvertPolarityFilter : BaseFilter
    {
    }

    public class NoiseGateFilter : BaseFilter
    {
        [JsonPropertyName("close_threshold")]
        public Decimal CloseThreshold { get; set; }
        [JsonPropertyName("hold_time")]
        public int HoldTime { get; set; }
        [JsonPropertyName("open_threshold")]
        public Decimal OpenThreshold { get; set; }
    }

    public class CompressorFilter : BaseFilter
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

    public class LimiterFilter : BaseFilter
    {
        [JsonPropertyName("release_time")]
        public int ReleaseTime { get; set; }
        [JsonPropertyName("threshold")]
        public Decimal Threshold { get; set; }
    }

    public class ExpanderFilter : BaseFilter
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

    public class VstFilter : BaseFilter
    {
        [JsonPropertyName("chunk_data")]
        public string ChunkDataBase64 { get; set; }
        [JsonPropertyName("plugin_path")]
        public string PluginPath { get; set; }
    }
}
