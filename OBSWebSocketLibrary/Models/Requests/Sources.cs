using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequests
{
    public class GetMediaSourcesListRequest : RequestBase
    {
    }

    public class GetSourcesListRequest : RequestBase
    {
    }

    public class GetSourceTypesListRequest : RequestBase
    {
    }

    public class GetVolumeRequest : RequestBase
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }
        [JsonPropertyName("useDecibel")]
        public bool UseDecibel { get; set; }
    }

    public class SetVolumeRequest : RequestBase
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }
        [JsonPropertyName("volume")]
        public double Volume { get; set; }
        [JsonPropertyName("useDecibel")]
        public bool UseDecibel { get; set; }
    }

    public class GetMuteRequest : RequestBase
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }
    }

    public class SetMuteRequest : RequestBase
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }
        [JsonPropertyName("mute")]
        public bool Mute { get; set; }
    }

    public class ToggleMuteRequest : RequestBase
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }
    }

    public class GetAudioActiveRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
    }

    public class SetSourceNameRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("newName")]
        public string NewName { get; set; }
    }

    public class SetSyncOffsetRequest : RequestBase
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }
        [JsonPropertyName("offset")]
        public int Offset { get; set; }
    }

    public class GetSyncOffsetRequest : RequestBase
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }
    }

    public class GetSourceSettingsRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("sourceType")]
        public string SourceType { get; set; }
    }

    public class SetSourceSettingsRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("sourceType")]
        public string SourceType { get; set; }
        [JsonPropertyName("sourceSettings")]
        public object SourceSettings { get; set; }
    }

    public class GetTextGDIPlusPropertiesRequest : RequestBase
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }
    }

    public class SetTextGDIPlusPropertiesRequest : RequestBase
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

    public class SetTextGDIPlusPropertiesRequestTextPropertyOnly : RequestBase
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class GetTextFreetype2PropertiesRequest : RequestBase
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }
    }

    public class SetTextFreetype2PropertiesRequest : RequestBase
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

    public class GetBrowserSourcePropertiesRequest : RequestBase
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }
    }

    public class SetBrowserSourcePropertiesRequest : RequestBase
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
        [JsonPropertyName("render")]
        public bool Render { get; set; }
    }

    public class GetSpecialSourcesRequest : RequestBase
    {
    }

    public class GetSourceFiltersRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
    }

    public class GetSourceFilterInfoRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("filterName")]
        public string FilterName { get; set; }
    }

    public class AddFilterToSourceRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("filterName")]
        public string FilterName { get; set; }
        [JsonPropertyName("filterType")]
        public string FilterType { get; set; }
        [JsonPropertyName("filterSettings")]
        public object FilterSettings { get; set; }
    }

    public class RemoveFilterFromSourceRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("filterName")]
        public string FilterName { get; set; }
    }

    public class ReorderSourceFilterRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("filterName")]
        public string FilterName { get; set; }
        [JsonPropertyName("newIndex")]
        public int NewIndex { get; set; }
    }

    public class MoveSourceFilterRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("filterName")]
        public string FilterName { get; set; }
        [JsonPropertyName("movementType")]
        public string MovementType { get; set; }
    }

    public class SetSourceFilterSettingsRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("filterName")]
        public string FilterName { get; set; }
        [JsonPropertyName("filterSettings")]
        public object FilterSettings { get; set; }
    }

    public class SetSourceFilterVisibilityRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("filterName")]
        public string FilterName { get; set; }
        [JsonPropertyName("filterEnabled")]
        public bool FilterEnabled { get; set; }
    }

    public class GetAudioMonitorTypeRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
    }

    public class SetAudioMonitorTypeRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("monitorType")]
        public string MonitorType { get; set; }
    }

    public class TakeSourceScreenshotRequest : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("embedPictureFormat")]
        public string EmbedPictureFormat { get; set; }
        [JsonPropertyName("pictureFormat")]
        public string PictureFormat { get; set; }
        [JsonPropertyName("GetVersion")]
        public string GetVersion { get; set; }
        [JsonPropertyName("compressionQuality")]
        public int CompressionQuality { get; set; }
        [JsonPropertyName("width")]
        public int Width { get; set; }
        [JsonPropertyName("height")]
        public int Height { get; set; }
    }
}
