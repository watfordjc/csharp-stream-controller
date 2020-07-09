using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.TypeDefs
{
    public class ObsWsCoordinates
    {
        [JsonPropertyName("x")]
        public Decimal X { get; set; }
        [JsonPropertyName("y")]
        public Decimal Y { get; set; }
    }

    public class ObsWsPositionProperties : ObsWsCoordinates
    {
        [JsonPropertyName("alignment")]
        public int Alignment { get; set; }
    }

    public class ObsWsBoundsProperties : ObsWsCoordinates
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("alignment")]
        public int Alignment { get; set; }
    }

    public class ObsWsDirections
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

    public class ObsWsFont
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

    public class ObsWsItemObject
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }

    public class ObsWsProfile
    {
        [JsonPropertyName("profile-name")]
        public string Name { get; set; }
    }

    public class ObsWsStream
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("metadata")]
        public object Metadata { get; set; }
        [JsonPropertyName("settings")]
        public ObsWsStreamSettings Settings { get; set; }
    }

    public class ObsWsStreamSettings
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

    public class ObsWsMixer
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
    }

    public class ObsRequestMetadata
    {
        public Guid RequestGuid { get; set; }
        public Data.ObsRequestType OriginalRequestType { get; set; }
        public object OriginalRequestData { get; set; }
    }

    public class ObsReplyObject
    {
        public Guid MessageId { get; set; }
        public Data.ObsRequestType RequestType { get; set; }
        public Data.ObsSourceType SourceType { get; set; }
        public object MessageObject { get; set; }
        public string Status { get; set; }
        public ObsRequestMetadata RequestMetadata { get; set; }
    }

    public class ObsEventObject
    {
        public Data.ObsEventType EventType { get; set; }
        public Data.ObsSourceType SourceType { get; set; }
        public object MessageObject { get; set; }
    }

    public class ObsWsFilter
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class ObsWsReplyFilter
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

    public class ObsWsReplyType
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
        public ObsWsTypeCapabilities Caps { get; set; }
    }

    public class ObsWsTypeCapabilities
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

    public class ObsWsOutputFlags
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

    public class ObsWsOutputSettings
    {
        [JsonPropertyName("active")]
        public bool Active { get; set; }
        [JsonPropertyName("reconnecting")]
        public bool Reconnecting { get; set; }
        [JsonPropertyName("congestion")]
        public double Congestion { get; set; }
    }

    public class ObsWsReplySource
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("typeId")]
        public string TypeId { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class ObsWsSceneItem
    {
        [JsonPropertyName("align")]
        public int Align { get; set; }
        [JsonPropertyName("bounds")]
        public TypeDefs.ObsWsCoordinates Bounds { get; set; }
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
        public TypeDefs.ObsWsCoordinates Pos { get; set; }
        [JsonPropertyName("private_settings")]
        public JsonElement PrivateSettings { get; set; }
        [JsonPropertyName("rot")]
        public Decimal Rot { get; set; }
        [JsonPropertyName("scale")]
        public TypeDefs.ObsWsCoordinates Scale { get; set; }
        [JsonPropertyName("scale_filter")]
        public string ScaleFilter { get; set; }
        [JsonPropertyName("visible")]
        public bool Visible { get; set; }
    }

    public class ObsWsEventSceneItem
    {
        [JsonPropertyName("source-name")]
        public string SourceName { get; set; }
        [JsonPropertyName("item-id")]
        public int ItemId { get; set; }
    }

    public class ObsWsTransitionName
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class ObsWsRequestTransition
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("duration")]
        public int Duration { get; set; }
    }

    public class ObsWsFile
    {
        [JsonPropertyName("hidden")]
        public bool Hidden { get; set; }
        [JsonPropertyName("selected")]
        public bool Selected { get; set; }
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class ObsWsVlcPlaylistItem
    {
        [JsonPropertyName("hidden")]
        public bool Hidden { get; set; }
        [JsonPropertyName("selected")]
        public bool Selected { get; set; }
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}
