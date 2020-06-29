using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.RequestReplies
{
    public class GetSourcesList : RequestReplyBase
    {
        [JsonPropertyName("sources")]
        public Source[] Sources { get; set; }
        public class Source
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("typeId")]
            public string TypeId { get; set; }
            [JsonPropertyName("type")]
            public string Type { get; set; }
        }
    }

    public class GetSourceTypesList : RequestReplyBase
    {
        [JsonPropertyName("types")]
        public Type[] Types { get; set; }
        public class Type
        {
            [JsonPropertyName("typeId")]
            public string TypeId { get; set; }
            [JsonPropertyName("displayName")]
            public string DisplayName { get; set; }
            [JsonPropertyName("type")]
            public string TypeType { get; set; }
            [JsonPropertyName("defaultSettings")]
            public JsonElement DefaultSettings { get; set; }
            public object DefaultSettingsObj;
            [JsonPropertyName("caps")]
            public CapsProperties Caps { get; set; }
            public class CapsProperties
            {
                [JsonPropertyName("isAsync")]
                public bool IsAsync { get; set; }
                [JsonPropertyName("hasVideo")]
                public bool HasVideo { get; set; }
                [JsonPropertyName("hasAudio")]
                public bool HasAudio { get; set; }
                [JsonPropertyName("canInteract")]
                public bool CanInteract { get; set; }
                [JsonPropertyName("isComposite")]
                public bool IsComposite { get; set; }
                [JsonPropertyName("doNotDuplicate")]
                public bool DoNotDuplicate { get; set; }
                [JsonPropertyName("doNotSelfMonitor")]
                public bool DoNotSelfMonitor { get; set; }
            }
        }
    }

    public class GetVolume : RequestReplyBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("volume")]
        public double Volume { get; set; }
        [JsonPropertyName("muted")]
        public bool Muted { get; set; }
    }

    public class SetVolume : RequestReplyBase
    {
    }

    public class GetMute : RequestReplyBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("muted")]
        public bool Muted { get; set; }
    }

    public class SetMute : RequestReplyBase
    {
    }

    public class ToggleMute : RequestReplyBase
    {
    }

    public class SetSourceName : RequestReplyBase
    {
    }

    public class SetSyncOffset : RequestReplyBase
    {
    }

    public class GetSyncOffset : RequestReplyBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("offset")]
        public int Offset { get; set; }
    }

    public class GetSourceSettings : RequestReplyBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("sourceType")]
        public string SourceType { get; set; }
        [JsonPropertyName("sourceSettings")]
        public JsonElement SourceSettings { get; set; }
        public object SourceSettingsObj { get; set; }
    }

    public class SetSourceSettings : RequestReplyBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("sourceType")]
        public string SourceType { get; set; }
        [JsonPropertyName("sourceSettings")]
        public JsonElement SourceSettings { get; set; }
        public object SourceSettingsObj { get; set; }

    }
    public class GetTextGDIPlusProperties : RequestReplyBase
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
        public TypeDefs.Font Font { get; set; }
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

    public class SetTextGDIPlusProperties : RequestReplyBase
    {
    }

    public class GetTextFreetype2Properties : RequestReplyBase
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
        public TypeDefs.Font Font { get; set; }
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

    public class SetTextFreetype2Properties : RequestReplyBase
    {
    }

    public class GetBrowserSourceProperties : RequestReplyBase
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }
        [JsonPropertyName("is_local_file")]
        public bool IsLocalFile { get; set; }
        [JsonPropertyName("local_file")]
        public string LocalFile { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
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

    public class SetBrowserSourceProperties : RequestReplyBase
    {
    }

    public class GetSpecialSources : RequestReplyBase
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

    public class GetSourceFilters : RequestReplyBase
    {
        [JsonPropertyName("filters")]
        public Filter[] Filters { get; set; }
        public class Filter
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
    }

    public class GetSourceFilterInfo : RequestReplyBase
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

    public class AddFilterToSource : RequestReplyBase
    {
    }

    public class RemoveFilterFromSource : RequestReplyBase
    {
    }

    public class ReorderSourceFilter : RequestReplyBase
    {
    }

    public class MoveSourceFilter : RequestReplyBase
    {
    }

    public class SetSourceFilterSettings : RequestReplyBase
    {
    }

    public class SetSourceFilterVisibility : RequestReplyBase
    {
    }

    public class GetAudioMonitorType : RequestReplyBase
    {
    }

    public class SetAudioMonitorType : RequestReplyBase
    {
    }

    public class TakeSourceScreenshot : RequestReplyBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("embedPictureFormat")]
        public string EmbedPictureFormat { get; set; }
        [JsonPropertyName("saveToFilePath")]
        public string SaveToFilePath { get; set; }
    }
}