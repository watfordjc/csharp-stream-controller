using System;
using System.Collections.Generic;
using System.Text;

namespace OBSWebSocketLibrary.Data
{
    public enum EventType
    {
        SwitchScenes = 1,
        ScenesChanged = 2,
        SceneCollectionChanged = 3,
        SceneCollectionListChanged = 4,
        SwitchTransition = 5,
        TransitionListChanged = 6,
        TransitionDurationChanged = 7,
        TransitionBegin = 8,
        TransitionEnd = 9,
        TransitionVideoEnd = 10,
        ProfileChanged = 11,
        ProfileListChanged = 12,
        StreamStarting = 13,
        StreamStarted = 14,
        StreamStopping = 15,
        StreamStopped = 16,
        StreamStatus = 17,
        RecordingStarting = 18,
        RecordingStarted = 19,
        RecordingStopping = 20,
        RecordingStopped = 21,
        RecordingPaused = 22,
        RecordingResumed = 23,
        ReplayStarting = 24,
        ReplayStarted = 25,
        ReplayStopping = 26,
        ReplayStopped = 27,
        Exiting = 28,
        Heartbeat = 29,
        BroadcastCustomMessage = 30,
        SourceCreated = 31,
        SourceDestroyed = 32,
        SourceVolumeChanged = 33,
        SourceMuteStateChanged = 34,
        SourceAudioSyncOffsetChanged = 35,
        SourceAudioMixersChanged = 36,
        SourceRenamed = 37,
        SourceFilterAdded = 38,
        SourceFilterRemoved = 39,
        SourceFilterVisibilityChanged = 40,
        SourceFiltersReordered = 41,
        SourceOrderChanged = 42,
        SceneItemAdded = 43,
        SceneItemRemoved = 44,
        SceneItemVisibilityChanged = 45,
        SceneItemLockChanged = 46,
        SceneItemTransformChanged = 47,
        SceneItemSelected = 48,
        SceneItemDeselected = 49,
        PreviewSceneChanged = 50,
        StudioModeSwitched = 51
    }

    public static class ObsEvent
    {
        private static readonly Dictionary<EventType, Type> eventDictionary = new Dictionary<EventType, Type>()
        {
            { EventType.SwitchScenes, typeof(Models.Events.SwitchScenes) },
            { EventType.ScenesChanged, typeof(Models.Events.ScenesChanged) },
            { EventType.SceneCollectionChanged, typeof(Models.Events.SceneCollectionChanged) },
            { EventType.SceneCollectionListChanged, typeof(Models.Events.SceneCollectionListChanged) },
            { EventType.SwitchTransition, typeof(Models.Events.SwitchTransition) },
            { EventType.TransitionListChanged, typeof(Models.Events.TransitionListChanged) },
            { EventType.TransitionDurationChanged, typeof(Models.Events.TransitionDurationChanged) },
            { EventType.TransitionBegin, typeof(Models.Events.TransitionBegin) },
            { EventType.TransitionEnd, typeof(Models.Events.TransitionEnd) },
            { EventType.TransitionVideoEnd, typeof(Models.Events.TransitionVideoEnd) },
            { EventType.ProfileChanged, typeof(Models.Events.ProfileChanged) },
            { EventType.ProfileListChanged, typeof(Models.Events.ProfileListChanged) },
            { EventType.StreamStarting, typeof(Models.Events.StreamStarting) },
            { EventType.StreamStarted, typeof(Models.Events.StreamStarted) },
            { EventType.StreamStopping, typeof(Models.Events.StreamStopping) },
            { EventType.StreamStopped, typeof(Models.Events.StreamStopped) },
            { EventType.StreamStatus, typeof(Models.Events.StreamStatus) },
            { EventType.RecordingStarting, typeof(Models.Events.RecordingStarting) },
            { EventType.RecordingStarted, typeof(Models.Events.RecordingStarted) },
            { EventType.RecordingStopping, typeof(Models.Events.RecordingStopping) },
            { EventType.RecordingStopped, typeof(Models.Events.RecordingStopped) },
            { EventType.RecordingPaused, typeof(Models.Events.RecordingPaused) },
            { EventType.RecordingResumed, typeof(Models.Events.RecordingResumed) },
            { EventType.ReplayStarting, typeof(Models.Events.ReplayStarting) },
            { EventType.ReplayStarted, typeof(Models.Events.ReplayStarted) },
            { EventType.ReplayStopping, typeof(Models.Events.ReplayStopping) },
            { EventType.ReplayStopped, typeof(Models.Events.ReplayStopped) },
            { EventType.Exiting, typeof(Models.Events.Exiting) },
            { EventType.Heartbeat, typeof(Models.Events.Heartbeat) },
            { EventType.BroadcastCustomMessage, typeof(Models.Events.BroadcastCustomMessage) },
            { EventType.SourceCreated, typeof(Models.Events.SourceCreated) },
            { EventType.SourceDestroyed, typeof(Models.Events.SourceDestroyed) },
            { EventType.SourceVolumeChanged, typeof(Models.Events.SourceVolumeChanged) },
            { EventType.SourceMuteStateChanged, typeof(Models.Events.SourceMuteStateChanged) },
            { EventType.SourceAudioSyncOffsetChanged, typeof(Models.Events.SourceAudioSyncOffsetChanged) },
            { EventType.SourceAudioMixersChanged, typeof(Models.Events.SourceAudioMixersChanged) },
            { EventType.SourceRenamed, typeof(Models.Events.SourceRenamed) },
            { EventType.SourceFilterAdded, typeof(Models.Events.SourceFilterAdded) },
            { EventType.SourceFilterRemoved, typeof(Models.Events.SourceFilterRemoved) },
            { EventType.SourceFilterVisibilityChanged, typeof(Models.Events.SourceFilterVisibilityChanged) },
            { EventType.SourceFiltersReordered, typeof(Models.Events.SourceFiltersReordered) },
            { EventType.SourceOrderChanged, typeof(Models.Events.SourceOrderChanged) },
            { EventType.SceneItemAdded, typeof(Models.Events.SceneItemAdded) },
            { EventType.SceneItemRemoved, typeof(Models.Events.SceneItemRemoved) },
            { EventType.SceneItemVisibilityChanged, typeof(Models.Events.SceneItemVisibilityChanged) },
            { EventType.SceneItemLockChanged, typeof(Models.Events.SceneItemLockChanged) },
            { EventType.SceneItemTransformChanged, typeof(Models.Events.SceneItemTransformChanged) },
            { EventType.SceneItemSelected, typeof(Models.Events.SceneItemSelected) },
            { EventType.SceneItemDeselected, typeof(Models.Events.SceneItemDeselected) },
            { EventType.PreviewSceneChanged, typeof(Models.Events.PreviewSceneChanged) },
            { EventType.StudioModeSwitched, typeof(Models.Events.StudioModeSwitched) }
        };

        public static Type GetType(EventType eventType)
        {
            return eventDictionary.TryGetValue(eventType, out Type value) ? value : null;
        }
    }
}
