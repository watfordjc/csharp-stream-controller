﻿using System;
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
}
