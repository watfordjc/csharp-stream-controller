using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
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

    public class PositionProperties : Coordinates
    {
        [JsonPropertyName("alignment")]
        public int Alignment { get; set; }
    }

    public class BoundsProperties : Coordinates
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("alignment")]
        public int Alignment { get; set; }
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

    public class ObsProfile
    {
        [JsonPropertyName("profile-name")]
        public string Name { get; set; }
    }

    public class ObsStream
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("metadata")]
        public object Metadata { get; set; }
        [JsonPropertyName("settings")]
        public ObsStreamSettings Settings { get; set; }
    }

    public class ObsStreamSettings
    {
        [JsonPropertyName("server")]
        public string Server { get; set; }
        [JsonPropertyName("key")]
        public string Key { get; set; }
        [JsonPropertyName("use_auth")]
        public bool UseAuth { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }

    public class ObsMixer
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
    }

    public class ObsRequestMetadata
    {
        public Guid RequestGuid { get; set; }
        public Data.RequestType OriginalRequestType { get; set; }
        public object OriginalRequestData { get; set; }
    }

    public class ObsReply
    {
        public Guid MessageId { get; set; }
        public Data.RequestType RequestType { get; set; }
        public Data.SourceType SourceType { get; set; }
        public object MessageObject { get; set; }
        public string Status { get; set; }
        public ObsRequestMetadata RequestMetadata { get; set; }
    }

    public class ObsEvent
    {
        public Data.EventType EventType { get; set; }
        public Data.SourceType SourceType { get; set; }
        public object MessageObject { get; set; }
    }

    public class ObsFilter
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class ObsReplyFilter
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("settings")]
        public JsonElement Settings { get; set; }
        public object SettingsObj { get; set; }
    }

    public class ObsReplyType
    {
        [JsonPropertyName("typeId")]
        public string TypeId { get; set; }
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }
        [JsonPropertyName("type")]
        public string TypeType { get; set; }
        [JsonPropertyName("defaultSettings")]
        public JsonElement DefaultSettings { get; set; }
        [JsonIgnore]
        public object DefaultSettingsObj { get; set; }
        [JsonPropertyName("caps")]
        public ObsTypeCapabilities Caps { get; set; }
    }

    public class ObsTypeCapabilities
    {
        [JsonPropertyName("isAsync")]
        public bool IsAsync { get; set; }
        [JsonPropertyName("hasVideo")]
        public bool HasVideo { get; set; }
        [JsonPropertyName("hasAudio")]
        public bool HasAudio { get; set; }
        [JsonPropertyName("canInteract")]
        public bool CanInteract { get; set; }
        [JsonPropertyName("isComposite")]
        public bool IsComposite { get; set; }
        [JsonPropertyName("doNotDuplicate")]
        public bool DoNotDuplicate { get; set; }
        [JsonPropertyName("doNotSelfMonitor")]
        public bool DoNotSelfMonitor { get; set; }
    }

    public class ObsOutputFlags
    {
        [JsonPropertyName("rawValue")]
        public int RawValue { get; set; }
        [JsonPropertyName("audio")]
        public bool Audio { get; set; }
        [JsonPropertyName("video")]
        public bool Video { get; set; }
        [JsonPropertyName("encoded")]
        public bool Encoded { get; set; }
        [JsonPropertyName("multiTrack")]
        public bool MultiTrack { get; set; }
        [JsonPropertyName("service")]
        public bool Service { get; set; }
    }

    public class ObsOutputSettings
    {
        [JsonPropertyName("active")]
        public bool Active { get; set; }
        [JsonPropertyName("reconnecting")]
        public bool Reconnecting { get; set; }
        [JsonPropertyName("congestion")]
        public double Congestion { get; set; }
    }

    public class ObsReplySource
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("typeId")]
        public string TypeId { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class ObsSceneItem
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

    public class ObsEventSceneItem
    {
        [JsonPropertyName("source-name")]
        public string SourceName { get; set; }
        [JsonPropertyName("item-id")]
        public int ItemId { get; set; }
    }

    public class ObsTransitionName
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class ObsRequestTransition
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("duration")]
        public int Duration { get; set; }
    }

    public class ObsFile
    {
        [JsonPropertyName("hidden")]
        public bool Hidden { get; set; }
        [JsonPropertyName("selected")]
        public bool Selected { get; set; }
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class VlcPlaylistItem
    {
        [JsonPropertyName("hidden")]
        public bool Hidden { get; set; }
        [JsonPropertyName("selected")]
        public bool Selected { get; set; }
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}
