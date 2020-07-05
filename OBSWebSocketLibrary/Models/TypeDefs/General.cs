using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.TypeDefs
{
    public class Coordinates
    {
        [JsonPropertyName("x")]
        public Decimal X { get; set; }
        [JsonPropertyName("y")]
        public Decimal Y { get; set; }
    }

    public class Directions
    {
        [JsonPropertyName("top")]
        public int Top { get; set; }
        [JsonPropertyName("right")]
        public int Right { get; set; }
        [JsonPropertyName("bottom")]
        public int Bottom { get; set; }
        [JsonPropertyName("left")]
        public int Left { get; set; }
    }

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

    public class ItemObject
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }

    public class Profile
    {
        [JsonPropertyName("profile-name")]
        public string Name { get; set; }
    }

    public class Mixer
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
    }
}
