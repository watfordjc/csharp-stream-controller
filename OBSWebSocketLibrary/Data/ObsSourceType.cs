using System;
using System.Collections.Generic;
using System.Text;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.Data
{
    public static class ObsTypes
    {
        public static readonly Dictionary<string, ObsSourceType> ObsTypeNameDictionary = new Dictionary<string, ObsSourceType>()
        {
            { "scene", ObsSourceType.Scene },
            { "group", ObsSourceType.Group },
            { "audio_line", ObsSourceType.AudioLine },
            { "image_source", ObsSourceType.ImageSource },
            { "color_source", ObsSourceType.ColorSource },
            { "color_source_v2", ObsSourceType.ColorSourceV2 },
            { "color_source_v3", ObsSourceType.ColorSourceV3 },
            { "slideshow", ObsSourceType.Slideshow },
            { "browser_source", ObsSourceType.BrowserSource },
            { "ffmpeg_source", ObsSourceType.FFmpegSource },
            { "mask_filter", ObsSourceType.MaskFilter },
            { "crop_filter", ObsSourceType.CropFilter },
            { "gain_filter", ObsSourceType.GainFilter },
            { "color_filter", ObsSourceType.ColorFilter },
            { "scale_filter", ObsSourceType.ScaleFilter },
            { "scroll_filter", ObsSourceType.ScrollFilter },
            { "gpu_delay", ObsSourceType.GpuDelay },
            { "color_key_filter", ObsSourceType.ColorKeyFilter },
            { "clut_filter", ObsSourceType.ClutFilter },
            { "sharpness_filter", ObsSourceType.SharpnessFilter },
            { "chroma_key_filter", ObsSourceType.ChromaKeyFilter },
            { "async_delay_filter", ObsSourceType.AsyncDelayFilter },
            { "noise_suppress_filter", ObsSourceType.NoiseSupressFilter },
            { "noise_suppress_filter_v2", ObsSourceType.NoiseSupressFilterV2 },
            { "invert_polarity_filter", ObsSourceType.InvertPolarityFilter },
            { "noise_gate_filter", ObsSourceType.NoiseGateFilter },
            { "compressor_filter", ObsSourceType.CompressorFilter },
            { "limiter_filter", ObsSourceType.LimiterFilter },
            { "expander_filter", ObsSourceType.ExpanderFilter },
            { "luma_key_filter", ObsSourceType.LumaKeyFilter },
            { "text_gdiplus", ObsSourceType.TextGdiPlus },
            { "text_gdiplus_v2", ObsSourceType.TextGdiPlusV2 },
            { "cut_transition", ObsSourceType.CutTransition },
            { "fade_transition", ObsSourceType.FadeTransition },
            { "swipe_transition", ObsSourceType.SwipeTransition },
            { "slide_transition", ObsSourceType.SlideTransition },
            { "obs_stinger_transition", ObsSourceType.ObsStingerTransition },
            { "fade_to_color_transition", ObsSourceType.FadeToColorTransition },
            { "wipe_transition", ObsSourceType.WipeTransition },
            { "vst_filter", ObsSourceType.VstFilter },
            { "text_ft2_source", ObsSourceType.TextFt2Source },
            { "text_ft2_source_v2", ObsSourceType.TextFt2SouceV2 },
            { "vlc_source", ObsSourceType.VlcSource },
            { "monitor_capture", ObsSourceType.MonitorCapture },
            { "window_capture", ObsSourceType.WindowCapture },
            { "game_capture", ObsSourceType.GameCapture },
            { "dshow_input", ObsSourceType.DShowInput },
            { "wasapi_input_capture", ObsSourceType.WasapiInputCapture },
            { "wasapi_output_capture", ObsSourceType.WasapiOutputCapture }
        };
    }

    public enum ObsSourceType
    {
        Unknown = 0,
        Scene = 1,
        Group = 2,
        AudioLine = 3,
        ImageSource = 4,
        ColorSource = 5,
        ColorSourceV2 = 6,
        ColorSourceV3 = 7,
        Slideshow = 8,
        BrowserSource = 9,
        FFmpegSource = 10,
        MaskFilter = 11,
        CropFilter = 12,
        GainFilter = 13,
        ColorFilter = 14,
        ScaleFilter = 15,
        ScrollFilter = 16,
        GpuDelay = 17,
        ColorKeyFilter = 18,
        ClutFilter = 19,
        SharpnessFilter = 20,
        ChromaKeyFilter = 21,
        AsyncDelayFilter = 22,
        NoiseSupressFilter = 23,
        NoiseSupressFilterV2 = 24,
        InvertPolarityFilter = 25,
        NoiseGateFilter = 26,
        CompressorFilter = 27,
        LimiterFilter = 28,
        ExpanderFilter = 29,
        LumaKeyFilter = 30,
        TextGdiPlus = 31,
        TextGdiPlusV2 = 32,
        CutTransition = 33,
        FadeTransition = 34,
        SwipeTransition = 35,
        SlideTransition = 36,
        ObsStingerTransition = 37,
        FadeToColorTransition = 38,
        WipeTransition = 39,
        VstFilter = 40,
        TextFt2Source = 41,
        TextFt2SouceV2 = 42,
        VlcSource = 43,
        MonitorCapture = 44,
        WindowCapture = 45,
        GameCapture = 46,
        DShowInput = 47,
        WasapiInputCapture = 48,
        WasapiOutputCapture = 49
    }

    public static class ObsWsSourceType
    {
        private static readonly Dictionary<ObsSourceType, Type> sourceTypeSettingsDictionary = new Dictionary<ObsSourceType, Type>()
        {
            { ObsSourceType.Scene, typeof(TypeDefs.Scene) },
            { ObsSourceType.Group, typeof(TypeDefs.Group) },
            { ObsSourceType.AudioLine, typeof(TypeDefs.AudioLine) },
            { ObsSourceType.ImageSource, typeof(TypeDefs.ImageSource) },
            { ObsSourceType.ColorSource, typeof(TypeDefs.ColorSource) },
            { ObsSourceType.ColorSourceV2, typeof(TypeDefs.ColorSourceV2) },
            { ObsSourceType.ColorSourceV3, typeof(TypeDefs.ColorSourceV3) },
            { ObsSourceType.Slideshow, typeof(TypeDefs.Slideshow) },
            { ObsSourceType.BrowserSource, typeof(TypeDefs.BrowserSource) },
            { ObsSourceType.FFmpegSource, typeof(TypeDefs.FFmpegSource) },
            { ObsSourceType.MaskFilter, typeof(TypeDefs.MaskFilter) },
            { ObsSourceType.CropFilter, typeof(TypeDefs.CropFilter) },
            { ObsSourceType.GainFilter, typeof(TypeDefs.GainFilter) },
            { ObsSourceType.ColorFilter, typeof(TypeDefs.ColorFilter) },
            { ObsSourceType.ScaleFilter, typeof(TypeDefs.ScaleFilter) },
            { ObsSourceType.ScrollFilter, typeof(TypeDefs.ScrollFilter) },
            { ObsSourceType.GpuDelay, typeof(TypeDefs.GpuDelay) },
            { ObsSourceType.ColorKeyFilter, typeof(TypeDefs.ColorKeyFilter) },
            { ObsSourceType.ClutFilter, typeof(TypeDefs.ClutFilter) },
            { ObsSourceType.SharpnessFilter, typeof(TypeDefs.SharpnessFilter) },
            { ObsSourceType.ChromaKeyFilter, typeof(TypeDefs.ChromaKeyFilter) },
            { ObsSourceType.AsyncDelayFilter, typeof(TypeDefs.AsyncDelayFilter) },
            { ObsSourceType.NoiseSupressFilter, typeof(TypeDefs.NoiseSupressFilter) },
            { ObsSourceType.NoiseSupressFilterV2, typeof(TypeDefs.NoiseSupressFilterV2) },
            { ObsSourceType.InvertPolarityFilter, typeof(TypeDefs.InvertPolarityFilter) },
            { ObsSourceType.NoiseGateFilter, typeof(TypeDefs.NoiseGateFilter) },
            { ObsSourceType.CompressorFilter, typeof(TypeDefs.CompressorFilter) },
            { ObsSourceType.LimiterFilter, typeof(TypeDefs.LimiterFilter) },
            { ObsSourceType.ExpanderFilter, typeof(TypeDefs.ExpanderFilter) },
            { ObsSourceType.LumaKeyFilter, typeof(TypeDefs.LumaKeyFilter) },
            { ObsSourceType.TextGdiPlus, typeof(TypeDefs.TextGdiPlus) },
            { ObsSourceType.TextGdiPlusV2, typeof(TypeDefs.TextGdiPlusV2) },
            { ObsSourceType.CutTransition, typeof(TypeDefs.CutTransition) },
            { ObsSourceType.FadeTransition, typeof(TypeDefs.FadeTransition) },
            { ObsSourceType.SwipeTransition, typeof(TypeDefs.SwipeTransition) },
            { ObsSourceType.SlideTransition, typeof(TypeDefs.SlideTransition) },
            { ObsSourceType.ObsStingerTransition, typeof(TypeDefs.ObsStingerTransition) },
            { ObsSourceType.FadeToColorTransition, typeof(TypeDefs.FadeToColorTransition) },
            { ObsSourceType.WipeTransition, typeof(TypeDefs.WipeTransition) },
            { ObsSourceType.VstFilter, typeof(TypeDefs.VstFilter) },
            { ObsSourceType.TextFt2Source, typeof(TypeDefs.TextFt2Source) },
            { ObsSourceType.TextFt2SouceV2, typeof(TypeDefs.TextFt2SouceV2) },
            { ObsSourceType.VlcSource, typeof(TypeDefs.VlcSource) },
            { ObsSourceType.MonitorCapture, typeof(TypeDefs.MonitorCapture) },
            { ObsSourceType.WindowCapture, typeof(TypeDefs.WindowCapture) },
            { ObsSourceType.GameCapture, typeof(TypeDefs.GameCapture) },
            { ObsSourceType.DShowInput, typeof(TypeDefs.DShowInput) },
            { ObsSourceType.WasapiInputCapture, typeof(TypeDefs.WasapiInputCapture) },
            { ObsSourceType.WasapiOutputCapture, typeof(TypeDefs.WasapiOutputCapture) }
        };

        public static Type GetType(ObsSourceType sourceType)
        {
            return sourceTypeSettingsDictionary.TryGetValue(sourceType, out Type value) ? value : null;
        }

        public static Type GetType(string obsType)
        {
            return GetType(ObsTypes.ObsTypeNameDictionary[obsType]);
        }

        public static object GetInstanceOfType(ObsSourceType sourceType)
        {
            return sourceTypeSettingsDictionary.TryGetValue(sourceType, out Type value) ? Activator.CreateInstance(value) : null;
        }
    }
}
