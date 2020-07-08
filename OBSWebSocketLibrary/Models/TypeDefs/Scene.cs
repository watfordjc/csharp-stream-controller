using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.TypeDefs
{
    public class Scene
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("sources")]
#pragma warning disable CA2227 // Collection properties should be read only
        public ObservableCollection<SceneItem> Sources { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
