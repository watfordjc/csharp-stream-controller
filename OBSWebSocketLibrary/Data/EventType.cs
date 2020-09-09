using System;
using System.Collections.Generic;
using System.Text;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.Data
{
    public enum ObsEventType
    {
        Unknown = 0,
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
        SourceAudioDeactivated = 35,
        SourceAudioActivated = 36,
        SourceAudioSyncOffsetChanged = 37,
        SourceAudioMixersChanged = 38,
        SourceRenamed = 39,
        SourceFilterAdded = 40,
        SourceFilterRemoved = 41,
        SourceFilterVisibilityChanged = 42,
        SourceFiltersReordered = 43,
        MediaPlaying = 44,
        MediaPaused = 45,
        MediaRestarted = 46,
        MediaStopped = 47,
        MediaNext = 48,
        MediaPrevious = 49,
        MediaStarted = 50,
        MediaEnded = 51,
        SourceOrderChanged = 52,
        SceneItemAdded = 53,
        SceneItemRemoved = 54,
        SceneItemVisibilityChanged = 55,
        SceneItemLockChanged = 56,
        SceneItemTransformChanged = 57,
        SceneItemSelected = 58,
        SceneItemDeselected = 59,
        PreviewSceneChanged = 60,
        StudioModeSwitched = 61
    }

    public static class ObsWsEvent
    {
        private static readonly Dictionary<ObsEventType, Type> eventDictionary = new Dictionary<ObsEventType, Type>()
        {
            { ObsEventType.Unknown, typeof(ObsEvents.EventBase) },
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
            { ObsEventType.SourceAudioDeactivated, typeof(ObsEvents.SourceAudioDeactivatedObsEvent) },
            { ObsEventType.SourceAudioActivated, typeof(ObsEvents.SourceAudioActivatedObsEvent) },
            { ObsEventType.SourceAudioSyncOffsetChanged, typeof(ObsEvents.SourceAudioSyncOffsetChangedObsEvent) },
            { ObsEventType.SourceAudioMixersChanged, typeof(ObsEvents.SourceAudioMixersChangedObsEvent) },
            { ObsEventType.SourceRenamed, typeof(ObsEvents.SourceRenamedObsEvent) },
            { ObsEventType.SourceFilterAdded, typeof(ObsEvents.SourceFilterAddedObsEvent) },
            { ObsEventType.SourceFilterRemoved, typeof(ObsEvents.SourceFilterRemovedObsEvent) },
            { ObsEventType.SourceFilterVisibilityChanged, typeof(ObsEvents.SourceFilterVisibilityChangedObsEvent) },
            { ObsEventType.SourceFiltersReordered, typeof(ObsEvents.SourceFiltersReorderedObsEvent) },
            { ObsEventType.MediaPlaying, typeof(ObsEvents.MediaPlayingObsEvent) },
            { ObsEventType.MediaPaused, typeof(ObsEvents.MediaPausedObsEvent) },
            { ObsEventType.MediaRestarted, typeof(ObsEvents.MediaRestartedObsEvent) },
            { ObsEventType.MediaStopped, typeof(ObsEvents.MediaStoppedObsEvent) },
            { ObsEventType.MediaNext, typeof(ObsEvents.MediaNextObsEvent) },
            { ObsEventType.MediaPrevious, typeof(ObsEvents.MediaPreviousObsEvent) },
            { ObsEventType.MediaStarted, typeof(ObsEvents.MediaStartedObsEvent) },
            { ObsEventType.MediaEnded, typeof(ObsEvents.MediaEndedObsEvent) },
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

        public static Type GetType(string obsEventType)
        {
            ObsEventType type = (ObsEventType)Enum.Parse(typeof(ObsEventType), obsEventType);
            return eventDictionary.TryGetValue(type, out Type value) ? value : null; ;
        }
    }
}
