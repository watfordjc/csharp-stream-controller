using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.TypeDefs
{

    public class MaskFilter : BaseFilter
    {
        [JsonPropertyName("opacity")]
        public int Opacity { get; set; }
    }

    public class CropFilter : BaseFilter
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

    public class ColorFilter : BaseFilter
    {
        [JsonPropertyName("opacity")]
        public int Opacity { get; set; }
    }

    public class ScaleFilter : BaseFilter
    {
        [JsonPropertyName("resolution")]
        public string Resolution { get; set; }
        [JsonPropertyName("sampling")]
        public string Sampling { get; set; }
    }

    public class ScrollFilter : BaseFilter
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

    public class GpuDelay : BaseFilter
    {
        [JsonPropertyName("gpu_delay")]
        public int DelayMs { get; set; }
    }

    public class ColorKeyFilter : BaseFilter
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

    public class ClutFilter : BaseFilter
    {
        [JsonPropertyName("clut_amount")]
        public Decimal ClutAmount { get; set; }
        [JsonPropertyName("image_path")]
        public string ImagePath { get; set; }
    }

    public class SharpnessFilter : BaseFilter
    {
        [JsonPropertyName("sharpness_filter")]
        public Decimal Sharpness { get; set; }
    }

    public class ChromaKeyFilter : BaseFilter
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

    public class AsyncDelayFilter : BaseFilter
    {
        [JsonPropertyName("delay_ms")]
        public int DelayMx { get; set; }
    }

    public class LumaKeyFilter : BaseFilter
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
}
