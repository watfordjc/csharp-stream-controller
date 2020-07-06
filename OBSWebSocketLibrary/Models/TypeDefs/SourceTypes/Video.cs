using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.TypeDefs.SourceTypes
{
    public class ImageSource : BaseType
    {
        [JsonPropertyName("file")]
        public string File { get; set; }
        [JsonPropertyName("unload")]
        public bool Unload { get; set; }
    }

    public class ColorSource : ColorSourceV2
    {
    }

    public class ColorSourceV2 : BaseType
    {
        [JsonPropertyName("color")]
        public long Color { get; set; }
        [JsonPropertyName("height")]
        public int Height { get; set; }
        [JsonPropertyName("width")]
        public int Width { get; set; }
    }

    public class Slideshow : BaseType
    {
        [JsonPropertyName("files")]
        public IList<File> Files { get; set; }
        public class File
        {
            [JsonPropertyName("hidden")]
            public bool Hidden { get; set; }
            [JsonPropertyName("selected")]
            public bool Selected { get; set; }
            [JsonPropertyName("value")]
            public string Value { get; set; }
        }
        [JsonPropertyName("playback_behavior")]
        public string PlaybackBehavior { get; set; }
        [JsonPropertyName("slide_time")]
        public int SlideTime { get; set; }
        [JsonPropertyName("transition")]
        public string Transition { get; set; }
        [JsonPropertyName("use_custom_size")]
        public string UseCustomSize { get; set; }
    }

    public class BrowserSource : BaseType
    {
        [JsonPropertyName("css")]
        public string CSS { get; set; }
        [JsonPropertyName("height")]
        public int Height { get; set; }
        [JsonPropertyName("reroute_audio")]
        public bool RerouteAudio { get; set; }
        [JsonPropertyName("restart_when_active")]
        public bool RestartWhenActive { get; set; }
        [JsonPropertyName("shutdown")]
        public bool Shutdown { get; set; }
        [JsonPropertyName("url")]
        public Uri URL { get; set; }
        [JsonPropertyName("width")]
        public int Width { get; set; }
    }

    public class FFmpegSource : BaseType
    {
        [JsonPropertyName("clear_on_media_end")]
        public bool ClearOnMediaEnd { get; set; }
        [JsonPropertyName("close_when_inactive")]
        public bool CloseWhenInactive { get; set; }
        [JsonPropertyName("hw_decode")]
        public bool HwDecode { get; set; }
        [JsonPropertyName("local_file")]
        public string LocalFile { get; set; }
    }

    public class TextGdiPlus : TextGdiPlusV2
    {
    }

    public class TextGdiPlusV2 : BaseType
    {
        [JsonPropertyName("align")]
        public string Align { get; set; }
        [JsonPropertyName("bk_color")]
        public long BkColor { get; set; }
        [JsonPropertyName("bk_opacity")]
        public int BkOpacity { get; set; }
        [JsonPropertyName("chatlog")]
        public bool Chatlog { get; set; }
        [JsonPropertyName("color")]
        public long Color { get; set; }
        [JsonPropertyName("extents")]
        public bool Extents { get; set; }
        [JsonPropertyName("file")]
        public string File { get; set; }
        [JsonPropertyName("font")]
        public TypeDefs.Font Font { get; set; }
        [JsonPropertyName("outline")]
        public bool Outline { get; set; }
        [JsonPropertyName("gradient")]
        public bool Gradient { get; set; }
        [JsonPropertyName("read_from_file")]
        public bool ReadFromFile { get; set; }
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("valign")]
        public string VAlign { get; set; }
        [JsonPropertyName("vertical")]
        public bool Vertical { get; set; }
    }

    public class CutTransition : BaseType
    {
    }

    public class FadeTransition : BaseType
    {
    }

    public class SwipeTransition : BaseType
    {
    }

    public class SlideTransition : BaseType
    {
    }

    public class ObsStingerTransition : BaseType
    {
    }

    public class FadeToColorTransition : BaseType
    {
    }

    public class WipeTransition : BaseType
    {
    }

    public class TextFt2Source : TextFt2SouceV2
    {
    }

    public class TextFt2SouceV2 : BaseType
    {
        [JsonPropertyName("color1")]
        public long Color1 { get; set; }
        [JsonPropertyName("color2")]
        public long Color2 { get; set; }
        [JsonPropertyName("custom_width")]
        public int CustomWidth { get; set; }
        [JsonPropertyName("drop_shadow")]
        public bool DropShadow { get; set; }
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

    public class VlcSource : BaseType
    {
        [JsonPropertyName("loop")]
        public bool Loop { get; set; }
        [JsonPropertyName("network_caching")]
        public int NetworkCaching { get; set; }
        [JsonPropertyName("playback_behavior")]
        public string PlaybackBehavior { get; set; }
        [JsonPropertyName("playlist")]
        public IList<PlaylistItem> Playlist { get; set; }
        public class PlaylistItem
        {
            [JsonPropertyName("hidden")]
            public bool Hidden { get; set; }
            [JsonPropertyName("selected")]
            public bool Selected { get; set; }
            [JsonPropertyName("value")]
            public string Value { get; set; }
        }
        [JsonPropertyName("shuffle")]
        public bool Shuffle { get; set; }
        [JsonPropertyName("subtitle")]
        public int Subtitle { get; set; }
        [JsonPropertyName("subtitle_enable")]
        public bool SubtitleEnable { get; set; }
    }

    public class MonitorCapture : BaseType
    {
        [JsonPropertyName("capture_cursor")]
        public bool CaptureCursor { get; set; }
        [JsonPropertyName("monitor")]
        public int Monitor { get; set; }
    }

    public class WindowCapture : BaseType
    {
        [JsonPropertyName("cursor")]
        public bool Cursor { get; set; }
        [JsonPropertyName("client_area")]
        public bool ClientArea { get; set; }
        [JsonPropertyName("method")]
        public int Method { get; set; }
        [JsonPropertyName("priority")]
        public int Priority { get; set; }
        [JsonPropertyName("window")]
        public string Window { get; set; }
    }

    public class GameCapture : BaseType
    {
        [JsonPropertyName("force_scaling")]
        public bool ForceScaling { get; set; }
        [JsonPropertyName("limit_framerate")]
        public bool LimitFramerate { get; set; }
        [JsonPropertyName("scale_res")]
        public string ScaleRes { get; set; }
    }

    public class DShowInput : BaseType
    {
        [JsonPropertyName("active")]
        public bool Active { get; set; }
        [JsonPropertyName("audio_device_id")]
        public string AudioDeviceId { get; set; }
        [JsonPropertyName("color_space")]
        public string ColorSpace { get; set; }
        [JsonPropertyName("deactivate_when_not_showing")]
        public bool DeactivateWhenNotShowing { get; set; }
        [JsonPropertyName("flip_vertically")]
        public bool FlipVertically { get; set; }
        [JsonPropertyName("last_video_device_id")]
        public string LastVideoDeviceId { get; set; }
        [JsonPropertyName("use_custom_audio_device")]
        public bool UseCustomAudioDevice { get; set; }
        [JsonPropertyName("video_device_id")]
        public string VideoDeviceId
        {
            get
            {
                return Dependencies.VideoDeviceId;
            }
            set
            {
                Dependencies.VideoDeviceId = value;
            }
        }
    }
}
