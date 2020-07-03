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
        public BaseType()
        {
            Dependencies = new DependencyProperties();
        }

        public DependencyProperties Dependencies { get; set; }
        public class DependencyProperties
        {
            public bool DependencyProblem { get; set; }
            public string AudioDeviceId { get; set; }
            public bool HasAudioInterface
            {
                get
                {
                    return AudioDeviceId != null && AudioDeviceId != String.Empty;
                }
            }
            public string VideoDeviceId { get; set; }
            public bool HasVideoInterface
            {
                get
                {
                    return VideoDeviceId != null && VideoDeviceId != String.Empty;
                }
            }
            public string[] FilePaths { get; set; }
            public bool HasFiles
            {
                get
                {
                    return FilePaths != null && FilePaths.Length != 0;
                }
            }
            public string[] Uris { get; set; }
            public bool HasURIs
            {
                get
                {
                    return Uris != null && Uris.Length != 0;
                }
            }
        }
    }
}