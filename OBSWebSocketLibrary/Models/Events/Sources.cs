using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.Events
{
    public class SourceCreated : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("sourceType")]
        public string SourceType { get; set; }
        [JsonPropertyName("sourceKind")]
        public string SourceKind { get; set; }
        [JsonPropertyName("sourceSettings")]
        public JsonElement SourceSettings { get; set; }
        public object SourceSettingsObj { get; set; }
    }

    public class SourceDestroyed : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("sourceType")]
        public string SourceType { get; set; }
        [JsonPropertyName("sourceKind")]
        public string SourceKind { get; set; }
    }

    public class SourceVolumeChanged : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("volume")]
        public float Volume { get; set; }
    }

    public class SourceMuteStateChanged : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("muted")]
        public bool Muted { get; set; }
    }

    public class SourceAudioSyncOffsetChanged : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("syncOffset")]
        public int SyncOffset { get; set; }
    }

    public class SourceAudioMixersChanged : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("mixers")]
        public IList<TypeDefs.ObsMixer> Mixers { get; set; }
        [JsonPropertyName("hexMixersValue")]
        public string HexMixersValue { get; set; }
    }
    public class SourceRenamed : EventBase
    {
        [JsonPropertyName("previousName")]
        public string PreviousName { get; set; }
        [JsonPropertyName("newName")]
        public string NewName { get; set; }
        [JsonPropertyName("sourceType")]
        public string SourceType { get; set; }
    }

    public class SourceFilterAdded : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("filterName")]
        public string FilterName { get; set; }
        [JsonPropertyName("filterType")]
        public string FilterType { get; set; }
        [JsonPropertyName("filterSettings")]
        public JsonElement FilterSettings { get; set; }
        public object FilterSettingsObj { get; set; }
    }

    public class SourceFilterRemoved : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("filterName")]
        public string FilterName { get; set; }
        [JsonPropertyName("filterType")]
        public string FilterType { get; set; }
    }

    public class SourceFilterVisibilityChanged : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("filterName")]
        public string FilterName { get; set; }
        [JsonPropertyName("filterEnabled")]
        public bool FilterEnabled { get; set; }
    }

    public class SourceFiltersReordered : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("filters")]
        public IList<TypeDefs.ObsFilter> Filters { get; set; }
    }
}