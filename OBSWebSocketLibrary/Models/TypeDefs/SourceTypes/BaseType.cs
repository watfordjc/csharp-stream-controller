using System;
using System.Collections.Generic;
using System.Text;
using Stream_Controller.SharedModels;

namespace OBSWebSocketLibrary.Models.TypeDefs.SourceTypes
{
    /*
     * OBSWebSocketLibrary.Models.RequestReplies.GetSourceTypesList.Type
    [JsonPropertyName("typeId")]
    public string TypeId { get; set; }
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }
    [JsonPropertyName("type")]
    public string TypeType { get; set; }
    [JsonPropertyName("defaultSettings")]
    public JsonElement DefaultSettings { get; set; }
    public object DefaultSettingsObj;
    [JsonPropertyName("caps")]
    public CapsProperties Caps { get; set; }
    public class CapsProperties
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
    */


public class BaseType : OBSWebSocketLibrary.Models.RequestReplies.GetSourceTypesList.Type
    {
        public class Dependencies
        {
            public bool HasAudioInterface { get; set; }
            public string AudioDeviceId { get; set; }
            public bool HasVideoInterface { get; set; }
            public string VideoDeviceId { get; set; }
            public bool HasFiles { get; set; }
            public string[] FilePaths { get; set; }
            public bool HasURIs { get; set; }
            public string[] Uris { get; set; }
        }
    }
}