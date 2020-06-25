using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.RequestReplies
{
    public class GetVersion : RequestBase
    {
        [JsonPropertyName("version")]
        public double Version { get; set; }
        [JsonPropertyName("obs-websocket-version")]
        public string ObsWebsocketVersion { get; set; }
        [JsonPropertyName("obs-studio-version")]
        public string ObsStudioVersion { get; set; }
        [JsonPropertyName("available-requests")]
        public string AvailableRequests { get; set; }
        [JsonPropertyName("supported-image-export-formats")]
        public string SupportedImageExportFormats { get; set; }
    }

    public class GetAuthRequired : RequestBase
    {
        [JsonPropertyName("authRequired")]
        public bool AuthRequired { get; set; }
        [JsonPropertyName("challenge")]
        public string Challenge { get; set; }
        [JsonPropertyName("salt")]
        public string Salt { get; set; }
    }

    public class Authenticate : RequestBase
    {
    }

    public class SetHeartbeat : RequestBase
    {
    }

    public class SetFilenameFormatting : RequestBase
    {
    }

    public class GetFilenameFormatting : RequestBase
    {
    }

    public class GetStats : RequestBase
    {
    }

    public class BroadcastCustomMessage : RequestBase
    {
    }

    public class GetVideoInfo : RequestBase
    {
        [JsonPropertyName("baseWidth")]
        public int BaseWidth { get; set; }
        [JsonPropertyName("baseHeight")]
        public int BaseHeight { get; set; }
        [JsonPropertyName("outputWidth")]
        public int OutputWidth { get; set; }
        [JsonPropertyName("outputHeight")]
        public int OutputHeight { get; set; }
        [JsonPropertyName("scaleType")]
        public string ScaleType { get; set; }
        [JsonPropertyName("fps")]
        public double Fps { get; set; }
        [JsonPropertyName("videoFormat")]
        public string VideoFormat { get; set; }
        [JsonPropertyName("colorSpace")]
        public string ColorSpace { get; set; }
        [JsonPropertyName("colorRange")]
        public string ColorRange { get; set; }
    }

    public class OpenProjector : RequestBase
    {
    }
}
