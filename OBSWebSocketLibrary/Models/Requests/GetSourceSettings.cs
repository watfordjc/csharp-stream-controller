using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.Requests
{
    public class GetSourceSettings
    {
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }
        [JsonPropertyName("sourceType")]
        public string SourceType { get; set; }
        [JsonPropertyName("sourceSettings")]
        public Settings SourceSettings { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }

        public class Settings
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
}
