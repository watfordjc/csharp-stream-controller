using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace uk.JohnCook.dotnet.StreamController.Models
{
    public class WeatherData
    {
        [JsonPropertyName("weatherData")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<WeatherDataItem> Items { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class WeatherDataItem
    {
        [JsonPropertyName("location")]
        public string Location { get; set; }
        [JsonPropertyName("icon")]
        public string Symbol { get; set; }
        [JsonPropertyName("temp")]
        public string Temperature { get; set; }
        [JsonPropertyName("tz")]
        public string Timezone { get; set; }
    }
}
