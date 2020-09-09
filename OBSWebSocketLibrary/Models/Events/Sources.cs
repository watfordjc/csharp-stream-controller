using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsEvents
{
    public class SourceCreatedObsEvent : EventBase
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

    public class SourceDestroyedObsEvent : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("sourceType")]
        public string SourceType { get; set; }
        [JsonPropertyName("sourceKind")]
        public string SourceKind { get; set; }
    }

    public class SourceVolumeChangedObsEvent : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("volume")]
        public float Volume { get; set; }
    }

    public class SourceMuteStateChangedObsEvent : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("muted")]
        public bool Muted { get; set; }
    }

    public class SourceAudioDeactivatedObsEvent : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
    }

    public class SourceAudioActivatedObsEvent : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
    }

    public class SourceAudioSyncOffsetChangedObsEvent : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("syncOffset")]
        public int SyncOffset { get; set; }
    }

    public class SourceAudioMixersChangedObsEvent : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("mixers")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TypeDefs.ObsWsMixer> Mixers { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
        [JsonPropertyName("hexMixersValue")]
        public string HexMixersValue { get; set; }
    }
    public class SourceRenamedObsEvent : EventBase
    {
        [JsonPropertyName("previousName")]
        public string PreviousName { get; set; }
        [JsonPropertyName("newName")]
        public string NewName { get; set; }
        [JsonPropertyName("sourceType")]
        public string SourceType { get; set; }
    }

    public class SourceFilterAddedObsEvent : EventBase
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

    public class SourceFilterRemovedObsEvent : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("filterName")]
        public string FilterName { get; set; }
        [JsonPropertyName("filterType")]
        public string FilterType { get; set; }
    }

    public class SourceFilterVisibilityChangedObsEvent : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("filterName")]
        public string FilterName { get; set; }
        [JsonPropertyName("filterEnabled")]
        public bool FilterEnabled { get; set; }
    }

    public class SourceFiltersReorderedObsEvent : EventBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("filters")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TypeDefs.ObsWsFilter> Filters { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}