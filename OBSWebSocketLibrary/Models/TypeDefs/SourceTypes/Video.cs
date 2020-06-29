using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.TypeDefs.SourceTypes
{
    public class ImageSource
    {
        [JsonPropertyName("file")]
        public string File { get; set; }
        [JsonPropertyName("unload")]
        public bool Unload { get; set; }
    }

    public class ColorSourceV2
    {
        [JsonPropertyName("color")]
        public long Color { get; set; }
        [JsonPropertyName("height")]
        public int Height { get; set; }
        [JsonPropertyName("width")]
        public int Width { get; set; }
    }

    public class Slideshow
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

    public class BrowserSource
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

    public class FFmpegSource
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

    public class MaskFilter
    {
        [JsonPropertyName("opacity")]
        public int Opacity { get; set; }
    }

    public class ColorFilter
    {
        [JsonPropertyName("opacity")]
        public int Opacity { get; set; }
    }

    public class TextGdiPlusV2
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

    public class WindowCapture
    {
        [JsonPropertyName("cursor")]
        public bool Cursor { get; set; }
        [JsonPropertyName("method")]
        public int Method { get; set; }
        [JsonPropertyName("priority")]
        public int Priority { get; set; }
        [JsonPropertyName("window")]
        public string Window { get; set; }
    }

    public class GameCapture
    {
        [JsonPropertyName("force_scaling")]
        public bool ForceScaling { get; set; }
        [JsonPropertyName("limit_framerate")]
        public bool LimitFramerate { get; set; }
        [JsonPropertyName("scale_res")]
        public string ScaleRes { get; set; }
    }

    public class DShowInput
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
        public string VideoDeviceId { get; set; }
    }
}
