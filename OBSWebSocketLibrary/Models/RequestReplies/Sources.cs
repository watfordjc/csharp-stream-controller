using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.RequestReplies
{
    public class GetSourcesList : RequestBase
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

    public class GetSourceTypesList : RequestBase
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
            public TypeDefaultSettings DefaultSettings { get; set; }
            public class TypeDefaultSettings
            {
            }
            [JsonPropertyName("caps")]
            public TypeCaps Caps { get; set; }
            public class TypeCaps
            {
                [JsonPropertyName("isAsync")]
                public bool CapsIsAsync { get; set; }
                [JsonPropertyName("hasVideo")]
                public bool CapsHasVideo { get; set; }
                [JsonPropertyName("hasAudio")]
                public bool CapsHasAudio { get; set; }
                [JsonPropertyName("canInteract")]
                public bool CapsCanInteract { get; set; }
                [JsonPropertyName("isComposite")]
                public bool CapsIsComposite { get; set; }
                [JsonPropertyName("doNotDuplicate")]
                public bool CapsDoNotDuplicate { get; set; }
                [JsonPropertyName("doNotSelfMonitor")]
                public bool CapsDoNotSelfMonitor { get; set; }
            }
        }
    }

    public class GetVolume : RequestBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("volume")]
        public double Volume { get; set; }
        [JsonPropertyName("muted")]
        public bool Muted { get; set; }
    }

    public class SetVolume : RequestBase
    {
    }

    public class GetMute : RequestBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("muted")]
        public bool Muted { get; set; }
    }

    public class SetMute : RequestBase
    {
    }

    public class ToggleMute : RequestBase
    {
    }

    public class SetSourceName : RequestBase
    {
    }

    public class SetSyncOffset : RequestBase
    {
    }

    public class GetSyncOffset : RequestBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("offset")]
        public int Offset { get; set; }
    }

    public class GetSourceSettings : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("sourceType")]
        public string SourceType { get; set; }
        [JsonPropertyName("sourceSettings")]
        public SourceSettingsSettings SourceSettings { get; set; }
        public class SourceSettingsSettings
        {
            [JsonPropertyName("device_id")]
            public string DeviceID { get; set; }
            [JsonPropertyName("use_device_timing")]
            public bool UseDeviceTiming { get; set; }
            [JsonPropertyName("height")]
            public int Height { get; set; }
            [JsonPropertyName("width")]
            public int Width { get; set; }
            [JsonPropertyName("align")]
            public string Align { get; set; }
            [JsonPropertyName("text")]
            public string Text { get; set; }
            [JsonPropertyName("font")]
            public FontSettings Font { get; set; }
            [JsonPropertyName("file")]
            public string File { get; set; }
            [JsonPropertyName("color")]
            public long Color { get; set; }
            [JsonPropertyName("cursor")]
            public bool Cursor { get; set; }
            [JsonPropertyName("method")]
            public int Method { get; set; }
            [JsonPropertyName("priority")]
            public int Priority { get; set; }
            [JsonPropertyName("window")]
            public string Window { get; set; }
            [JsonPropertyName("css")]
            public string CSS { get; set; }
            [JsonPropertyName("url")]
            public string URL { get; set; }
            [JsonPropertyName("reroute_audio")]
            public bool RerouteAudio { get; set; }
            [JsonPropertyName("restart_when_active")]
            public bool RestartWhenActive { get; set; }
            [JsonPropertyName("shutdown")]
            public bool Shutdown { get; set; }
            [JsonPropertyName("valign")]
            public string Valign { get; set; }
            [JsonPropertyName("chatlog")]
            public bool Chatlog { get; set; }
            [JsonPropertyName("extents")]
            public bool Extents { get; set; }
            [JsonPropertyName("outline")]
            public bool Outline { get; set; }
            [JsonPropertyName("vertical")]
            public bool Vertical { get; set; }
            [JsonPropertyName("bk_color")]
            public long BkColor { get; set; }
            [JsonPropertyName("bk_opacity")]
            public int bk_opacity { get; set; }
            [JsonPropertyName("unload")]
            public bool Unload { get; set; }
            [JsonPropertyName("scale_res")]
            public string ScaleRes { get; set; }
            [JsonPropertyName("force_scaling")]
            public bool ForceScaling { get; set; }
            [JsonPropertyName("limit_framerate")]
            public bool LimitFramerate { get; set; }
            [JsonPropertyName("sourceName")]
            public bool SourceName { get; set; }
            [JsonPropertyName("active")]
            public bool Active { get; set; }
            [JsonPropertyName("deactive_when_not_showing")]
            public bool DeactiveWhenNotShowing { get; set; }
            [JsonPropertyName("use_custom_audio_device")]
            public bool UseCustomAudioDevice { get; set; }
            [JsonPropertyName("last_video_device_id")]
            public string LastVideoDeviceId { get; set; }
            [JsonPropertyName("video_device_id")]
            public string VideoDeviceId { get; set; }
            [JsonPropertyName("audio_device_id")]
            public string AudioDeviceId { get; set; }
            [JsonPropertyName("color_space")]
            public string ColorSpace { get; set; }
            [JsonPropertyName("flip_vertically")]
            public bool FlipVertically { get; set; }
            [JsonPropertyName("files")]
            public FileSettings[] Files { get; set; }
            [JsonPropertyName("playback_behavior")]
            public string PlaybackBehavior { get; set; }
            [JsonPropertyName("value")]
            public string Value { get; set; }
            [JsonPropertyName("selected")]
            public bool Selected { get; set; }
            [JsonPropertyName("transition")]
            public string Transition { get; set; }
            [JsonPropertyName("use_custom_size")]
            public string UseCustomSize { get; set; }
            [JsonPropertyName("slide_time")]
            public int SlideTime { get; set; }
            [JsonPropertyName("local_file")]
            public string LocalFile { get; set; }
            [JsonPropertyName("clear_on_media_end")]
            public bool ClearOnMediaEnd { get; set; }
            [JsonPropertyName("close_when_inactive")]
            public bool CloseWhenInactive { get; set; }
            [JsonPropertyName("hw_decode")]
            public bool HwDecode { get; set; }
            [JsonPropertyName("gradient")]
            public bool Gradient { get; set; }
            [JsonPropertyName("read_from_file")]
            public bool ReadFromFile { get; set; }
            public class FontSettings
            {
                [JsonPropertyName("face")]
                public string Face { get; set; }
                [JsonPropertyName("flags")]
                public int Flags { get; set; }
                [JsonPropertyName("size")]
                public int Size { get; set; }
                [JsonPropertyName("style")]
                public string Style { get; set; }
            }

            public class FileSettings
            {
                [JsonPropertyName("hidden")]
                public bool Hidden { get; set; }
                [JsonPropertyName("selected")]
                public bool Selected { get; set; }
                [JsonPropertyName("value")]
                public string Value { get; set; }
            }
        }
    }

    public class SetSourceSettings : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("sourceType")]
        public string SourceType { get; set; }
        [JsonPropertyName("sourceSettings")]
        public SourceSettingsSettings SourceSettings { get; set; }
        public class SourceSettingsSettings
        {
            [JsonPropertyName("device_id")]
            public string DeviceID { get; set; }
            [JsonPropertyName("use_device_timing")]
            public bool UseDeviceTiming { get; set; }
            [JsonPropertyName("height")]
            public int Height { get; set; }
            [JsonPropertyName("width")]
            public int Width { get; set; }
            [JsonPropertyName("align")]
            public string Align { get; set; }
            [JsonPropertyName("text")]
            public string Text { get; set; }
            [JsonPropertyName("font")]
            public FontSettings Font { get; set; }
            [JsonPropertyName("file")]
            public string File { get; set; }
            [JsonPropertyName("color")]
            public long Color { get; set; }
            [JsonPropertyName("cursor")]
            public bool Cursor { get; set; }
            [JsonPropertyName("method")]
            public int Method { get; set; }
            [JsonPropertyName("priority")]
            public int Priority { get; set; }
            [JsonPropertyName("window")]
            public string Window { get; set; }
            [JsonPropertyName("css")]
            public string CSS { get; set; }
            [JsonPropertyName("url")]
            public string URL { get; set; }
            [JsonPropertyName("reroute_audio")]
            public bool RerouteAudio { get; set; }
            [JsonPropertyName("restart_when_active")]
            public bool RestartWhenActive { get; set; }
            [JsonPropertyName("shutdown")]
            public bool Shutdown { get; set; }
            [JsonPropertyName("valign")]
            public string Valign { get; set; }
            [JsonPropertyName("chatlog")]
            public bool Chatlog { get; set; }
            [JsonPropertyName("extents")]
            public bool Extents { get; set; }
            [JsonPropertyName("outline")]
            public bool Outline { get; set; }
            [JsonPropertyName("vertical")]
            public bool Vertical { get; set; }
            [JsonPropertyName("bk_color")]
            public long BkColor { get; set; }
            [JsonPropertyName("bk_opacity")]
            public int bk_opacity { get; set; }
            [JsonPropertyName("unload")]
            public bool Unload { get; set; }
            [JsonPropertyName("scale_res")]
            public string ScaleRes { get; set; }
            [JsonPropertyName("force_scaling")]
            public bool ForceScaling { get; set; }
            [JsonPropertyName("limit_framerate")]
            public bool LimitFramerate { get; set; }
            [JsonPropertyName("sourceName")]
            public bool SourceName { get; set; }
            [JsonPropertyName("active")]
            public bool Active { get; set; }
            [JsonPropertyName("deactive_when_not_showing")]
            public bool DeactiveWhenNotShowing { get; set; }
            [JsonPropertyName("use_custom_audio_device")]
            public bool UseCustomAudioDevice { get; set; }
            [JsonPropertyName("last_video_device_id")]
            public string LastVideoDeviceId { get; set; }
            [JsonPropertyName("video_device_id")]
            public string VideoDeviceId { get; set; }
            [JsonPropertyName("audio_device_id")]
            public string AudioDeviceId { get; set; }
            [JsonPropertyName("color_space")]
            public string ColorSpace { get; set; }
            [JsonPropertyName("flip_vertically")]
            public bool FlipVertically { get; set; }
            [JsonPropertyName("files")]
            public FileSettings[] Files { get; set; }
            [JsonPropertyName("playback_behavior")]
            public string PlaybackBehavior { get; set; }
            [JsonPropertyName("value")]
            public string Value { get; set; }
            [JsonPropertyName("selected")]
            public bool Selected { get; set; }
            [JsonPropertyName("transition")]
            public string Transition { get; set; }
            [JsonPropertyName("use_custom_size")]
            public string UseCustomSize { get; set; }
            [JsonPropertyName("slide_time")]
            public int SlideTime { get; set; }
            [JsonPropertyName("local_file")]
            public string LocalFile { get; set; }
            [JsonPropertyName("clear_on_media_end")]
            public bool ClearOnMediaEnd { get; set; }
            [JsonPropertyName("close_when_inactive")]
            public bool CloseWhenInactive { get; set; }
            [JsonPropertyName("hw_decode")]
            public bool HwDecode { get; set; }
            [JsonPropertyName("gradient")]
            public bool Gradient { get; set; }
            [JsonPropertyName("read_from_file")]
            public bool ReadFromFile { get; set; }
            public class FontSettings
            {
                [JsonPropertyName("face")]
                public string Face { get; set; }
                [JsonPropertyName("flags")]
                public int Flags { get; set; }
                [JsonPropertyName("size")]
                public int Size { get; set; }
                [JsonPropertyName("style")]
                public string Style { get; set; }
            }

            public class FileSettings
            {
                [JsonPropertyName("hidden")]
                public bool Hidden { get; set; }
                [JsonPropertyName("selected")]
                public bool Selected { get; set; }
                [JsonPropertyName("value")]
                public string Value { get; set; }
            }
        }
    }
    public class GetTextGDIPlusProperties : RequestBase
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

    public class SetTextGDIPlusProperties : RequestBase
    {
    }

    public class GetTextFreetype2Properties : RequestBase
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

    public class SetTextFreetype2Properties : RequestBase
    {
    }

    public class GetBrowserSourceProperties : RequestBase
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

    public class SetBrowserSourceProperties : RequestBase
    {
    }

    public class GetSpecialSources : RequestBase
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

    public class GetSourceFilters : RequestBase
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
            public FilterSettings Settings { get; set; }
            public class FilterSettings
            {
            }
        }
    }

    public class GetSourceFilterInfo : RequestBase
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("settings")]
        public FilterSettings Settings { get; set; }
        public class FilterSettings
        {
        }
    }

    public class AddFilterToSource : RequestBase
    {
    }

    public class RemoveFilterFromSource : RequestBase
    {
    }

    public class ReorderSourceFilter : RequestBase
    {
    }

    public class MoveSourceFilter : RequestBase
    {
    }

    public class SetSourceFilterSettings : RequestBase
    {
    }

    public class SetSourceFilterVisibility : RequestBase
    {
    }

    public class GetAudioMonitorType : RequestBase
    {
    }

    public class SetAudioMonitorType : RequestBase
    {
    }

    public class TakeSourceScreenshot : RequestBase
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("embedPictureFormat")]
        public string EmbedPictureFormat { get; set; }
        [JsonPropertyName("saveToFilePath")]
        public string SaveToFilePath { get; set; }
    }
}