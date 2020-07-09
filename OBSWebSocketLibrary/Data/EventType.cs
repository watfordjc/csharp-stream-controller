using System;
using System.Collections.Generic;
using System.Text;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.Data
{
    public enum ObsEventType
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

    public static class ObsWsEvent
    {
        private static readonly Dictionary<ObsEventType, Type> eventDictionary = new Dictionary<ObsEventType, Type>()
        {
            { ObsEventType.SwitchScenes, typeof(ObsEvents.SwitchScenesObsEvent) },
            { ObsEventType.ScenesChanged, typeof(ObsEvents.ScenesChangedObsEvent) },
            { ObsEventType.SceneCollectionChanged, typeof(ObsEvents.SceneCollectionChangedObsEvent) },
            { ObsEventType.SceneCollectionListChanged, typeof(ObsEvents.SceneCollectionListChangedObsEvent) },
            { ObsEventType.SwitchTransition, typeof(ObsEvents.SwitchTransitionObsEvent) },
            { ObsEventType.TransitionListChanged, typeof(ObsEvents.TransitionListChangedObsEvent) },
            { ObsEventType.TransitionDurationChanged, typeof(ObsEvents.TransitionDurationChangedObsEvent) },
            { ObsEventType.TransitionBegin, typeof(ObsEvents.TransitionBeginObsEvent) },
            { ObsEventType.TransitionEnd, typeof(ObsEvents.TransitionEndObsEvent) },
            { ObsEventType.TransitionVideoEnd, typeof(ObsEvents.TransitionVideoEndObsEvent) },
            { ObsEventType.ProfileChanged, typeof(ObsEvents.ProfileChangedObsEvent) },
            { ObsEventType.ProfileListChanged, typeof(ObsEvents.ProfileListChangedObsEvent) },
            { ObsEventType.StreamStarting, typeof(ObsEvents.StreamStartingObsEvent) },
            { ObsEventType.StreamStarted, typeof(ObsEvents.StreamStartedObsEvent) },
            { ObsEventType.StreamStopping, typeof(ObsEvents.StreamStoppingObsEvent) },
            { ObsEventType.StreamStopped, typeof(ObsEvents.StreamStoppedObsEvent) },
            { ObsEventType.StreamStatus, typeof(ObsEvents.StreamStatusObsEvent) },
            { ObsEventType.RecordingStarting, typeof(ObsEvents.RecordingStartingObsEvent) },
            { ObsEventType.RecordingStarted, typeof(ObsEvents.RecordingStartedObsEvent) },
            { ObsEventType.RecordingStopping, typeof(ObsEvents.RecordingStoppingObsEvent) },
            { ObsEventType.RecordingStopped, typeof(ObsEvents.RecordingStoppedObsEvent) },
            { ObsEventType.RecordingPaused, typeof(ObsEvents.RecordingPausedObsEvent) },
            { ObsEventType.RecordingResumed, typeof(ObsEvents.RecordingResumedObsEvent) },
            { ObsEventType.ReplayStarting, typeof(ObsEvents.ReplayStartingObsEvent) },
            { ObsEventType.ReplayStarted, typeof(ObsEvents.ReplayStartedObsEvent) },
            { ObsEventType.ReplayStopping, typeof(ObsEvents.ReplayStoppingObsEvent) },
            { ObsEventType.ReplayStopped, typeof(ObsEvents.ReplayStoppedObsEvent) },
            { ObsEventType.Exiting, typeof(ObsEvents.ExitingObsEvent) },
            { ObsEventType.Heartbeat, typeof(ObsEvents.HeartbeatObsEvent) },
            { ObsEventType.BroadcastCustomMessage, typeof(ObsEvents.BroadcastCustomMessageObsEvent) },
            { ObsEventType.SourceCreated, typeof(ObsEvents.SourceCreatedObsEvent) },
            { ObsEventType.SourceDestroyed, typeof(ObsEvents.SourceDestroyedObsEvent) },
            { ObsEventType.SourceVolumeChanged, typeof(ObsEvents.SourceVolumeChangedObsEvent) },
            { ObsEventType.SourceMuteStateChanged, typeof(ObsEvents.SourceMuteStateChangedObsEvent) },
            { ObsEventType.SourceAudioSyncOffsetChanged, typeof(ObsEvents.SourceAudioSyncOffsetChangedObsEvent) },
            { ObsEventType.SourceAudioMixersChanged, typeof(ObsEvents.SourceAudioMixersChangedObsEvent) },
            { ObsEventType.SourceRenamed, typeof(ObsEvents.SourceRenamedObsEvent) },
            { ObsEventType.SourceFilterAdded, typeof(ObsEvents.SourceFilterAddedObsEvent) },
            { ObsEventType.SourceFilterRemoved, typeof(ObsEvents.SourceFilterRemovedObsEvent) },
            { ObsEventType.SourceFilterVisibilityChanged, typeof(ObsEvents.SourceFilterVisibilityChangedObsEvent) },
            { ObsEventType.SourceFiltersReordered, typeof(ObsEvents.SourceFiltersReorderedObsEvent) },
            { ObsEventType.SourceOrderChanged, typeof(ObsEvents.SourceOrderChangedObsEvent) },
            { ObsEventType.SceneItemAdded, typeof(ObsEvents.SceneItemAddedObsEvent) },
            { ObsEventType.SceneItemRemoved, typeof(ObsEvents.SceneItemRemovedObsEvent) },
            { ObsEventType.SceneItemVisibilityChanged, typeof(ObsEvents.SceneItemVisibilityChangedObsEvent) },
            { ObsEventType.SceneItemLockChanged, typeof(ObsEvents.SceneItemLockChangedObsEvent) },
            { ObsEventType.SceneItemTransformChanged, typeof(ObsEvents.SceneItemTransformChangedObsEvent) },
            { ObsEventType.SceneItemSelected, typeof(ObsEvents.SceneItemSelectedObsEvent) },
            { ObsEventType.SceneItemDeselected, typeof(ObsEvents.SceneItemDeselectedObsEvent) },
            { ObsEventType.PreviewSceneChanged, typeof(ObsEvents.PreviewSceneChangedObsEvent) },
            { ObsEventType.StudioModeSwitched, typeof(ObsEvents.StudioModeSwitchedObsEvent) }
        };

        public static Type GetType(ObsEventType eventType)
        {
            return eventDictionary.TryGetValue(eventType, out Type value) ? value : null;
        }
    }
}
