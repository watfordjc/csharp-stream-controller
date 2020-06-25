using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.TypeDefs
{
    public class Font
    {
        [JsonPropertyName("face")]
        public string FontFace { get; set; }
        [JsonPropertyName("flags")]
        public int Flags { get; set; }
        [JsonPropertyName("size")]
        public int FontSize { get; set; }
        [JsonPropertyName("style")]
        public string FontStyle { get; set; }
    }
}
