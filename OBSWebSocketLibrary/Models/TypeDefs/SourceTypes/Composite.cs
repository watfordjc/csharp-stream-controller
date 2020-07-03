using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.TypeDefs.SourceTypes
{
    public class Scene : BaseType
    {
        [JsonPropertyName("custom_size")]
        public bool CustomSize { get; set; }
        [JsonPropertyName("id_counter")]
        public int IdCounter { get; set; }
        [JsonPropertyName("items")]
        public Item[] Items { get; set; }
        public class Item
        {
            [JsonPropertyName("align")]
            public int Align { get; set; }
            [JsonPropertyName("bounds")]
            public TypeDefs.Coordinates Bounds { get; set; }
            [JsonPropertyName("bounds_align")]
            public int BoundsAlign { get; set; }
            [JsonPropertyName("bounds_type")]
            public int BoundsType { get; set; }
            [JsonPropertyName("crop_bottom")]
            public int CropBottom { get; set; }
            [JsonPropertyName("crop_left")]
            public int CropLeft { get; set; }
            [JsonPropertyName("crop_right")]
            public int CropRight { get; set; }
            [JsonPropertyName("crop_top")]
            public int CropTop { get; set; }
            [JsonPropertyName("group_item_backup")]
            public bool GroupItemBackup { get; set; }
            [JsonPropertyName("id")]
            public int Id { get; set; }
            [JsonPropertyName("locked")]
            public bool Locked { get; set; }
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("pos")]
            public TypeDefs.Coordinates Pos { get; set; }
            [JsonPropertyName("private_settings")]
            public JsonElement PrivateSettings { get; set; }
            [JsonPropertyName("rot")]
            public Decimal Rot { get; set; }
            [JsonPropertyName("scale")]
            public TypeDefs.Coordinates Scale { get; set; }
            [JsonPropertyName("scale_filter")]
            public string ScaleFilter { get; set; }
            [JsonPropertyName("visible")]
            public bool Visible { get; set; }
        }
    }

    public class Group : BaseType
    {
        [JsonPropertyName("custom_size")]
        public bool CustomSize { get; set; }
        [JsonPropertyName("cx")]
        public int Cx { get; set; }
        [JsonPropertyName("cy")]
        public int Cy { get; set; }
        [JsonPropertyName("id_counter")]
        public int IdCounter { get; set; }
        [JsonPropertyName("items")]
        public Scene.Item[] Items { get; set; }
    }
}
