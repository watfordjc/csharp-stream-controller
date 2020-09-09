using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequestReplies
{
    public class GetMediaSourcesListReply : ReplyBase
    {
        [JsonPropertyName("mediaSources")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TypeDefs.ObsWsReplyMediaSource> MediaSources { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class GetSourcesListReply : ReplyBase
    {
        [JsonPropertyName("sources")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TypeDefs.ObsWsReplySource> Sources { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class GetSourceTypesListReply : ReplyBase
    {
        [JsonPropertyName("types")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TypeDefs.ObsWsReplyType> Types { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class GetVolumeReply : ReplyBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("volume")]
        public double Volume { get; set; }
        [JsonPropertyName("muted")]
        public bool Muted { get; set; }
    }

    public class SetVolumeReply : ReplyBase
    {
    }

    public class GetMuteReply : ReplyBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("muted")]
        public bool Muted { get; set; }
    }

    public class SetMuteReply : ReplyBase
    {
    }

    public class ToggleMuteReply : ReplyBase
    {
    }

    public class GetAudioActiveReply : ReplyBase
    {
        [JsonPropertyName("audioActive")]
        public bool AudioActive { get; set; }
    }

    public class SetSourceNameReply : ReplyBase
    {
    }

    public class SetSyncOffsetReply : ReplyBase
    {
    }

    public class GetSyncOffsetReply : ReplyBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("offset")]
        public int Offset { get; set; }
    }

    public class GetSourceSettingsReply : ReplyBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("sourceType")]
        public string SourceType { get; set; }
        [JsonPropertyName("sourceSettings")]
        public JsonElement SourceSettings { get; set; }
        public object SourceSettingsObj { get; set; }
    }

    public class SetSourceSettingsReply : ReplyBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("sourceType")]
        public string SourceType { get; set; }
        [JsonPropertyName("sourceSettings")]
        public JsonElement SourceSettings { get; set; }
        public object SourceSettingsObj { get; set; }

    }
    public class GetTextGDIPlusPropertiesReply : ReplyBase
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }
        [JsonPropertyName("align")]
        public string Align { get; set; }
        [JsonPropertyName("bk-color")]
        public int BkColor { get; set; }
        [JsonPropertyName("bk-opacity")]
        public int BkOpacity { get; set; }
        [JsonPropertyName("chatlog")]
        public bool Chatlog { get; set; }
        [JsonPropertyName("chatlog_lines")]
        public int ChatlogLines { get; set; }
        [JsonPropertyName("color")]
        public int Color { get; set; }
        [JsonPropertyName("extents")]
        public bool Extents { get; set; }
        [JsonPropertyName("extents_cx")]
        public int ExtentsCx { get; set; }
        [JsonPropertyName("extents_cy")]
        public int ExtentsCy { get; set; }
        [JsonPropertyName("file")]
        public string File { get; set; }
        [JsonPropertyName("read_from_file")]
        public bool ReadFromFile { get; set; }
        [JsonPropertyName("font")]
        public TypeDefs.ObsWsFont Font { get; set; }
        [JsonPropertyName("gradient")]
        public bool Gradient { get; set; }
        [JsonPropertyName("gradient_color")]
        public int GradientColor { get; set; }
        [JsonPropertyName("gradient_dir")]
        public float GradientDir { get; set; }
        [JsonPropertyName("gradient_opacity")]
        public int GradientOpacity { get; set; }
        [JsonPropertyName("outline")]
        public bool Outline { get; set; }
        [JsonPropertyName("outline_color")]
        public int OutlineColor { get; set; }
        [JsonPropertyName("outline_size")]
        public int OutlineSize { get; set; }
        [JsonPropertyName("outline_opacity")]
        public int OutlineOpacity { get; set; }
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("valign")]
        public string Valign { get; set; }
        [JsonPropertyName("vertical")]
        public bool Vertical { get; set; }
    }

    public class SetTextGDIPlusPropertiesReply : ReplyBase
    {
    }

    public class GetTextFreetype2PropertiesReply : ReplyBase
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }
        [JsonPropertyName("color1")]
        public int Color1 { get; set; }
        [JsonPropertyName("color2")]
        public int Color2 { get; set; }
        [JsonPropertyName("custom_width")]
        public int CustomWidth { get; set; }
        [JsonPropertyName("drop_shadow")]
        public bool DropShadow { get; set; }
        [JsonPropertyName("font")]
        public TypeDefs.ObsWsFont Font { get; set; }
        [JsonPropertyName("from_file")]
        public bool FromFile { get; set; }
        [JsonPropertyName("log_mode")]
        public bool LogMode { get; set; }
        [JsonPropertyName("outline")]
        public bool Outline { get; set; }
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("text_file")]
        public string TextFile { get; set; }
        [JsonPropertyName("word_wrap")]
        public bool WordWrap { get; set; }
    }

    public class SetTextFreetype2PropertiesReply : ReplyBase
    {
    }

    public class GetBrowserSourcePropertiesReply : ReplyBase
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }
        [JsonPropertyName("is_local_file")]
        public bool IsLocalFile { get; set; }
        [JsonPropertyName("local_file")]
        public string LocalFile { get; set; }
        [JsonPropertyName("url")]
        public Uri Url { get; set; }
        [JsonPropertyName("css")]
        public string Css { get; set; }
        [JsonPropertyName("width")]
        public int Width { get; set; }
        [JsonPropertyName("height")]
        public int Height { get; set; }
        [JsonPropertyName("fps")]
        public int Fps { get; set; }
        [JsonPropertyName("shutdown")]
        public bool Shutdown { get; set; }
    }

    public class SetBrowserSourcePropertiesReply : ReplyBase
    {
    }

    public class GetSpecialSourcesReply : ReplyBase
    {
        [JsonPropertyName("desktop-1")]
        public string Desktop1 { get; set; }
        [JsonPropertyName("desktop-2")]
        public string Desktop2 { get; set; }
        [JsonPropertyName("mic-1")]
        public string Mic1 { get; set; }
        [JsonPropertyName("mic-2")]
        public string Mic2 { get; set; }
        [JsonPropertyName("mic-3")]
        public string Mic3 { get; set; }
    }

    public class GetSourceFiltersReply : ReplyBase
    {
        [JsonPropertyName("filters")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TypeDefs.ObsWsReplyFilter> Filters { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class GetSourceFilterInfoReply : ReplyBase
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("settings")]
        public JsonElement Settings { get; set; }
        public object SettingsObj { get; set; }
    }

    public class AddFilterToSourceReply : ReplyBase
    {
    }

    public class RemoveFilterFromSourceReply : ReplyBase
    {
    }

    public class ReorderSourceFilterReply : ReplyBase
    {
    }

    public class MoveSourceFilterReply : ReplyBase
    {
    }

    public class SetSourceFilterSettingsReply : ReplyBase
    {
    }

    public class SetSourceFilterVisibilityReply : ReplyBase
    {
    }

    public class GetAudioMonitorTypeReply : ReplyBase
    {
        [JsonPropertyName("monitorType")]
        public string MonitorType { get; set; }
    }

    public class SetAudioMonitorTypeReply : ReplyBase
    {
    }

    public class TakeSourceScreenshotReply : ReplyBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("embedPictureFormat")]
        public string EmbedPictureFormat { get; set; }
        [JsonPropertyName("saveToFilePath")]
        public string SaveToFilePath { get; set; }
    }
}