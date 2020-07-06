using System;
using System.Collections.Generic;
using System.Text;

namespace OBSWebSocketLibrary.Data
{
    public static class ObsTypes
    {
        public static readonly Dictionary<string, SourceTypes2> ObsTypeNameDictionary = new Dictionary<string, SourceTypes2>()
        {
            { "scene", SourceTypes2.Scene },
            { "group", SourceTypes2.Group },
            { "audio_line", SourceTypes2.AudioLine },
            { "image_source", SourceTypes2.ImageSource },
            { "color_source", SourceTypes2.ColorSource },
            { "color_source_v2", SourceTypes2.ColorSourceV2 },
            { "slideshow", SourceTypes2.Slideshow },
            { "browser_source", SourceTypes2.BrowserSource },
            { "ffmpeg_source", SourceTypes2.FFmpegSource },
            { "mask_filter", SourceTypes2.MaskFilter },
            { "crop_filter", SourceTypes2.CropFilter },
            { "gain_filter", SourceTypes2.GainFilter },
            { "color_filter", SourceTypes2.ColorFilter },
            { "scale_filter", SourceTypes2.ScaleFilter },
            { "scroll_filter", SourceTypes2.ScrollFilter },
            { "gpu_delay", SourceTypes2.GpuDelay },
            { "color_key_filter", SourceTypes2.ColorKeyFilter },
            { "clut_filter", SourceTypes2.ClutFilter },
            { "sharpness_filter", SourceTypes2.SharpnessFilter },
            { "chroma_key_filter", SourceTypes2.ChromaKeyFilter },
            { "async_delay_filter", SourceTypes2.AsyncDelayFilter },
            { "noise_suppress_filter", SourceTypes2.NoiseSupressFilter },
            { "invert_polarity_filter", SourceTypes2.InvertPolarityFilter },
            { "noise_gate_filter", SourceTypes2.NoiseGateFilter },
            { "compressor_filter", SourceTypes2.CompressorFilter },
            { "limiter_filter", SourceTypes2.LimiterFilter },
            { "expander_filter", SourceTypes2.ExpanderFilter },
            { "luma_key_filter", SourceTypes2.LumaKeyFilter },
            { "text_gdiplus", SourceTypes2.TextGdiPlus },
            { "text_gdiplus_v2", SourceTypes2.TextGdiPlusV2 },
            { "cut_transition", SourceTypes2.CutTransition },
            { "fade_transition", SourceTypes2.FadeTransition },
            { "swipe_transition", SourceTypes2.SwipeTransition },
            { "slide_transition", SourceTypes2.SlideTransition },
            { "obs_stinger_transition", SourceTypes2.ObsStingerTransition },
            { "fade_to_color_transition", SourceTypes2.FadeToColorTransition },
            { "wipe_transition", SourceTypes2.WipeTransition },
            { "vst_filter", SourceTypes2.VstFilter },
            { "text_ft2_source", SourceTypes2.TextFt2Source },
            { "text_ft2_source_v2", SourceTypes2.TextFt2SouceV2 },
            { "vlc_source", SourceTypes2.VlcSource },
            { "monitor_capture", SourceTypes2.MonitorCapture },
            { "window_capture", SourceTypes2.WindowCapture },
            { "game_capture", SourceTypes2.GameCapture },
            { "dshow_input", SourceTypes2.DShowInput },
            { "wasapi_input_capture", SourceTypes2.WasapiInputCapture },
            { "wasapi_output_capture", SourceTypes2.WasapiOutputCapture }
        };
    }

    public enum SourceTypes2
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
        private static readonly Dictionary<SourceTypes2, Type> sourceTypeSettingsDictionary = new Dictionary<SourceTypes2, Type>()
        {
            { SourceTypes2.Scene, typeof(Models.TypeDefs.SourceTypes.Scene) },
            { SourceTypes2.Group, typeof(Models.TypeDefs.SourceTypes.Group) },
            { SourceTypes2.AudioLine, typeof(Models.TypeDefs.SourceTypes.AudioLine) },
            { SourceTypes2.ImageSource, typeof(Models.TypeDefs.SourceTypes.ImageSource) },
            { SourceTypes2.ColorSource, typeof(Models.TypeDefs.SourceTypes.ColorSource) },
            { SourceTypes2.ColorSourceV2, typeof(Models.TypeDefs.SourceTypes.ColorSourceV2) },
            { SourceTypes2.Slideshow, typeof(Models.TypeDefs.SourceTypes.Slideshow) },
            { SourceTypes2.BrowserSource, typeof(Models.TypeDefs.SourceTypes.BrowserSource) },
            { SourceTypes2.FFmpegSource, typeof(Models.TypeDefs.SourceTypes.FFmpegSource) },
            { SourceTypes2.MaskFilter, typeof(Models.TypeDefs.FilterTypes.MaskFilter) },
            { SourceTypes2.CropFilter, typeof(Models.TypeDefs.FilterTypes.CropFilter) },
            { SourceTypes2.GainFilter, typeof(Models.TypeDefs.FilterTypes.GainFilter) },
            { SourceTypes2.ColorFilter, typeof(Models.TypeDefs.FilterTypes.ColorFilter) },
            { SourceTypes2.ScaleFilter, typeof(Models.TypeDefs.FilterTypes.ScaleFilter) },
            { SourceTypes2.ScrollFilter, typeof(Models.TypeDefs.FilterTypes.ScrollFilter) },
            { SourceTypes2.GpuDelay, typeof(Models.TypeDefs.FilterTypes.GpuDelay) },
            { SourceTypes2.ColorKeyFilter, typeof(Models.TypeDefs.FilterTypes.ColorKeyFilter) },
            { SourceTypes2.ClutFilter, typeof(Models.TypeDefs.FilterTypes.ClutFilter) },
            { SourceTypes2.SharpnessFilter, typeof(Models.TypeDefs.FilterTypes.SharpnessFilter) },
            { SourceTypes2.ChromaKeyFilter, typeof(Models.TypeDefs.FilterTypes.ChromaKeyFilter) },
            { SourceTypes2.AsyncDelayFilter, typeof(Models.TypeDefs.FilterTypes.AsyncDelayFilter) },
            { SourceTypes2.NoiseSupressFilter, typeof(Models.TypeDefs.FilterTypes.NoiseSupressFilter) },
            { SourceTypes2.InvertPolarityFilter, typeof(Models.TypeDefs.FilterTypes.InvertPolarityFilter) },
            { SourceTypes2.NoiseGateFilter, typeof(Models.TypeDefs.FilterTypes.NoiseGateFilter) },
            { SourceTypes2.CompressorFilter, typeof(Models.TypeDefs.FilterTypes.CompressorFilter) },
            { SourceTypes2.LimiterFilter, typeof(Models.TypeDefs.FilterTypes.LimiterFilter) },
            { SourceTypes2.ExpanderFilter, typeof(Models.TypeDefs.FilterTypes.ExpanderFilter) },
            { SourceTypes2.LumaKeyFilter, typeof(Models.TypeDefs.FilterTypes.LumaKeyFilter) },
            { SourceTypes2.TextGdiPlus, typeof(Models.TypeDefs.SourceTypes.TextGdiPlus) },
            { SourceTypes2.TextGdiPlusV2, typeof(Models.TypeDefs.SourceTypes.TextGdiPlusV2) },
            { SourceTypes2.CutTransition, typeof(Models.TypeDefs.SourceTypes.CutTransition) },
            { SourceTypes2.FadeTransition, typeof(Models.TypeDefs.SourceTypes.FadeTransition) },
            { SourceTypes2.SwipeTransition, typeof(Models.TypeDefs.SourceTypes.SwipeTransition) },
            { SourceTypes2.SlideTransition, typeof(Models.TypeDefs.SourceTypes.SlideTransition) },
            { SourceTypes2.ObsStingerTransition, typeof(Models.TypeDefs.SourceTypes.ObsStingerTransition) },
            { SourceTypes2.FadeToColorTransition, typeof(Models.TypeDefs.SourceTypes.FadeToColorTransition) },
            { SourceTypes2.WipeTransition, typeof(Models.TypeDefs.SourceTypes.WipeTransition) },
            { SourceTypes2.VstFilter, typeof(Models.TypeDefs.FilterTypes.VstFilter) },
            { SourceTypes2.TextFt2Source, typeof(Models.TypeDefs.SourceTypes.TextFt2Source) },
            { SourceTypes2.TextFt2SouceV2, typeof(Models.TypeDefs.SourceTypes.TextFt2SouceV2) },
            { SourceTypes2.VlcSource, typeof(Models.TypeDefs.SourceTypes.VlcSource) },
            { SourceTypes2.MonitorCapture, typeof(Models.TypeDefs.SourceTypes.MonitorCapture) },
            { SourceTypes2.WindowCapture, typeof(Models.TypeDefs.SourceTypes.WindowCapture) },
            { SourceTypes2.GameCapture, typeof(Models.TypeDefs.SourceTypes.GameCapture) },
            { SourceTypes2.DShowInput, typeof(Models.TypeDefs.SourceTypes.DShowInput) },
            { SourceTypes2.WasapiInputCapture, typeof(Models.TypeDefs.SourceTypes.WasapiInputCapture) },
            { SourceTypes2.WasapiOutputCapture, typeof(Models.TypeDefs.SourceTypes.WasapiOutputCapture) },
        };

        public static Type GetType(SourceTypes2 sourceType)
        {
            return sourceTypeSettingsDictionary.TryGetValue(sourceType, out Type value) ? value : null;
        }

        public static Type GetType(string obsType)
        {
            return GetType(ObsTypes.ObsTypeNameDictionary[obsType]);
        }

        public static object GetInstanceOfType(SourceTypes2 sourceType)
        {
            return sourceTypeSettingsDictionary.TryGetValue(sourceType, out Type value) ? Activator.CreateInstance(value) : null;
        }
    }
}
