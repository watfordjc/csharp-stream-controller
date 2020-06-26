using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.Requests
{
    public class GetStreamingStatus : RequestBase
    {
    }

    public class StartStopStreaming : RequestBase
    {
    }

    public class StartStreaming : RequestBase
    {
        [JsonPropertyName("stream")]
        public Stream StreamItem { get; set; }
        public class Stream
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }
            [JsonPropertyName("metadata")]
            public JsonElement Metadata { get; set; }
            [JsonPropertyName("settings")]
            public StreamSettings Settings { get; set; }
            public class StreamSettings
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
        }
    }

    public class StopStreaming : RequestBase
    {
    }

    public class SetStreamSettings : RequestBase
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("settings")]
        public StreamSettings Settings { get; set; }
        public class StreamSettings
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
        [JsonPropertyName("save")]
        public bool Save { get; set; }
    }

    public class GetStreamSettings : RequestBase
    {
    }

    public class SaveStreamSettings : RequestBase
    {
    }

    public class SendCaptions : RequestBase
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
