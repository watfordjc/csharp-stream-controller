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
        public File[] Files { get; set; }
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
        public string URL { get; set; }
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

    public class MaskFilter : BaseType
    {
        [JsonPropertyName("opacity")]
        public int Opacity { get; set; }
    }

    public class CropFilter : BaseType
    {
        [JsonPropertyName("cx")]
        public int Cx { get; set; }
        [JsonPropertyName("cy")]
        public int Cy { get; set; }
        [JsonPropertyName("bottom")]
        public int Bottom { get; set; }
        [JsonPropertyName("left")]
        public int Left { get; set; }
        [JsonPropertyName("relative")]
        public bool Relative { get; set; }
        [JsonPropertyName("right")]
        public int Right { get; set; }
        [JsonPropertyName("top")]
        public int Top { get; set; }
    }

    public class ColorFilter : BaseType
    {
        [JsonPropertyName("opacity")]
        public int Opacity { get; set; }
    }

    public class ScaleFilter : BaseType
    {
        [JsonPropertyName("resolution")]
        public string Resolution { get; set; }
        [JsonPropertyName("sampling")]
        public string Sampling { get; set; }
    }

    public class ScrollFilter : BaseType
    {
        [JsonPropertyName("cx")]
        public int Cx { get; set; }
        [JsonPropertyName("cy")]
        public int Cy { get; set; }
        [JsonPropertyName("limit_cx")]
        public bool LimitCx { get; set; }
        [JsonPropertyName("limit_cy")]
        public bool LimitCy { get; set; }
        [JsonPropertyName("loop")]
        public bool Loop { get; set; }
        [JsonPropertyName("speed_x")]
        public Decimal SpeedX { get; set; }
        [JsonPropertyName("speed_y")]
        public Decimal SpeedY { get; set; }
    }

    public class GpuDelay : BaseType
    {
        [JsonPropertyName("gpu_delay")]
        public int DelayMs { get; set; }
    }

    public class ColorKeyFilter : BaseType
    {
        [JsonPropertyName("brightness")]
        public Decimal Brightness { get; set; }
        [JsonPropertyName("contrast")]
        public Decimal Contrast { get; set; }
        [JsonPropertyName("gamma")]
        public Decimal Gamma { get; set; }
        [JsonPropertyName("key_color_type")]
        public string KeyColorType { get; set; }
        [JsonPropertyName("opacity")]
        public int Opacity { get; set; }
        [JsonPropertyName("similarity")]
        public int Similarity { get; set; }
        [JsonPropertyName("smoothness")]
        public int Smoothness { get; set; }
    }

    public class ClutFilter : BaseType
    {
        [JsonPropertyName("clut_amount")]
        public Decimal ClutAmount { get; set; }
        [JsonPropertyName("image_path")]
        public string ImagePath { get; set; }
    }

    public class SharpnessFilter : BaseType
    {
        [JsonPropertyName("sharpness_filter")]
        public Decimal Sharpness { get; set; }
    }

    public class ChromaKeyFilter : BaseType
    {
        [JsonPropertyName("brightness")]
        public Decimal Brightness { get; set; }
        [JsonPropertyName("contrast")]
        public Decimal Contrast { get; set; }
        [JsonPropertyName("gamma")]
        public Decimal Gamma { get; set; }
        [JsonPropertyName("key_color_type")]
        public string KeyColorType { get; set; }
        [JsonPropertyName("opacity")]
        public int Opacity { get; set; }
        [JsonPropertyName("similarity")]
        public int Similarity { get; set; }
        [JsonPropertyName("smoothness")]
        public int Smoothness { get; set; }
        [JsonPropertyName("spill")]
        public int Spill { get; set; }
    }

    public class AsyncDelayFilter : BaseType
    {
        [JsonPropertyName("delay_ms")]
        public int DelayMx { get; set; }
    }

    public class LumaKeyFilter : BaseType
    {
        [JsonPropertyName("luma_max")]
        public Decimal LumaMax { get; set; }
        [JsonPropertyName("luma_max_smooth")]
        public Decimal LumaMaxSmooth { get; set; }
        [JsonPropertyName("luma_min")]
        public Decimal LumaMin { get; set; }
        [JsonPropertyName("luma_min_smooth")]
        public Decimal LumaMinSmooth { get; set; }
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
        public PlaylistItem[] Playlist { get; set; }
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
