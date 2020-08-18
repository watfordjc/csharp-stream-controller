﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.Data
{
    public enum ObsRequestType
    {
        GetVersion = 1,
        GetAuthRequired = 2,
        Authenticate = 3,
        SetHeartbeat = 4,
        SetFilenameFormatting = 5,
        GetFilenameFormatting = 6,
        GetStats = 7,
        BroadcastCustomMessage = 8,
        GetVideoInfo = 9,
        OpenProjector = 10,
        ListOutputs = 11,
        GetOutputInfo = 12,
        StartOutput = 13,
        StopOutput = 14,
        SetCurrentProfile = 15,
        GetCurrentProfile = 16,
        ListProfiles = 17,
        StartStopRecording = 18,
        StartRecording = 19,
        StopRecording = 20,
        PauseRecording = 21,
        ResumeRecording = 22,
        SetRecordingFolder = 23,
        GetRecordingFolder = 24,
        StartStopReplayBuffer = 25,
        StartReplayBuffer = 26,
        StopReplayBuffer = 27,
        SaveReplayBuffer = 28,
        SetCurrentSceneCollection = 29,
        GetCurrentSceneCollection = 30,
        ListSceneCollections = 31,
        GetSceneItemProperties = 32,
        SetSceneItemProperties = 33,
        ResetSceneItem = 34,
        SetSceneItemRender = 35,
        SetSceneItemPosition = 36,
        SetSceneItemTransform = 37,
        SetSceneItemCrop = 38,
        DeleteSceneItem = 39,
        DuplicateSceneItem = 40,
        SetCurrentScene = 41,
        GetCurrentScene = 42,
        GetSceneList = 43,
        ReorderSceneItems = 44,
        SetSceneTransitionOverride = 45,
        RemoveSceneTransitionOverride = 46,
        GetSceneTransitionOverride = 47,
        GetSourcesList = 48,
        GetSourceTypesList = 49,
        GetVolume = 50,
        SetVolume = 51,
        GetMute = 52,
        SetMute = 53,
        ToggleMute = 54,
        SetSourceName = 55,
        SetSyncOffset = 56,
        GetSyncOffset = 57,
        GetSourceSettings = 58,
        SetSourceSettings = 59,
        GetTextGDIPlusProperties = 60,
        SetTextGDIPlusProperties = 61,
        GetTextFreetype2Properties = 62,
        SetTextFreetype2Properties = 63,
        GetBrowserSourceProperties = 64,
        SetBrowserSourceProperties = 65,
        GetSpecialSources = 66,
        GetSourceFilters = 67,
        GetSourceFilterInfo = 68,
        AddFilterToSource = 69,
        RemoveFilterFromSource = 70,
        ReorderSourceFilter = 71,
        MoveSourceFilter = 72,
        SetSourceFilterSettings = 73,
        SetSourceFilterVisibility = 74,
        GetAudioMonitorType = 75,
        SetAudioMonitorType = 76,
        TakeSourceScreenshot = 77,
        GetStreamingStatus = 78,
        StartStopStreaming = 79,
        StartStreaming = 80,
        StopStreaming = 81,
        SetStreamSettings = 82,
        GetStreamSettings = 83,
        SaveStreamSettings = 84,
        SendCaptions = 85,
        GetStudioModeStatus = 86,
        GetPreviewScene = 87,
        SetPreviewScene = 88,
        TransitionToProgram = 89,
        EnableStudioMode = 90,
        DisableStudioMode = 91,
        ToggleStudioMode = 92,
        GetTransitionList = 93,
        GetCurrentTransition = 94,
        SetCurrentTransition = 95,
        SetTransitionDuration = 96,
        GetTransitionDuration = 97,
        GetTransitionPosition = 98
    }

    public static class ObsWsRequestReply
    {
        private static readonly Dictionary<ObsRequestType, Type> requestReplyDictionary = new Dictionary<ObsRequestType, Type>() {
            { ObsRequestType.GetVersion, typeof(ObsRequestReplies.GetVersionReply) },
            { ObsRequestType.GetAuthRequired, typeof(ObsRequestReplies.GetAuthRequiredReply) },
            { ObsRequestType.Authenticate, typeof(ObsRequestReplies.AuthenticateReply) },
            { ObsRequestType.SetHeartbeat, typeof(ObsRequestReplies.SetHeartbeatReply) },
            { ObsRequestType.SetFilenameFormatting, typeof(ObsRequestReplies.SetFilenameFormattingReply) },
            { ObsRequestType.GetFilenameFormatting, typeof(ObsRequestReplies.GetFilenameFormattingReply) },
            { ObsRequestType.GetStats, typeof(ObsRequestReplies.GetStatsReply) },
            { ObsRequestType.BroadcastCustomMessage, typeof(ObsRequestReplies.BroadcastCustomMessageReply) },
            { ObsRequestType.GetVideoInfo, typeof(ObsRequestReplies.GetVideoInfoReply) },
            { ObsRequestType.OpenProjector, typeof(ObsRequestReplies.OpenProjectorReply) },
            { ObsRequestType.ListOutputs, typeof(ObsRequestReplies.ListOutputsReply) },
            { ObsRequestType.GetOutputInfo, typeof(ObsRequestReplies.GetOutputInfoReply) },
            { ObsRequestType.StartOutput, typeof(ObsRequestReplies.StartOutputReply) },
            { ObsRequestType.StopOutput, typeof(ObsRequestReplies.StopOutputReply) },
            { ObsRequestType.SetCurrentProfile, typeof(ObsRequestReplies.SetCurrentProfileReply) },
            { ObsRequestType.GetCurrentProfile, typeof(ObsRequestReplies.GetCurrentProfileReply) },
            { ObsRequestType.ListProfiles, typeof(ObsRequestReplies.ListProfilesReply) },
            { ObsRequestType.StartStopRecording, typeof(ObsRequestReplies.StartStopRecordingReply) },
            { ObsRequestType.StartRecording, typeof(ObsRequestReplies.StartRecordingReply) },
            { ObsRequestType.StopRecording, typeof(ObsRequestReplies.StopRecordingReply) },
            { ObsRequestType.PauseRecording, typeof(ObsRequestReplies.PauseRecordingReply) },
            { ObsRequestType.ResumeRecording, typeof(ObsRequestReplies.ResumeRecordingReply) },
            { ObsRequestType.SetRecordingFolder, typeof(ObsRequestReplies.SetRecordingFolderReply) },
            { ObsRequestType.GetRecordingFolder, typeof(ObsRequestReplies.GetRecordingFolderReply) },
            { ObsRequestType.StartStopReplayBuffer, typeof(ObsRequestReplies.StartStopReplayBufferReply) },
            { ObsRequestType.StartReplayBuffer, typeof(ObsRequestReplies.StartReplayBufferReply) },
            { ObsRequestType.StopReplayBuffer, typeof(ObsRequestReplies.StopReplayBufferReply) },
            { ObsRequestType.SaveReplayBuffer, typeof(ObsRequestReplies.SaveReplayBufferReply) },
            { ObsRequestType.SetCurrentSceneCollection, typeof(ObsRequestReplies.SetCurrentSceneCollectionReply) },
            { ObsRequestType.GetCurrentSceneCollection, typeof(ObsRequestReplies.GetCurrentSceneCollectionReply) },
            { ObsRequestType.ListSceneCollections, typeof(ObsRequestReplies.ListSceneCollectionsReply) },
            { ObsRequestType.GetSceneItemProperties, typeof(ObsRequestReplies.GetSceneItemPropertiesReply) },
            { ObsRequestType.SetSceneItemProperties, typeof(ObsRequestReplies.SetSceneItemPropertiesReply) },
            { ObsRequestType.ResetSceneItem, typeof(ObsRequestReplies.ResetSceneItemReply) },
            { ObsRequestType.SetSceneItemRender, typeof(ObsRequestReplies.SetSceneItemRenderReply) },
            { ObsRequestType.SetSceneItemPosition, typeof(ObsRequestReplies.SetSceneItemPositionReply) },
            { ObsRequestType.SetSceneItemTransform, typeof(ObsRequestReplies.SetSceneItemTransformReply) },
            { ObsRequestType.SetSceneItemCrop, typeof(ObsRequestReplies.SetSceneItemCropReply) },
            { ObsRequestType.DeleteSceneItem, typeof(ObsRequestReplies.DeleteSceneItemReply) },
            { ObsRequestType.DuplicateSceneItem, typeof(ObsRequestReplies.DuplicateSceneItemReply) },
            { ObsRequestType.SetCurrentScene, typeof(ObsRequestReplies.SetCurrentSceneReply) },
            { ObsRequestType.GetCurrentScene, typeof(ObsRequestReplies.GetCurrentSceneReply) },
            { ObsRequestType.GetSceneList, typeof(ObsRequestReplies.GetSceneListReply) },
            { ObsRequestType.ReorderSceneItems, typeof(ObsRequestReplies.ReorderSceneItemsReply) },
            { ObsRequestType.SetSceneTransitionOverride, typeof(ObsRequestReplies.SetSceneTransitionOverrideReply) },
            { ObsRequestType.RemoveSceneTransitionOverride, typeof(ObsRequestReplies.RemoveSceneTransitionOverrideReply) },
            { ObsRequestType.GetSceneTransitionOverride, typeof(ObsRequestReplies.GetSceneTransitionOverrideReply) },
            { ObsRequestType.GetSourcesList, typeof(ObsRequestReplies.GetSourcesListReply) },
            { ObsRequestType.GetSourceTypesList, typeof(ObsRequestReplies.GetSourceTypesListReply) },
            { ObsRequestType.GetVolume, typeof(ObsRequestReplies.GetVolumeReply) },
            { ObsRequestType.SetVolume, typeof(ObsRequestReplies.SetVolumeReply) },
            { ObsRequestType.GetMute, typeof(ObsRequestReplies.GetMuteReply) },
            { ObsRequestType.SetMute, typeof(ObsRequestReplies.SetMuteReply) },
            { ObsRequestType.ToggleMute, typeof(ObsRequestReplies.ToggleMuteReply) },
            { ObsRequestType.SetSourceName, typeof(ObsRequestReplies.SetSourceNameReply) },
            { ObsRequestType.SetSyncOffset, typeof(ObsRequestReplies.SetSyncOffsetReply) },
            { ObsRequestType.GetSyncOffset, typeof(ObsRequestReplies.GetSyncOffsetReply) },
            { ObsRequestType.GetSourceSettings, typeof(ObsRequestReplies.GetSourceSettingsReply) },
            { ObsRequestType.SetSourceSettings, typeof(ObsRequestReplies.SetSourceSettingsReply) },
            { ObsRequestType.GetTextGDIPlusProperties, typeof(ObsRequestReplies.GetTextGDIPlusPropertiesReply) },
            { ObsRequestType.SetTextGDIPlusProperties, typeof(ObsRequestReplies.SetTextGDIPlusPropertiesReply) },
            { ObsRequestType.GetTextFreetype2Properties, typeof(ObsRequestReplies.GetTextFreetype2PropertiesReply) },
            { ObsRequestType.SetTextFreetype2Properties, typeof(ObsRequestReplies.SetTextFreetype2PropertiesReply) },
            { ObsRequestType.GetBrowserSourceProperties, typeof(ObsRequestReplies.GetBrowserSourcePropertiesReply) },
            { ObsRequestType.SetBrowserSourceProperties, typeof(ObsRequestReplies.SetBrowserSourcePropertiesReply) },
            { ObsRequestType.GetSpecialSources, typeof(ObsRequestReplies.GetSpecialSourcesReply) },
            { ObsRequestType.GetSourceFilters, typeof(ObsRequestReplies.GetSourceFiltersReply) },
            { ObsRequestType.GetSourceFilterInfo, typeof(ObsRequestReplies.GetSourceFilterInfoReply) },
            { ObsRequestType.AddFilterToSource, typeof(ObsRequestReplies.AddFilterToSourceReply) },
            { ObsRequestType.RemoveFilterFromSource, typeof(ObsRequestReplies.RemoveFilterFromSourceReply) },
            { ObsRequestType.ReorderSourceFilter, typeof(ObsRequestReplies.ReorderSourceFilterReply) },
            { ObsRequestType.MoveSourceFilter, typeof(ObsRequestReplies.MoveSourceFilterReply) },
            { ObsRequestType.SetSourceFilterSettings, typeof(ObsRequestReplies.SetSourceFilterSettingsReply) },
            { ObsRequestType.SetSourceFilterVisibility, typeof(ObsRequestReplies.SetSourceFilterVisibilityReply) },
            { ObsRequestType.GetAudioMonitorType, typeof(ObsRequestReplies.GetAudioMonitorTypeReply) },
            { ObsRequestType.SetAudioMonitorType, typeof(ObsRequestReplies.SetAudioMonitorTypeReply) },
            { ObsRequestType.TakeSourceScreenshot, typeof(ObsRequestReplies.TakeSourceScreenshotReply) },
            { ObsRequestType.GetStreamingStatus, typeof(ObsRequestReplies.GetStreamingStatusReply) },
            { ObsRequestType.StartStopStreaming, typeof(ObsRequestReplies.StartStopStreamingReply) },
            { ObsRequestType.StartStreaming, typeof(ObsRequestReplies.StartStreamingReply) },
            { ObsRequestType.StopStreaming, typeof(ObsRequestReplies.StopStreamingReply) },
            { ObsRequestType.SetStreamSettings, typeof(ObsRequestReplies.SetStreamSettingsReply) },
            { ObsRequestType.GetStreamSettings, typeof(ObsRequestReplies.GetStreamSettingsReply) },
            { ObsRequestType.SaveStreamSettings, typeof(ObsRequestReplies.SaveStreamSettingsReply) },
            { ObsRequestType.SendCaptions, typeof(ObsRequestReplies.SendCaptionsReply) },
            { ObsRequestType.GetStudioModeStatus, typeof(ObsRequestReplies.GetStudioModeStatusReply) },
            { ObsRequestType.GetPreviewScene, typeof(ObsRequestReplies.GetPreviewSceneReply) },
            { ObsRequestType.SetPreviewScene, typeof(ObsRequestReplies.SetPreviewSceneReply) },
            { ObsRequestType.TransitionToProgram, typeof(ObsRequestReplies.TransitionToProgramReply) },
            { ObsRequestType.EnableStudioMode, typeof(ObsRequestReplies.EnableStudioModeReply) },
            { ObsRequestType.DisableStudioMode, typeof(ObsRequestReplies.DisableStudioModeReply) },
            { ObsRequestType.ToggleStudioMode, typeof(ObsRequestReplies.ToggleStudioModeReply) },
            { ObsRequestType.GetTransitionList, typeof(ObsRequestReplies.GetTransitionListReply) },
            { ObsRequestType.GetCurrentTransition, typeof(ObsRequestReplies.GetCurrentTransitionReply) },
            { ObsRequestType.SetCurrentTransition, typeof(ObsRequestReplies.SetCurrentTransitionReply) },
            { ObsRequestType.SetTransitionDuration, typeof(ObsRequestReplies.SetTransitionDurationReply) },
            { ObsRequestType.GetTransitionDuration, typeof(ObsRequestReplies.GetTransitionDurationReply) },
            { ObsRequestType.GetTransitionPosition, typeof(ObsRequestReplies.GetTransitionPositionReply) }
            };

        public static Type GetType(ObsRequestType requestType)
        {
            return requestReplyDictionary.TryGetValue(requestType, out Type value) ? value : null;
        }
    }

    public static class ObsWsRequest
    {
        private static readonly Dictionary<ObsRequestType, Type> requestDictionary = new Dictionary<ObsRequestType, Type>() {
            { ObsRequestType.GetVersion, typeof(ObsRequests.GetVersionRequest) },
            { ObsRequestType.GetAuthRequired, typeof(ObsRequests.GetAuthRequiredRequest) },
            { ObsRequestType.Authenticate, typeof(ObsRequests.AuthenticateRequest) },
            { ObsRequestType.SetHeartbeat, typeof(ObsRequests.SetHeartbeatRequest) },
            { ObsRequestType.SetFilenameFormatting, typeof(ObsRequests.SetFilenameFormattingRequest) },
            { ObsRequestType.GetFilenameFormatting, typeof(ObsRequests.GetFilenameFormattingRequest) },
            { ObsRequestType.GetStats, typeof(ObsRequests.GetStatsRequest) },
            { ObsRequestType.BroadcastCustomMessage, typeof(ObsRequests.BroadcastCustomMessageRequest) },
            { ObsRequestType.GetVideoInfo, typeof(ObsRequests.GetVideoInfoRequest) },
            { ObsRequestType.OpenProjector, typeof(ObsRequests.OpenProjectorRequest) },
            { ObsRequestType.ListOutputs, typeof(ObsRequests.ListOutputsRequest) },
            { ObsRequestType.GetOutputInfo, typeof(ObsRequests.GetOutputInfoRequest) },
            { ObsRequestType.StartOutput, typeof(ObsRequests.StartOutputRequest) },
            { ObsRequestType.StopOutput, typeof(ObsRequests.StopOutputRequest) },
            { ObsRequestType.SetCurrentProfile, typeof(ObsRequests.SetCurrentProfileRequest) },
            { ObsRequestType.GetCurrentProfile, typeof(ObsRequests.GetCurrentProfileRequest) },
            { ObsRequestType.ListProfiles, typeof(ObsRequests.ListProfilesRequest) },
            { ObsRequestType.StartStopRecording, typeof(ObsRequests.StartStopRecordingRequest) },
            { ObsRequestType.StartRecording, typeof(ObsRequests.StartRecordingRequest) },
            { ObsRequestType.StopRecording, typeof(ObsRequests.StopRecordingRequest) },
            { ObsRequestType.PauseRecording, typeof(ObsRequests.PauseRecordingRequest) },
            { ObsRequestType.ResumeRecording, typeof(ObsRequests.ResumeRecordingRequest) },
            { ObsRequestType.SetRecordingFolder, typeof(ObsRequests.SetRecordingFolderRequest) },
            { ObsRequestType.GetRecordingFolder, typeof(ObsRequests.GetRecordingFolderRequest) },
            { ObsRequestType.StartStopReplayBuffer, typeof(ObsRequests.StartStopReplayBufferRequest) },
            { ObsRequestType.StartReplayBuffer, typeof(ObsRequests.StartReplayBufferRequest) },
            { ObsRequestType.StopReplayBuffer, typeof(ObsRequests.StopReplayBufferRequest) },
            { ObsRequestType.SaveReplayBuffer, typeof(ObsRequests.SaveReplayBufferRequest) },
            { ObsRequestType.SetCurrentSceneCollection, typeof(ObsRequests.SetCurrentSceneCollectionRequest) },
            { ObsRequestType.GetCurrentSceneCollection, typeof(ObsRequests.GetCurrentSceneCollectionRequest) },
            { ObsRequestType.ListSceneCollections, typeof(ObsRequests.ListSceneCollectionsRequest) },
            { ObsRequestType.GetSceneItemProperties, typeof(ObsRequests.GetSceneItemPropertiesRequest) },
            { ObsRequestType.SetSceneItemProperties, typeof(ObsRequests.SetSceneItemPropertiesRequest) },
            { ObsRequestType.ResetSceneItem, typeof(ObsRequests.ResetSceneItemRequest) },
            { ObsRequestType.SetSceneItemRender, typeof(ObsRequests.SetSceneItemRenderRequest) },
            { ObsRequestType.SetSceneItemPosition, typeof(ObsRequests.SetSceneItemPositionRequest) },
            { ObsRequestType.SetSceneItemTransform, typeof(ObsRequests.SetSceneItemTransformRequest) },
            { ObsRequestType.SetSceneItemCrop, typeof(ObsRequests.SetSceneItemCropRequest) },
            { ObsRequestType.DeleteSceneItem, typeof(ObsRequests.DeleteSceneItemRequest) },
            { ObsRequestType.DuplicateSceneItem, typeof(ObsRequests.DuplicateSceneItemRequest) },
            { ObsRequestType.SetCurrentScene, typeof(ObsRequests.SetCurrentSceneRequest) },
            { ObsRequestType.GetCurrentScene, typeof(ObsRequests.GetCurrentSceneRequest) },
            { ObsRequestType.GetSceneList, typeof(ObsRequests.GetSceneListRequest) },
            { ObsRequestType.ReorderSceneItems, typeof(ObsRequests.ReorderSceneItemsRequest) },
            { ObsRequestType.SetSceneTransitionOverride, typeof(ObsRequests.SetSceneTransitionOverrideRequest) },
            { ObsRequestType.RemoveSceneTransitionOverride, typeof(ObsRequests.RemoveSceneTransitionOverrideRequest) },
            { ObsRequestType.GetSceneTransitionOverride, typeof(ObsRequests.GetSceneTransitionOverrideRequest) },
            { ObsRequestType.GetSourcesList, typeof(ObsRequests.GetSourcesListRequest) },
            { ObsRequestType.GetSourceTypesList, typeof(ObsRequests.GetSourceTypesListRequest) },
            { ObsRequestType.GetVolume, typeof(ObsRequests.GetVolumeRequest) },
            { ObsRequestType.SetVolume, typeof(ObsRequests.SetVolumeRequest) },
            { ObsRequestType.GetMute, typeof(ObsRequests.GetMuteRequest) },
            { ObsRequestType.SetMute, typeof(ObsRequests.SetMuteRequest) },
            { ObsRequestType.ToggleMute, typeof(ObsRequests.ToggleMuteRequest) },
            { ObsRequestType.SetSourceName, typeof(ObsRequests.SetSourceNameRequest) },
            { ObsRequestType.SetSyncOffset, typeof(ObsRequests.SetSyncOffsetRequest) },
            { ObsRequestType.GetSyncOffset, typeof(ObsRequests.GetSyncOffsetRequest) },
            { ObsRequestType.GetSourceSettings, typeof(ObsRequests.GetSourceSettingsRequest) },
            { ObsRequestType.SetSourceSettings, typeof(ObsRequests.SetSourceSettingsRequest) },
            { ObsRequestType.GetTextGDIPlusProperties, typeof(ObsRequests.GetTextGDIPlusPropertiesRequest) },
            { ObsRequestType.SetTextGDIPlusProperties, typeof(ObsRequests.SetTextGDIPlusPropertiesRequest) },
            { ObsRequestType.GetTextFreetype2Properties, typeof(ObsRequests.GetTextFreetype2PropertiesRequest) },
            { ObsRequestType.SetTextFreetype2Properties, typeof(ObsRequests.SetTextFreetype2PropertiesRequest) },
            { ObsRequestType.GetBrowserSourceProperties, typeof(ObsRequests.GetBrowserSourcePropertiesRequest) },
            { ObsRequestType.SetBrowserSourceProperties, typeof(ObsRequests.SetBrowserSourcePropertiesRequest) },
            { ObsRequestType.GetSpecialSources, typeof(ObsRequests.GetSpecialSourcesRequest) },
            { ObsRequestType.GetSourceFilters, typeof(ObsRequests.GetSourceFiltersRequest) },
            { ObsRequestType.GetSourceFilterInfo, typeof(ObsRequests.GetSourceFilterInfoRequest) },
            { ObsRequestType.AddFilterToSource, typeof(ObsRequests.AddFilterToSourceRequest) },
            { ObsRequestType.RemoveFilterFromSource, typeof(ObsRequests.RemoveFilterFromSourceRequest) },
            { ObsRequestType.ReorderSourceFilter, typeof(ObsRequests.ReorderSourceFilterRequest) },
            { ObsRequestType.MoveSourceFilter, typeof(ObsRequests.MoveSourceFilterRequest) },
            { ObsRequestType.SetSourceFilterSettings, typeof(ObsRequests.SetSourceFilterSettingsRequest) },
            { ObsRequestType.SetSourceFilterVisibility, typeof(ObsRequests.SetSourceFilterVisibilityRequest) },
            { ObsRequestType.GetAudioMonitorType, typeof(ObsRequests.GetAudioMonitorTypeRequest) },
            { ObsRequestType.SetAudioMonitorType, typeof(ObsRequests.SetAudioMonitorTypeRequest) },
            { ObsRequestType.TakeSourceScreenshot, typeof(ObsRequests.TakeSourceScreenshotRequest) },
            { ObsRequestType.GetStreamingStatus, typeof(ObsRequests.GetStreamingStatusRequest) },
            { ObsRequestType.StartStopStreaming, typeof(ObsRequests.StartStopStreamingRequest) },
            { ObsRequestType.StartStreaming, typeof(ObsRequests.StartStreamingRequest) },
            { ObsRequestType.StopStreaming, typeof(ObsRequests.StopStreamingRequest) },
            { ObsRequestType.SetStreamSettings, typeof(ObsRequests.SetStreamSettingsRequest) },
            { ObsRequestType.GetStreamSettings, typeof(ObsRequests.GetStreamSettingsRequest) },
            { ObsRequestType.SaveStreamSettings, typeof(ObsRequests.SaveStreamSettingsRequest) },
            { ObsRequestType.SendCaptions, typeof(ObsRequests.SendCaptionsRequest) },
            { ObsRequestType.GetStudioModeStatus, typeof(ObsRequests.GetStudioModeStatusRequest) },
            { ObsRequestType.GetPreviewScene, typeof(ObsRequests.GetPreviewSceneRequest) },
            { ObsRequestType.SetPreviewScene, typeof(ObsRequests.SetPreviewSceneRequest) },
            { ObsRequestType.TransitionToProgram, typeof(ObsRequests.TransitionToProgramRequest) },
            { ObsRequestType.EnableStudioMode, typeof(ObsRequests.EnableStudioModeRequest) },
            { ObsRequestType.DisableStudioMode, typeof(ObsRequests.DisableStudioModeRequest) },
            { ObsRequestType.ToggleStudioMode, typeof(ObsRequests.ToggleStudioModeRequest) },
            { ObsRequestType.GetTransitionList, typeof(ObsRequests.GetTransitionListRequest) },
            { ObsRequestType.GetCurrentTransition, typeof(ObsRequests.GetCurrentTransitionRequest) },
            { ObsRequestType.SetCurrentTransition, typeof(ObsRequests.SetCurrentTransitionRequest) },
            { ObsRequestType.SetTransitionDuration, typeof(ObsRequests.SetTransitionDurationRequest) },
            { ObsRequestType.GetTransitionDuration, typeof(ObsRequests.GetTransitionDurationRequest) },
            { ObsRequestType.GetTransitionPosition, typeof(ObsRequests.GetTransitionPositionRequest) }
            };

        /// <summary>
        /// Classes for partial requests are needed until .NET Core supports ignoring default properties during serialization
        /// </summary>
        private static readonly Dictionary<Type, ObsRequestType> partialRequestDictionary = new Dictionary<Type, ObsRequestType>()
        {
            { typeof(ObsRequests.SetTextGDIPlusPropertiesRequestTextPropertyOnly), ObsRequestType.SetTextGDIPlusProperties }
        };

        public static Type GetType(ObsRequestType requestType)
        {
            return requestDictionary.TryGetValue(requestType, out Type value) ? value : null;
        }

        public static object GetInstanceOfType(ObsRequestType requestType)
        {
            return requestDictionary.TryGetValue(requestType, out Type value) ? Activator.CreateInstance(value) : null;
        }

        public static Data.ObsRequestType GetRequestEnum(Type objectType)
        {
            Data.ObsRequestType type = requestDictionary.FirstOrDefault(k => k.Value == objectType).Key;
            if (type != default)
            {
                return type;
            }
            else
            {
                return partialRequestDictionary.TryGetValue(objectType, out ObsRequestType value) ? value : value;
            }
        }
    }
}
