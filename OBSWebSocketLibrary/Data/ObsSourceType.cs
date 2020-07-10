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
        Scene = 1,
        Group = 2,
        AudioLine = 3,
        ImageSource = 4,
        ColorSource = 5,
        ColorSourceV2 = 6,
        Slideshow = 7,
        BrowserSource = 8,
        FFmpegSource = 9,
        MaskFilter = 10,
        CropFilter = 11,
        GainFilter = 12,
        ColorFilter = 13,
        ScaleFilter = 14,
        ScrollFilter = 15,
        GpuDelay = 16,
        ColorKeyFilter = 17,
        ClutFilter = 18,
        SharpnessFilter = 19,
        ChromaKeyFilter = 20,
        AsyncDelayFilter = 21,
        NoiseSupressFilter = 22,
        InvertPolarityFilter = 23,
        NoiseGateFilter = 24,
        CompressorFilter = 25,
        LimiterFilter = 26,
        ExpanderFilter = 27,
        LumaKeyFilter = 28,
        TextGdiPlus = 29,
        TextGdiPlusV2 = 30,
        CutTransition = 31,
        FadeTransition = 32,
        SwipeTransition = 33,
        SlideTransition = 34,
        ObsStingerTransition = 35,
        FadeToColorTransition = 36,
        WipeTransition = 37,
        VstFilter = 38,
        TextFt2Source = 39,
        TextFt2SouceV2 = 40,
        VlcSource = 41,
        MonitorCapture = 42,
        WindowCapture = 43,
        GameCapture = 44,
        DShowInput = 45,
        WasapiInputCapture = 46,
        WasapiOutputCapture = 47
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
            { ObsSourceType.WasapiOutputCapture, typeof(TypeDefs.WasapiOutputCapture) },
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
