using System;
using System.Collections.Generic;
using System.Text;

namespace OBSWebSocketLibrary.Data
{
    public static class ObsTypes
    {
        public static readonly Dictionary<string, SourceType> ObsTypeNameDictionary = new Dictionary<string, SourceType>()
        {
            { "scene", SourceType.Scene },
            { "group", SourceType.Group },
            { "audio_line", SourceType.AudioLine },
            { "image_source", SourceType.ImageSource },
            { "color_source", SourceType.ColorSource },
            { "color_source_v2", SourceType.ColorSourceV2 },
            { "slideshow", SourceType.Slideshow },
            { "browser_source", SourceType.BrowserSource },
            { "ffmpeg_source", SourceType.FFmpegSource },
            { "mask_filter", SourceType.MaskFilter },
            { "crop_filter", SourceType.CropFilter },
            { "gain_filter", SourceType.GainFilter },
            { "color_filter", SourceType.ColorFilter },
            { "scale_filter", SourceType.ScaleFilter },
            { "scroll_filter", SourceType.ScrollFilter },
            { "gpu_delay", SourceType.GpuDelay },
            { "color_key_filter", SourceType.ColorKeyFilter },
            { "clut_filter", SourceType.ClutFilter },
            { "sharpness_filter", SourceType.SharpnessFilter },
            { "chroma_key_filter", SourceType.ChromaKeyFilter },
            { "async_delay_filter", SourceType.AsyncDelayFilter },
            { "noise_suppress_filter", SourceType.NoiseSupressFilter },
            { "invert_polarity_filter", SourceType.InvertPolarityFilter },
            { "noise_gate_filter", SourceType.NoiseGateFilter },
            { "compressor_filter", SourceType.CompressorFilter },
            { "limiter_filter", SourceType.LimiterFilter },
            { "expander_filter", SourceType.ExpanderFilter },
            { "luma_key_filter", SourceType.LumaKeyFilter },
            { "text_gdiplus", SourceType.TextGdiPlus },
            { "text_gdiplus_v2", SourceType.TextGdiPlusV2 },
            { "cut_transition", SourceType.CutTransition },
            { "fade_transition", SourceType.FadeTransition },
            { "swipe_transition", SourceType.SwipeTransition },
            { "slide_transition", SourceType.SlideTransition },
            { "obs_stinger_transition", SourceType.ObsStingerTransition },
            { "fade_to_color_transition", SourceType.FadeToColorTransition },
            { "wipe_transition", SourceType.WipeTransition },
            { "vst_filter", SourceType.VstFilter },
            { "text_ft2_source", SourceType.TextFt2Source },
            { "text_ft2_source_v2", SourceType.TextFt2SouceV2 },
            { "vlc_source", SourceType.VlcSource },
            { "monitor_capture", SourceType.MonitorCapture },
            { "window_capture", SourceType.WindowCapture },
            { "game_capture", SourceType.GameCapture },
            { "dshow_input", SourceType.DShowInput },
            { "wasapi_input_capture", SourceType.WasapiInputCapture },
            { "wasapi_output_capture", SourceType.WasapiOutputCapture }
        };
    }

    public enum SourceType
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

    public static class SourceTypeSettings
    {
        private static readonly Dictionary<SourceType, Type> sourceTypeSettingsDictionary = new Dictionary<SourceType, Type>()
        {
            { SourceType.Scene, typeof(Models.TypeDefs.SourceTypes.Scene) },
            { SourceType.Group, typeof(Models.TypeDefs.SourceTypes.Group) },
            { SourceType.AudioLine, typeof(Models.TypeDefs.SourceTypes.AudioLine) },
            { SourceType.ImageSource, typeof(Models.TypeDefs.SourceTypes.ImageSource) },
            { SourceType.ColorSource, typeof(Models.TypeDefs.SourceTypes.ColorSource) },
            { SourceType.ColorSourceV2, typeof(Models.TypeDefs.SourceTypes.ColorSourceV2) },
            { SourceType.Slideshow, typeof(Models.TypeDefs.SourceTypes.Slideshow) },
            { SourceType.BrowserSource, typeof(Models.TypeDefs.SourceTypes.BrowserSource) },
            { SourceType.FFmpegSource, typeof(Models.TypeDefs.SourceTypes.FFmpegSource) },
            { SourceType.MaskFilter, typeof(Models.TypeDefs.FilterTypes.MaskFilter) },
            { SourceType.CropFilter, typeof(Models.TypeDefs.FilterTypes.CropFilter) },
            { SourceType.GainFilter, typeof(Models.TypeDefs.FilterTypes.GainFilter) },
            { SourceType.ColorFilter, typeof(Models.TypeDefs.FilterTypes.ColorFilter) },
            { SourceType.ScaleFilter, typeof(Models.TypeDefs.FilterTypes.ScaleFilter) },
            { SourceType.ScrollFilter, typeof(Models.TypeDefs.FilterTypes.ScrollFilter) },
            { SourceType.GpuDelay, typeof(Models.TypeDefs.FilterTypes.GpuDelay) },
            { SourceType.ColorKeyFilter, typeof(Models.TypeDefs.FilterTypes.ColorKeyFilter) },
            { SourceType.ClutFilter, typeof(Models.TypeDefs.FilterTypes.ClutFilter) },
            { SourceType.SharpnessFilter, typeof(Models.TypeDefs.FilterTypes.SharpnessFilter) },
            { SourceType.ChromaKeyFilter, typeof(Models.TypeDefs.FilterTypes.ChromaKeyFilter) },
            { SourceType.AsyncDelayFilter, typeof(Models.TypeDefs.FilterTypes.AsyncDelayFilter) },
            { SourceType.NoiseSupressFilter, typeof(Models.TypeDefs.FilterTypes.NoiseSupressFilter) },
            { SourceType.InvertPolarityFilter, typeof(Models.TypeDefs.FilterTypes.InvertPolarityFilter) },
            { SourceType.NoiseGateFilter, typeof(Models.TypeDefs.FilterTypes.NoiseGateFilter) },
            { SourceType.CompressorFilter, typeof(Models.TypeDefs.FilterTypes.CompressorFilter) },
            { SourceType.LimiterFilter, typeof(Models.TypeDefs.FilterTypes.LimiterFilter) },
            { SourceType.ExpanderFilter, typeof(Models.TypeDefs.FilterTypes.ExpanderFilter) },
            { SourceType.LumaKeyFilter, typeof(Models.TypeDefs.FilterTypes.LumaKeyFilter) },
            { SourceType.TextGdiPlus, typeof(Models.TypeDefs.SourceTypes.TextGdiPlus) },
            { SourceType.TextGdiPlusV2, typeof(Models.TypeDefs.SourceTypes.TextGdiPlusV2) },
            { SourceType.CutTransition, typeof(Models.TypeDefs.SourceTypes.CutTransition) },
            { SourceType.FadeTransition, typeof(Models.TypeDefs.SourceTypes.FadeTransition) },
            { SourceType.SwipeTransition, typeof(Models.TypeDefs.SourceTypes.SwipeTransition) },
            { SourceType.SlideTransition, typeof(Models.TypeDefs.SourceTypes.SlideTransition) },
            { SourceType.ObsStingerTransition, typeof(Models.TypeDefs.SourceTypes.ObsStingerTransition) },
            { SourceType.FadeToColorTransition, typeof(Models.TypeDefs.SourceTypes.FadeToColorTransition) },
            { SourceType.WipeTransition, typeof(Models.TypeDefs.SourceTypes.WipeTransition) },
            { SourceType.VstFilter, typeof(Models.TypeDefs.FilterTypes.VstFilter) },
            { SourceType.TextFt2Source, typeof(Models.TypeDefs.SourceTypes.TextFt2Source) },
            { SourceType.TextFt2SouceV2, typeof(Models.TypeDefs.SourceTypes.TextFt2SouceV2) },
            { SourceType.VlcSource, typeof(Models.TypeDefs.SourceTypes.VlcSource) },
            { SourceType.MonitorCapture, typeof(Models.TypeDefs.SourceTypes.MonitorCapture) },
            { SourceType.WindowCapture, typeof(Models.TypeDefs.SourceTypes.WindowCapture) },
            { SourceType.GameCapture, typeof(Models.TypeDefs.SourceTypes.GameCapture) },
            { SourceType.DShowInput, typeof(Models.TypeDefs.SourceTypes.DShowInput) },
            { SourceType.WasapiInputCapture, typeof(Models.TypeDefs.SourceTypes.WasapiInputCapture) },
            { SourceType.WasapiOutputCapture, typeof(Models.TypeDefs.SourceTypes.WasapiOutputCapture) },
        };

        public static Type GetType(SourceType sourceType)
        {
            return sourceTypeSettingsDictionary.TryGetValue(sourceType, out Type value) ? value : null;
        }

        public static Type GetType(string obsType)
        {
            return GetType(ObsTypes.ObsTypeNameDictionary[obsType]);
        }

        public static object GetInstanceOfType(SourceType sourceType)
        {
            return sourceTypeSettingsDictionary.TryGetValue(sourceType, out Type value) ? Activator.CreateInstance(value) : null;
        }
    }
}
