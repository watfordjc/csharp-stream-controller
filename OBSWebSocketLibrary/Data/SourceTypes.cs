using System;
using System.Collections.Generic;
using System.Text;

namespace OBSWebSocketLibrary.Data
{
    public enum SourceTypes
    {
        scene = 1,
        group = 2,
        audio_line = 3,
        image_source = 4,
        color_source = 5,
        color_source_v2 = 6,
        slideshow = 7,
        browser_source = 8,
        ffmpeg_source = 9,
        mask_filter = 10,
        crop_filter = 11,
        gain_filter = 12,
        color_filter = 13,
        scale_filter = 14,
        scroll_filter = 15,
        gpu_delay = 16,
        color_key_filter = 17,
        clut_filter = 18,
        sharpness_filter = 19,
        chroma_key_filter = 20,
        async_delay_filter = 21,
        noise_suppress_filter = 22,
        invert_polarity_filter = 23,
        noise_gate_filter = 24,
        compressor_filter = 25,
        limiter_filter = 26,
        expander_filter = 27,
        luma_key_filter = 28,
        text_gdiplus = 29,
        text_gdiplus_v2 = 30,
        cut_transition = 31,
        fade_transition = 32,
        swipe_transition = 33,
        slide_transition = 34,
        obs_stinger_transition = 35,
        fade_to_color_transition = 36,
        wipe_transition = 37,
        vst_filter = 38,
        text_ft2_source = 39,
        text_ft2_source_v2 = 40,
        vlc_source = 41,
        monitor_capture = 42,
        window_capture = 43,
        game_capture = 44,
        dshow_input = 45,
        wasapi_input_capture = 46,
        wasapi_output_capture = 47
    }

    public static class SourceTypeSettings
    {
        private static readonly Dictionary<SourceTypes, Type> sourceTypeSettingsDictionary = new Dictionary<SourceTypes, Type>()
        {
            { SourceTypes.image_source, typeof(Models.TypeDefs.SourceTypes.ImageSource) },
            { SourceTypes.color_source_v2, typeof(Models.TypeDefs.SourceTypes.ColorSourceV2) },
            { SourceTypes.slideshow, typeof(Models.TypeDefs.SourceTypes.Slideshow) },
            { SourceTypes.browser_source, typeof(Models.TypeDefs.SourceTypes.BrowserSource) },
            { SourceTypes.ffmpeg_source, typeof(Models.TypeDefs.SourceTypes.FFmpegSource) },
            { SourceTypes.mask_filter, typeof(Models.TypeDefs.SourceTypes.MaskFilter) },
            { SourceTypes.color_filter, typeof(Models.TypeDefs.SourceTypes.ColorFilter) },
            { SourceTypes.text_gdiplus_v2, typeof(Models.TypeDefs.SourceTypes.TextGdiPlusV2) },
            { SourceTypes.window_capture, typeof(Models.TypeDefs.SourceTypes.WindowCapture) },
            { SourceTypes.game_capture, typeof(Models.TypeDefs.SourceTypes.GameCapture) },
            { SourceTypes.dshow_input, typeof(Models.TypeDefs.SourceTypes.DShowInput) },
            { SourceTypes.noise_suppress_filter, typeof(Models.TypeDefs.SourceTypes.NoiseSupressFilter) },
            { SourceTypes.noise_gate_filter, typeof(Models.TypeDefs.SourceTypes.NoiseGateFilter) },
            { SourceTypes.compressor_filter, typeof(Models.TypeDefs.SourceTypes.CompressorFilter) },
            { SourceTypes.wasapi_input_capture, typeof(Models.TypeDefs.SourceTypes.WasapiInputCapture) },
            { SourceTypes.wasapi_output_capture, typeof(Models.TypeDefs.SourceTypes.WasapiOutputCapture) },
        };

        public static Type GetType(SourceTypes sourceType)
        {
            return sourceTypeSettingsDictionary.TryGetValue(sourceType, out Type value) ? value : null;
        }
    }
}
