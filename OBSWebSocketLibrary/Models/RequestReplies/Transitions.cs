﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.RequestReplies
{
    public class GetTransitionList : RequestReplyBase
    {
        [JsonPropertyName("current-transition")]
        public string CurrentTransition { get; set; }
        [JsonPropertyName("transitions")]
        public Transition[] Transitions { get; set; }
        public class Transition
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }
    }

    public class GetCurrentTransition : RequestReplyBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("duration")]
        public int Duration { get; set; }
    }

    public class SetCurrentTransition : RequestReplyBase
    {
    }

    public class SetTransitionDuration : RequestReplyBase
    {
    }

    public class GetTransitionDuration : RequestReplyBase
    {
        [JsonPropertyName("transition-duration")]
        public int Duration { get; set; }
    }

    public class GetTransitionPosition : RequestReplyBase
    {
        [JsonPropertyName("position")]
        public double Position { get; set; }
    }
}