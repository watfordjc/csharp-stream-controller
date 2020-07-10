﻿using NAudio.CoreAudioApi;
using NAudio.Wave;
using uk.JohnCook.dotnet.NAudioWrapperLibrary;
using uk.JohnCook.dotnet.OBSWebSocketLibrary;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.TypeDefs;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsEvents;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequests;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequestReplies;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.Data;
using uk.JohnCook.dotnet.WebSocketLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace uk.JohnCook.dotnet.StreamController
{
    /// <summary>
    /// Interaction logic for AudioCheck.xaml
    /// </summary>
    public partial class AudioCheck : Window, IDisposable
    {
        private readonly SynchronizationContext _Context;
        private readonly ObsWsClient webSocket;
        private readonly TaskCompletionSource<bool> audioDevicesEnumerated = new TaskCompletionSource<bool>();
        private string connectionError = String.Empty;
        private WaveOutEvent silentAudioEvent = null;
        private static readonly Brush primaryBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xE2, 0xC1, 0xEA));
        private static readonly Brush secondaryBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xC5, 0xC0, 0xEB));
        private CancellationTokenSource pulseCancellationToken;
        private readonly System.Timers.Timer _ReconnectCountdownTimer = new System.Timers.Timer(1000);
        private int _ReconnectTimeRemaining;
        private readonly ObservableCollection<ObsScene> sceneList = new ObservableCollection<ObsScene>();
        private ObsScene currentScene;
        private GetSourceTypesListReply sourceTypes;
        private readonly Dictionary<string, object> obsSourceDictionary = new Dictionary<string, object>();
        private readonly Dictionary<int, ObsScene> obsSceneItemSceneDictionary = new Dictionary<int, ObsScene>();
        private bool disposedValue;

        #region Instantiation and initialisation

        public AudioCheck()
        {
            InitializeComponent();
            _Context = SynchronizationContext.Current;
            Uri obs_uri = new UriBuilder(
                Preferences.Default.obs_uri_scheme,
                Preferences.Default.obs_uri_host,
                int.Parse(Preferences.Default.obs_uri_port, CultureInfo.InvariantCulture)
                ).Uri;
            webSocket = new ObsWsClient(obs_uri)
            {
                AutoReconnect = Preferences.Default.obs_auto_reconnect
            };
        }

        private async void Window_ContentRendered(object sender, EventArgs e)
        {
            pulseCancellationToken = new CancellationTokenSource();
            UpdateUIConnectStatus(null, null, null);
            AudioInterfaceCollection.Instance.DeviceCollectionEnumerated += AudioDevicesEnumerated;
            if (AudioInterfaceCollection.Instance.DevicesAreEnumerated)
            {
                AudioDevicesEnumerated(this, true);
            }
            AudioInterfaceCollection.Instance.DefaultDeviceChange += DefaultAudioDeviceChanged;
            webSocket.SetExponentialBackoff(Preferences.Default.obs_reconnect_min_seconds, Preferences.Default.obs_reconnect_max_minutes);
            webSocket.StateChange += WebSocket_StateChange_ContextSwitch;
            webSocket.ErrorState += WebSocket_Error_ContextSwitch;
            webSocket.OnObsEvent += WebSocket_Event_ContextSwitch;
            webSocket.OnObsReply += Websocket_Reply_ContextSwitch;
            _ReconnectCountdownTimer.Elapsed += ReconnectCountdownTimer_Elapsed;
            await ObsWebsocketConnect().ConfigureAwait(true);
        }

        private async void Window_Closed(object sender, EventArgs e)
        {
            AudioInterfaceCollection.Instance.DeviceCollectionEnumerated -= AudioDevicesEnumerated;
            AudioInterfaceCollection.Instance.DefaultDeviceChange -= DefaultAudioDeviceChanged;
            webSocket.StateChange -= WebSocket_StateChange_ContextSwitch;
            webSocket.ErrorState -= WebSocket_Error_ContextSwitch;
            webSocket.OnObsEvent -= WebSocket_Event_ContextSwitch;
            webSocket.OnObsReply -= Websocket_Reply_ContextSwitch;
            _ReconnectCountdownTimer.Elapsed -= ReconnectCountdownTimer_Elapsed;
            webSocket.AutoReconnect = false;
            await webSocket.DisconnectAsync(true).ConfigureAwait(true);
            sceneList.Clear();
            currentScene = null;
            sourceTypes = null;
            obsSourceDictionary.Clear();
            obsSceneItemSceneDictionary.Clear();
        }
        #endregion

        #region Audio interfaces

        private void AudioDevicesEnumerated(object sender, bool deviceEnumerationComplete)
        {
            if (deviceEnumerationComplete)
            {
                audioDevicesEnumerated.SetResult(true);
            }
        }

        private async void DefaultAudioDeviceChanged(object sender, DataFlow dataFlow)
        {
            if (dataFlow == DataFlow.Render)
            {
                await audioDevicesEnumerated.Task.ConfigureAwait(false);
                DisplayPortAudioWorkaround();
            }
        }

        private void DisplayPortAudioWorkaround()
        {
            if (AudioInterfaceCollection.Instance.DefaultRender.FriendlyName.Contains("NVIDIA", StringComparison.Ordinal) && silentAudioEvent?.PlaybackState != PlaybackState.Playing)
            {
                _ = Task.Run(
                    () => StartPlaySilence(AudioInterfaceCollection.Instance.DefaultRender)
                );
            }
            else
            {
                _ = Task.Run(
                    () => StopPlaySilence()
                    );
            }
        }

        private Task StartPlaySilence(SharedModels.AudioInterface audioInterface)
        {
            if (audioInterface.IsActive && silentAudioEvent?.PlaybackState != PlaybackState.Playing)
            {
                SilenceProvider provider = new SilenceProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
                silentAudioEvent = new WaveOutEvent()
                {
                    DeviceNumber = GetWaveOutDeviceNumber(audioInterface)
                };
                silentAudioEvent.Init(provider);
                silentAudioEvent.Play();
            }
            return Task.CompletedTask;
        }

        private Task StopPlaySilence()
        {
            if (silentAudioEvent?.PlaybackState == PlaybackState.Playing)
            {
                silentAudioEvent.Stop();
                silentAudioEvent.Dispose();
            }
            return Task.CompletedTask;
        }

        private static int GetWaveOutDeviceNumber(SharedModels.AudioInterface audioInterface)
        {
            int deviceNameMaxLength = Math.Min(audioInterface.FriendlyName.Length, 31);
            string deviceNameTruncated = audioInterface.FriendlyName.Substring(0, deviceNameMaxLength);
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                if (WaveOut.GetCapabilities(i).ProductName == deviceNameTruncated)
                {
                    return i;
                }
            }
            return -1;
        }

        #endregion

        #region obs-websocket

        private async Task ObsWebsocketConnect()
        {
            await webSocket.AutoReconnectConnectAsync().ConfigureAwait(true);
        }

        private void ReconnectCountdownTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ReconnectTimeRemaining--;
            if (_ReconnectTimeRemaining > 0)
            {
                UpdateUIConnectStatus(TimeSpan.FromSeconds(_ReconnectTimeRemaining).ToString("c", CultureInfo.CurrentCulture), null, null);
            }
        }

        private void WebSocket_StateChange_ContextSwitch(object sender, WebSocketState newState)
        {
            _Context.Send(
                x => WebSocket_StateChange(newState),
                null);
        }

        private async void WebSocket_StateChange(WebSocketState newState)
        {
            if (newState == WebSocketState.Open)
            {
                connectionError = String.Empty;
                _ReconnectCountdownTimer.Stop();
                UpdateUIConnectStatus(String.Empty, Brushes.DarkGreen, null);
                obsSourceDictionary.Clear();
                await Obs_Get(ObsRequestType.GetSourceTypesList).ConfigureAwait(true);
            }
            else if (newState != WebSocketState.Connecting)
            {
                _ReconnectCountdownTimer.Start();
                UpdateUIConnectStatus(null, Brushes.Red, null);
            }
            else
            {
                connectionError = String.Empty;
                _ReconnectCountdownTimer.Stop();
                UpdateUIConnectStatus("\u2026", Brushes.DarkGoldenrod, null);
            }
        }

        private void WebSocket_Error_ContextSwitch(object sender, WsClientErrorMessage e)
        {
            _Context.Send(
                x => WebSocket_Error(e),
                null);
        }

        private void WebSocket_Error(WsClientErrorMessage e)
        {
            if (e.ReconnectDelay > 0)
            {
                UpdateUIConnectStatus(TimeSpan.FromSeconds(e.ReconnectDelay).ToString("c", CultureInfo.CurrentCulture), null, null);
                _ReconnectTimeRemaining = e.ReconnectDelay;
            }
            if (e.Error != null)
            {
                connectionError = $"{e.Error.Message}\n{e.Error.InnerException?.Message}";
            }
        }

        private void WebSocket_Event_ContextSwitch(object sender, ObsEventObject eventObject)
        {
            _Context.Send(
                x => WebSocket_Event(eventObject),
                null);
        }

        private async void WebSocket_Event(ObsEventObject eventObject)
        {
            switch (eventObject.EventType)
            {
                case ObsEventType.Heartbeat:
                    Heartbeat_Event((HeartbeatObsEvent)eventObject.MessageObject);
                    break;
                case ObsEventType.SwitchScenes:
                    SwitchScenes_Event((SwitchScenesObsEvent)eventObject.MessageObject);
                    break;
                case ObsEventType.ScenesChanged:
                    break;
                case ObsEventType.TransitionBegin:
                    string nextScene = ((TransitionBeginObsEvent)eventObject.MessageObject).ToScene;
                    await UpdateTransitionMessage($"\u27a1\ufe0f {nextScene}\u2026").ConfigureAwait(true);
                    break;
                case ObsEventType.TransitionEnd:
                case ObsEventType.TransitionVideoEnd:
                    await UpdateTransitionMessage(String.Empty).ConfigureAwait(true);
                    break;
                case ObsEventType.SourceOrderChanged:
                    SourceOrderChanged_Event((SourceOrderChangedObsEvent)eventObject.MessageObject);
                    break;
                case ObsEventType.SourceCreated:
                    SourceCreated_Event((SourceCreatedObsEvent)eventObject.MessageObject);
                    break;
                case ObsEventType.SceneItemAdded:
                    SceneItemAdded_Event((SceneItemAddedObsEvent)eventObject.MessageObject);
                    break;
                case ObsEventType.SceneItemRemoved:
                    SceneItemRemoved_Event((SceneItemRemovedObsEvent)eventObject.MessageObject);
                    break;
                case ObsEventType.SourceDestroyed:
                    SourceDestroyed_Event((SourceDestroyedObsEvent)eventObject.MessageObject);
                    break;
                case ObsEventType.SceneItemTransformChanged:
                    SceneItemTransformChanged_Event((SceneItemTransformChangedObsEvent)eventObject.MessageObject);
                    break;
                case ObsEventType.SourceVolumeChanged:
                    SourceVolumeChanged_Event((SourceVolumeChangedObsEvent)eventObject.MessageObject);
                    break;
                case ObsEventType.SourceMuteStateChanged:
                    SourceMuteStateChanged_Event((SourceMuteStateChangedObsEvent)eventObject.MessageObject);
                    break;
                case ObsEventType.SourceAudioSyncOffsetChanged:
                    SourceAudioSyncOffsetChanged_Event((SourceAudioSyncOffsetChangedObsEvent)eventObject.MessageObject);
                    break;
                case ObsEventType.SourceRenamed:
                    SourceRenamed_Event((SourceRenamedObsEvent)eventObject.MessageObject);
                    break;
                case ObsEventType.SceneItemVisibilityChanged:
                    SceneItemVisibilityChanged_Event((SceneItemVisibilityChangedObsEvent)eventObject.MessageObject);
                    break;
                case ObsEventType.SceneItemLockChanged:
                    SceneItemLockChanged_Event((SceneItemLockChangedObsEvent)eventObject.MessageObject);
                    break;
                case ObsEventType.SceneCollectionChanged:
                    tbTransitioning.Text = String.Empty;
                    break;
                case ObsEventType.SourceAudioMixersChanged:
                    SourceAudioMixersChanged_Event((SourceAudioMixersChangedObsEvent)eventObject.MessageObject);
                    break;
                case ObsEventType.SourceFilterAdded:
                    SourceFilterAdded_Event((SourceFilterAddedObsEvent)eventObject.MessageObject);
                    break;
                case ObsEventType.SourceFilterRemoved:
                    SourceFilterRemoved_Event((SourceFilterRemovedObsEvent)eventObject.MessageObject);
                    break;
                case ObsEventType.SourceFilterVisibilityChanged:
                    SourceFilterVisibilityChanged_Event((SourceFilterVisibilityChangedObsEvent)eventObject.MessageObject);
                    break;
                case ObsEventType.SourceFiltersReordered:
                    SourceFiltersReordered_Event((SourceFiltersReorderedObsEvent)eventObject.MessageObject);
                    break;
                case ObsEventType.SceneItemSelected:
                    SceneItemSelectedObsEvent itemSelectedEvent = eventObject.MessageObject as SceneItemSelectedObsEvent;
                    Trace.WriteLine($"Item {itemSelectedEvent.ItemName} has been selected in scene {itemSelectedEvent.SceneName}");
                    break;
                case ObsEventType.SceneItemDeselected:
                    SceneItemDeselectedObsEvent itemDeselectedEvent = eventObject.MessageObject as SceneItemDeselectedObsEvent;
                    Trace.WriteLine($"Item {itemDeselectedEvent.ItemName} has been deselected in scene {itemDeselectedEvent.SceneName}");
                    break;
            }
        }

        private async Task PopulateSceneItemSources(IList<ObsSceneItem> sceneItems, ObsScene scene)
        {
            foreach (ObsSceneItem sceneItem in sceneItems)
            {
                sceneItem.Source = (BaseType)obsSourceDictionary.GetValueOrDefault(sceneItem.Name);
                if (sceneItem.GroupChildren != null)
                {
                    await PopulateSceneItemSources(sceneItem.GroupChildren, scene).ConfigureAwait(true);
                }
                obsSceneItemSceneDictionary[sceneItem.Id] = scene;
                await Obs_Get(ObsRequestType.GetSceneItemProperties, sceneItem.Name, scene.Name).ConfigureAwait(true);
                await Obs_Get(ObsRequestType.GetAudioMonitorType, sceneItem.Name).ConfigureAwait(true);
            }
        }

        private void Websocket_Reply_ContextSwitch(object sender, ObsReplyObject replyObject)
        {
            _Context.Send(
                x => Websocket_Reply(replyObject),
                null);
        }

        private async void Websocket_Reply(ObsReplyObject replyObject)
        {
            switch (replyObject.RequestType)
            {
                case ObsRequestType.GetCurrentScene:
                    currentScene = replyObject.MessageObject as ObsScene;
                    await PopulateSceneItemSources(currentScene.Sources, currentScene).ConfigureAwait(true);
                    await UpdateSceneInformation().ConfigureAwait(true);
                    break;
                case ObsRequestType.GetSceneList:
                    ReadOnlyMemory<char> currentSceneName = (replyObject.MessageObject as GetSceneListReply).CurrentScene.AsMemory();
                    sceneList.Clear();
                    foreach (ObsScene scene in (replyObject.MessageObject as GetSceneListReply).Scenes)
                    {
                        sceneList.Add(scene);
                        await PopulateSceneItemSources(scene.Sources, scene).ConfigureAwait(true);
                    }
                    Debug.Assert(sceneList.Any(x => x.Name == currentSceneName.ToString()), $"Scene {currentSceneName} wasn't added to sceneList.");
                    currentScene = sceneList.First(x => x.Name == currentSceneName.ToString());
                    await UpdateSceneInformation().ConfigureAwait(true);
                    break;
                case ObsRequestType.GetSourceTypesList:
                    sourceTypes = (GetSourceTypesListReply)replyObject.MessageObject;
                    await Obs_Get(ObsRequestType.GetSourcesList).ConfigureAwait(true);
                    break;
                case ObsRequestType.GetSourcesList:
                    GetSourcesListReply sourcesList = (GetSourcesListReply)replyObject.MessageObject;
                    foreach (ObsWsReplySource source in sourcesList.Sources)
                    {
                        await Obs_Get(ObsRequestType.GetSourceSettings, source.Name).ConfigureAwait(true);
                    }
                    await GetDeviceIdsForSources().ConfigureAwait(true);
                    await Obs_Get(ObsRequestType.GetSceneList).ConfigureAwait(true);
                    await Obs_Get(ObsRequestType.GetTransitionList).ConfigureAwait(true);
                    break;
                case ObsRequestType.GetSourceSettings:
                    GetSourceSettingsReply sourceSettings = (GetSourceSettingsReply)replyObject.MessageObject;
                    BaseType newSource = (BaseType)sourceSettings.SourceSettingsObj;
                    newSource.Name = sourceSettings.SourceName;
                    Debug.Assert(sourceTypes.Types.Any(x => x.TypeId == sourceSettings.SourceType), $"Source type {sourceSettings.SourceType} isn't in list from server.");
                    newSource.Type = sourceTypes.Types.First(x => x.TypeId == sourceSettings.SourceType);
                    obsSourceDictionary[newSource.Name] = newSource;
                    await Obs_Get(ObsRequestType.GetSourceFilters, sourceSettings.SourceName).ConfigureAwait(true);
                    break;
                case ObsRequestType.GetSourceFilters:
                    BaseType sourceToModify = obsSourceDictionary[(replyObject.RequestMetadata.OriginalRequestData as GetSourceFiltersRequest).SourceName] as BaseType;
                    foreach (ObsWsReplyFilter filter in (replyObject.MessageObject as GetSourceFiltersReply).Filters)
                    {
                        sourceToModify.Filters.Add(filter);
                    }
                    break;
                case ObsRequestType.GetTransitionList:
                    GetTransitionListReply transitionList = (GetTransitionListReply)replyObject.MessageObject;
                    foreach (ObsWsTransitionName transition in transitionList.Transitions)
                    {
                        //  Trace.WriteLine($"{transition.Name}");
                    }
                    break;
                case ObsRequestType.GetSceneItemProperties:
                    GetSceneItemPropertiesReply itemProps = (GetSceneItemPropertiesReply)replyObject.MessageObject;
                    ObsSceneItem existingSceneItem = GetSceneItemFromSceneItemId(itemProps.ItemId, null);
                    existingSceneItem.Transform = itemProps;
                    break;
                case ObsRequestType.GetAudioMonitorType:
                    GetAudioMonitorTypeRequest requestSent = replyObject.RequestMetadata.OriginalRequestData as GetAudioMonitorTypeRequest;
                    (obsSourceDictionary[requestSent.SourceName] as BaseType).MonitorType = (replyObject.MessageObject as GetAudioMonitorTypeReply).MonitorType;
                    break;
                default:
                    break;
            }
        }

        private ObsSceneItem GetSceneItemFromSceneItemId(int sceneItemId, ObservableCollection<ObsSceneItem> sceneItems)
        {
            /*
             * 
             * DANGER: Untested Recursion
             * 
             */
            ObsSceneItem returnValue = null;
            if (sceneItems == null)
            {
                ObsScene firstScene = obsSceneItemSceneDictionary[sceneItemId];
                returnValue = GetSceneItemFromSceneItemId(sceneItemId, firstScene.Sources);
            }
            if (returnValue == null && sceneItems != null)
            {
                returnValue = sceneItems.Where(x => x.Id == sceneItemId).First();
            }

            if (returnValue == null)
            {
                foreach (ObsSceneItem nextSceneItem in sceneItems.Where(x => x.GroupChildren.Count > 0).ToArray())
                {
                    returnValue = GetSceneItemFromSceneItemId(sceneItemId, nextSceneItem.GroupChildren);
                    if (returnValue != null)
                    {
                        break;
                    }
                }
            }
            return returnValue;
        }

        private async Task GetDeviceIdsForSources()
        {
            await audioDevicesEnumerated.Task.ConfigureAwait(false);
            while (webSocket.WaitingForReplyForType(ObsRequestType.GetSourceSettings))
            {
                await Task.Delay(250).ConfigureAwait(false);
            }

            foreach (BaseType source in obsSourceDictionary.Values)
            {
                GetDeviceIdForSource(source);
            }
        }

        private static void GetDeviceIdForSource(BaseType source)
        {
            switch (ObsTypes.ObsTypeNameDictionary[source.Type.TypeId])
            {
                case ObsSourceType.WasapiOutputCapture:
                case ObsSourceType.WasapiInputCapture:
                case ObsSourceType.DShowInput:
                    break;
                default:
                    return;
            }
            DependencyProperties dependencies = source.Dependencies;
            Debug.Assert(ObsTypes.ObsTypeNameDictionary.ContainsValue(ObsSourceType.DShowInput), "DShowInput not in type dictionary.");
            if (source.Type.TypeId == ObsTypes.ObsTypeNameDictionary.First(x => x.Value == ObsSourceType.DShowInput).Key)
            {
                ReadOnlyMemory<char> deviceName = ((DShowInput)source).AudioDeviceId.AsMemory();
                dependencies.AudioDeviceId = AudioInterfaceCollection.GetAudioInterfaceByName(deviceName[0..^1].ToString()).ID;
            }
            dependencies.AudioInterface = AudioInterfaceCollection.GetAudioInterfaceById(dependencies.AudioDeviceId);
            //Trace.WriteLine($"{sourceReply.SourceName} -> {sourceReply.SourceType} -> device_id: {audioInterface?.ID} AKA {audioInterface?.FriendlyName}");
            // WASAPI and DirectShow source types should reference an audio device
            if (!dependencies.HasAudioInterface)
            {
                dependencies.DependencyProblem = true;
                return;
            }
        }

        #region obs-events

        private void Heartbeat_Event(HeartbeatObsEvent messageObject)
        {
            if (!pulseCancellationToken.IsCancellationRequested)
            {
                pulseCancellationToken.Cancel();
            }
            pulseCancellationToken = new CancellationTokenSource();
            try
            {
                UpdateUIConnectStatus(
                    null,
                    messageObject.Pulse ? primaryBrush : secondaryBrush,
                    Brushes.Gray);
            }
            catch (TaskCanceledException)
            {
                UpdateUIConnectStatus(null, Brushes.Gray, null);
            }
        }

        private void SwitchScenes_Event(SwitchScenesObsEvent messageObject)
        {
            if (sceneList.Any(x => x.Name == messageObject.SceneName))
            {
                currentScene = sceneList.First(x => x.Name == messageObject.SceneName);
                UpdateSceneInformation();
            }
        }

        private void SourceOrderChanged_Event(SourceOrderChangedObsEvent messageObject)
        {
            if (messageObject.SceneName != currentScene.Name)
            {
                return;
            }
            List<int> collectionOrderList = messageObject.SceneItems.Select(x => x.ItemId).ToList();
            ListCollectionView listCollection = (ListCollectionView)CollectionViewSource.GetDefaultView(lbSourceList.ItemsSource);
            listCollection.CustomSort = new Utils.OrderSceneItemsByListOfIds(collectionOrderList);
        }

        private void SceneItemRemoved_Event(SceneItemRemovedObsEvent messageObject)
        {
            if (!sceneList.Any(x => x.Name == messageObject.SceneName)) { return; }

            ObsScene scene = sceneList.First(x => x.Name == messageObject.SceneName);
            ObsSceneItem sceneItem = scene.Sources.FirstOrDefault(x => x.Name == messageObject.ItemName);
            if (sceneItem != default)
            {
                scene.Sources.Remove(sceneItem);
            }
        }

        private void SourceCreated_Event(SourceCreatedObsEvent messageObject)
        {
            ObsSourceType sourceType = ObsTypes.ObsTypeNameDictionary[messageObject.SourceKind];
            object createdSource = messageObject.SourceSettingsObj;
            ObsWsReplyType replyType = sourceTypes.Types.FirstOrDefault(x => x.TypeId == messageObject.SourceKind);
            if (replyType == default) { return; }

            (createdSource as BaseType).Type = replyType;
            (createdSource as BaseType).Name = messageObject.SourceName;
            obsSourceDictionary[messageObject.SourceName] = createdSource;
            GetDeviceIdForSource(createdSource as BaseType);
            if (sourceType == ObsSourceType.Scene)
            {
                ObsScene newScene = new ObsScene()
                {
                    Name = (createdSource as Scene).Name,
                    Sources = new ObservableCollection<ObsSceneItem>()
                };
                sceneList.Add(newScene);
            }
        }

        private void SourceDestroyed_Event(SourceDestroyedObsEvent messageObject)
        {
            obsSourceDictionary.Remove(messageObject.SourceName);
            if (ObsTypes.ObsTypeNameDictionary[messageObject.SourceKind] == ObsSourceType.Scene)
            {
                ObsScene scene = sceneList.FirstOrDefault(x => x.Name == messageObject.SourceName);
                if (scene != default)
                {
                    sceneList.Remove(scene);
                }
            }
        }

        private void SceneItemAdded_Event(SceneItemAddedObsEvent messageObject)
        {
            // Don't add scenes to themselves
            if (messageObject.SceneName == messageObject.ItemName) { return; }
            if (!obsSourceDictionary.TryGetValue(messageObject.ItemName, out object source))
            {
                return;
            }
            ObsSceneItem newSceneItem = new ObsSceneItem()
            {
                Name = messageObject.ItemName,
                Id = messageObject.ItemId,
                Source = (BaseType)source
            };
            newSceneItem.Type = newSceneItem.Source.Type.TypeId;
            if (sceneList.Any(x => x.Name == messageObject.SceneName))
            {
                sceneList.First(x => x.Name == messageObject.SceneName).Sources.Insert(0, newSceneItem);
            }
        }

        private void SceneItemTransformChanged_Event(SceneItemTransformChangedObsEvent messageObject)
        {
            if (!sceneList.Any(x => x.Name == messageObject.SceneName)) { return; }

            ObsScene existingScene = sceneList.First(x => x.Name == messageObject.SceneName);
            ObsSceneItem existingSceneItem = existingScene.Sources.FirstOrDefault(x => x.Name == messageObject.ItemName);
            if (existingSceneItem != default)
            {
                existingSceneItem.Transform = messageObject.Transform;
            }
        }

        private void SourceVolumeChanged_Event(SourceVolumeChangedObsEvent messageObject)
        {
            (obsSourceDictionary[messageObject.SourceName] as BaseType).Volume = messageObject.Volume;
        }

        private void SourceMuteStateChanged_Event(SourceMuteStateChangedObsEvent messageObject)
        {
            (obsSourceDictionary[messageObject.SourceName] as BaseType).Muted = messageObject.Muted;
        }

        private void SourceAudioSyncOffsetChanged_Event(SourceAudioSyncOffsetChangedObsEvent messageObject)
        {
            (obsSourceDictionary[messageObject.SourceName] as BaseType).SyncOffset = messageObject.SyncOffset;
        }

        private void SourceRenamed_Event(SourceRenamedObsEvent messageObject)
        {
            if (!obsSourceDictionary.ContainsKey(messageObject.PreviousName))
            {
                return;
            }
            obsSourceDictionary[messageObject.NewName] = obsSourceDictionary[messageObject.PreviousName];
            (obsSourceDictionary[messageObject.NewName] as BaseType).Name = messageObject.NewName;
            obsSourceDictionary.Remove(messageObject.PreviousName);
        }

        private void SceneItemVisibilityChanged_Event(SceneItemVisibilityChangedObsEvent messageObject)
        {
            if (!sceneList.Any(x => x.Name == messageObject.SceneName)) { return; }

            ObsScene scene = sceneList.First(x => x.Name == messageObject.SceneName);
            ObsSceneItem sceneItem = scene.Sources.FirstOrDefault(x => x.Name == messageObject.ItemName);
            if (sceneItem != default)
            {
                sceneItem.Render = messageObject.ItemVisible;
            }
        }

        private void SceneItemLockChanged_Event(SceneItemLockChangedObsEvent messageObject)
        {
            if (!sceneList.Any(x => x.Name == messageObject.SceneName)) { return; }

            ObsScene scene = sceneList.First(x => x.Name == messageObject.SceneName);
            ObsSceneItem sceneItem = scene.Sources.FirstOrDefault(x => x.Name == messageObject.ItemName);
            if (sceneItem != default)
            {
                sceneItem.Locked = messageObject.ItemLocked;
            }
        }

        private void SourceAudioMixersChanged_Event(SourceAudioMixersChangedObsEvent messageObject)
        {
            (obsSourceDictionary[messageObject.SourceName] as BaseType).Mixers = messageObject.Mixers;
            (obsSourceDictionary[messageObject.SourceName] as BaseType).HexMixersValue = messageObject.HexMixersValue;
        }

        private void SourceFilterAdded_Event(SourceFilterAddedObsEvent messageObject)
        {
            BaseType source = (BaseType)obsSourceDictionary[messageObject.SourceName];
            BaseFilter filter = (BaseFilter)messageObject.FilterSettingsObj;
            source.Filters.Add(filter);
        }

        private void SourceFilterRemoved_Event(SourceFilterRemovedObsEvent messageObject)
        {
            BaseType source = (BaseType)obsSourceDictionary[messageObject.SourceName];
            BaseFilter filter = source.Filters.FirstOrDefault(x => x.Name == messageObject.FilterName);
            if (filter != default)
            {
                source.Filters.Remove(filter);
            }
        }

        private void SourceFilterVisibilityChanged_Event(SourceFilterVisibilityChangedObsEvent messageObject)
        {
            BaseType source = (BaseType)obsSourceDictionary[messageObject.SourceName];
            BaseFilter filter = source.Filters.FirstOrDefault(x => x.Name == messageObject.FilterName);
            if (filter != default)
            {
                filter.Enabled = messageObject.FilterEnabled;
            }
        }

        private void SourceFiltersReordered_Event(SourceFiltersReorderedObsEvent messageObject)
        {
            BaseType source = (BaseType)obsSourceDictionary[messageObject.SourceName];
            List<string> collectionOrderList = messageObject.Filters.Select(x => x.Name).ToList();
            for (int i = 0; i < collectionOrderList.Count; i++)
            {
                BaseFilter filter = source.Filters.FirstOrDefault(x => x.Name == collectionOrderList[i]);
                if (filter != default)
                {
                    source.Filters.Move(source.Filters.IndexOf(filter), i);
                }
            }
        }

        #endregion

        #region obs-requests

        /// <summary>
        /// Sends an OBS Request without any request parameters.
        /// </summary>
        /// <param name="requestType">The request type constant.</param>
        /// <returns>The Guid for the request.</returns>
        private async ValueTask<Guid> Obs_Get(ObsRequestType requestType)
        {
            return await webSocket.ObsSend(ObsWsRequest.GetInstanceOfType(requestType)).ConfigureAwait(true);
        }

        /// <summary>
        /// Sends an OBS Request with a single request parameter.
        /// </summary>
        /// <param name="requestType">The request type constant.</param>
        /// <param name="name">The primary request parameter - see method for request type assumptions.</param>
        /// <returns>The Guid for the request.</returns>
        private async ValueTask<Guid> Obs_Get(ObsRequestType requestType, string name)
        {
            object request = ObsWsRequest.GetInstanceOfType(requestType);
            switch (requestType)
            {
                case ObsRequestType.GetSceneItemProperties:
                    (request as GetSceneItemPropertiesRequest).Item = name;
                    break;
                case ObsRequestType.GetSourceSettings:
                    (request as GetSourceSettingsRequest).SourceName = name;
                    break;
                case ObsRequestType.GetSourceFilters:
                    (request as GetSourceFiltersRequest).SourceName = name;
                    break;
                case ObsRequestType.GetAudioMonitorType:
                    (request as GetAudioMonitorTypeRequest).SourceName = name;
                    break;
                default:
                    return Guid.Empty;
            }
            return await webSocket.ObsSend(request).ConfigureAwait(true);
        }

        /// <summary>
        /// Sends an OBS Request with two request parameters.
        /// </summary>
        /// <param name="requestType">The request type constant.</param>
        /// <param name="name">The primary request parameter - see method for request type assumptions.</param>
        /// <param name="name2">The secondary request parameter - see method for request type assumptions.</param>
        /// <returns>The Guid for the request.</returns>
        private async ValueTask<Guid> Obs_Get(ObsRequestType requestType, string name, string name2)
        {
            object request = ObsWsRequest.GetInstanceOfType(requestType);
            switch (requestType)
            {
                case ObsRequestType.GetSceneItemProperties:
                    (request as GetSceneItemPropertiesRequest).Item = name;
                    (request as GetSceneItemPropertiesRequest).SceneName = name2;
                    break;
                default:
                    return Guid.Empty;
            }
            return await webSocket.ObsSend(request).ConfigureAwait(true);
        }

        #endregion

        #endregion

        #region User Interface

        private void UpdateUIConnectStatus(string countdownText, Brush brush1, Brush brush2)
        {
            _ = Task.Run(
                () => UpdateConnectStatus(countdownText, brush1, brush2)
                );
        }

        private async Task UpdateConnectStatus(string countdownText, Brush brush1, Brush brush2)
        {
            _Context.Send(
                _ => tbStatus.Text = connectionError,
                null);
            if (countdownText != null)
            {
                _Context.Send(
                    _ => tbReconnectCountdown.Text = countdownText,
                    null);
            }
            if (brush1 != null)
            {
                _Context.Send(
                    _ => sbCircleStatus.Fill = brush1,
                    null);
            }
            if (brush2 != null)
            {
                await Task.Delay(250, pulseCancellationToken.Token).ConfigureAwait(false);
                _Context.Send(
                    _ => sbCircleStatus.Fill = brush2,
                    null);
            }
        }

        private Task UpdateSceneInformation()
        {
            _Context.Send(
                x => DataContext = currentScene,
                null);
            return Task.CompletedTask;
        }

        private Task UpdateTransitionMessage(string transitionMessage)
        {
            _Context.Send(
                x => tbTransitioning.Text = transitionMessage,
                null);
            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _ReconnectCountdownTimer.Dispose();
                    pulseCancellationToken.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                silentAudioEvent?.Stop();
                silentAudioEvent?.Dispose();
                webSocket.Dispose();
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~AudioCheck()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
