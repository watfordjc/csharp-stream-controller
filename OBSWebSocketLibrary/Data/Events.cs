using System;
using System.Collections.Generic;
using System.Text;

namespace OBSWebSocketLibrary.Data
{
    public enum Events
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
        private static readonly Dictionary<Events, Type> eventDictionary = new Dictionary<Events, Type>()
        {
            { Events.SwitchScenes, typeof(Models.Events.SwitchScenes) },
            { Events.ScenesChanged, typeof(Models.Events.ScenesChanged) },
            { Events.SceneCollectionChanged, typeof(Models.Events.SceneCollectionChanged) },
            { Events.SceneCollectionListChanged, typeof(Models.Events.SceneCollectionListChanged) },
            { Events.SwitchTransition, typeof(Models.Events.SwitchTransition) },
            { Events.TransitionListChanged, typeof(Models.Events.TransitionListChanged) },
            { Events.TransitionDurationChanged, typeof(Models.Events.TransitionDurationChanged) },
            { Events.TransitionBegin, typeof(Models.Events.TransitionBegin) },
            { Events.TransitionEnd, typeof(Models.Events.TransitionEnd) },
            { Events.TransitionVideoEnd, typeof(Models.Events.TransitionVideoEnd) },
            { Events.ProfileChanged, typeof(Models.Events.ProfileChanged) },
            { Events.ProfileListChanged, typeof(Models.Events.ProfileListChanged) },
            { Events.StreamStarting, typeof(Models.Events.StreamStarting) },
            { Events.StreamStarted, typeof(Models.Events.StreamStarted) },
            { Events.StreamStopping, typeof(Models.Events.StreamStopping) },
            { Events.StreamStopped, typeof(Models.Events.StreamStopped) },
            { Events.StreamStatus, typeof(Models.Events.StreamStatus) },
            { Events.RecordingStarting, typeof(Models.Events.RecordingStarting) },
            { Events.RecordingStarted, typeof(Models.Events.RecordingStarted) },
            { Events.RecordingStopping, typeof(Models.Events.RecordingStopping) },
            { Events.RecordingStopped, typeof(Models.Events.RecordingStopped) },
            { Events.RecordingPaused, typeof(Models.Events.RecordingPaused) },
            { Events.RecordingResumed, typeof(Models.Events.RecordingResumed) },
            { Events.ReplayStarting, typeof(Models.Events.ReplayStarting) },
            { Events.ReplayStarted, typeof(Models.Events.ReplayStarted) },
            { Events.ReplayStopping, typeof(Models.Events.ReplayStopping) },
            { Events.ReplayStopped, typeof(Models.Events.ReplayStopped) },
            { Events.Exiting, typeof(Models.Events.Exiting) },
            { Events.Heartbeat, typeof(Models.Events.Heartbeat) },
            { Events.BroadcastCustomMessage, typeof(Models.Events.BroadcastCustomMessage) },
            { Events.SourceCreated, typeof(Models.Events.SourceCreated) },
            { Events.SourceDestroyed, typeof(Models.Events.SourceDestroyed) },
            { Events.SourceVolumeChanged, typeof(Models.Events.SourceVolumeChanged) },
            { Events.SourceMuteStateChanged, typeof(Models.Events.SourceMuteStateChanged) },
            { Events.SourceAudioSyncOffsetChanged, typeof(Models.Events.SourceAudioSyncOffsetChanged) },
            { Events.SourceAudioMixersChanged, typeof(Models.Events.SourceAudioMixersChanged) },
            { Events.SourceRenamed, typeof(Models.Events.SourceRenamed) },
            { Events.SourceFilterAdded, typeof(Models.Events.SourceFilterAdded) },
            { Events.SourceFilterRemoved, typeof(Models.Events.SourceFilterRemoved) },
            { Events.SourceFilterVisibilityChanged, typeof(Models.Events.SourceFilterVisibilityChanged) },
            { Events.SourceFiltersReordered, typeof(Models.Events.SourceFiltersReordered) },
            { Events.SourceOrderChanged, typeof(Models.Events.SourceOrderChanged) },
            { Events.SceneItemAdded, typeof(Models.Events.SceneItemAdded) },
            { Events.SceneItemRemoved, typeof(Models.Events.SceneItemRemoved) },
            { Events.SceneItemVisibilityChanged, typeof(Models.Events.SceneItemVisibilityChanged) },
            { Events.SceneItemLockChanged, typeof(Models.Events.SceneItemLockChanged) },
            { Events.SceneItemTransformChanged, typeof(Models.Events.SceneItemTransformChanged) },
            { Events.SceneItemSelected, typeof(Models.Events.SceneItemSelected) },
            { Events.SceneItemDeselected, typeof(Models.Events.SceneItemDeselected) },
            { Events.PreviewSceneChanged, typeof(Models.Events.PreviewSceneChanged) },
            { Events.StudioModeSwitched, typeof(Models.Events.StudioModeSwitched) }
        };

        public static Type GetType(Events eventType)
        {
            return eventDictionary.TryGetValue(eventType, out Type value) ? value : null;
        }
    }
}
