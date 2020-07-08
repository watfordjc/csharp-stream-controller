﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace OBSWebSocketLibrary.Models.RequestReplies
{
    public class SetCurrentScene : RequestReplyBase
    {
    }

    public class GetCurrentScene : RequestReplyBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("sources")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TypeDefs.SceneItem> Sources { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class GetSceneList : RequestReplyBase
    {
        [JsonPropertyName("current-scene")]
        public string CurrentScene { get; set; }
        [JsonPropertyName("scenes")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TypeDefs.Scene> Scenes { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class ReorderSceneItems : RequestReplyBase
    {
    }

    public class SetSceneTransitionOverride : RequestReplyBase
    {
    }

    public class RemoveSceneTransitionOverride : RequestReplyBase
    {
    }

    public class GetSceneTransitionOverride : RequestReplyBase
    {
        [JsonPropertyName("transitionName")]
        public string TransitionName { get; set; }
        [JsonPropertyName("transitionDuration")]
        public int TransitionDuration { get; set; }
    }
}
