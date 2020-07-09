using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.TypeDefs
{
    public class ObsScene
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("sources")]
#pragma warning disable CA2227 // Collection properties should be read only
        public ObservableCollection<ObsSceneItem> Sources { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
