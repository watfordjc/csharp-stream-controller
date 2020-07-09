using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudioWrapperLibrary;
using OBSWebSocketLibrary;
using StreamController.SharedModels;
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
using WebSocketLibrary;

namespace StreamController
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
        private readonly ObservableCollection<OBSWebSocketLibrary.Models.TypeDefs.Scene> sceneList = new ObservableCollection<OBSWebSocketLibrary.Models.TypeDefs.Scene>();
        private OBSWebSocketLibrary.Models.TypeDefs.Scene currentScene;
        private OBSWebSocketLibrary.Models.RequestReplies.GetSourceTypesList sourceTypes;
        private readonly Dictionary<string, object> obsSourceDictionary = new Dictionary<string, object>();
        private readonly Dictionary<int, OBSWebSocketLibrary.Models.TypeDefs.Scene> obsSceneItemSceneDictionary = new Dictionary<int, OBSWebSocketLibrary.Models.TypeDefs.Scene>();
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

        private Task StartPlaySilence(AudioInterface audioInterface)
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

        private static int GetWaveOutDeviceNumber(AudioInterface audioInterface)
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
                await Obs_Get(OBSWebSocketLibrary.Data.RequestType.GetSourceTypesList).ConfigureAwait(true);
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

        private void WebSocket_Error_ContextSwitch(object sender, WebSocketLibrary.Models.ErrorMessage e)
        {
            _Context.Send(
                x => WebSocket_Error(e),
                null);
        }

        private void WebSocket_Error(WebSocketLibrary.Models.ErrorMessage e)
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

        private void WebSocket_Event_ContextSwitch(object sender, OBSWebSocketLibrary.Models.TypeDefs.ObsEvent eventObject)
        {
            _Context.Send(
                x => WebSocket_Event(eventObject),
                null);
        }

        private async void WebSocket_Event(OBSWebSocketLibrary.Models.TypeDefs.ObsEvent eventObject)
        {
            switch (eventObject.EventType)
            {
                case OBSWebSocketLibrary.Data.EventType.Heartbeat:
                    Heartbeat_Event((OBSWebSocketLibrary.Models.Events.Heartbeat)eventObject.MessageObject);
                    break;
                case OBSWebSocketLibrary.Data.EventType.SwitchScenes:
                    SwitchScenes_Event((OBSWebSocketLibrary.Models.Events.SwitchScenes)eventObject.MessageObject);
                    break;
                case OBSWebSocketLibrary.Data.EventType.ScenesChanged:
                    break;
                case OBSWebSocketLibrary.Data.EventType.TransitionBegin:
                    string nextScene = ((OBSWebSocketLibrary.Models.Events.TransitionBegin)eventObject.MessageObject).ToScene;
                    await UpdateTransitionMessage($"\u27a1\ufe0f {nextScene}\u2026").ConfigureAwait(true);
                    break;
                case OBSWebSocketLibrary.Data.EventType.TransitionEnd:
                case OBSWebSocketLibrary.Data.EventType.TransitionVideoEnd:
                    await UpdateTransitionMessage(String.Empty).ConfigureAwait(true);
                    break;
                case OBSWebSocketLibrary.Data.EventType.SourceOrderChanged:
                    SourceOrderChanged_Event((OBSWebSocketLibrary.Models.Events.SourceOrderChanged)eventObject.MessageObject);
                    break;
                case OBSWebSocketLibrary.Data.EventType.SourceCreated:
                    SourceCreated_Event((OBSWebSocketLibrary.Models.Events.SourceCreated)eventObject.MessageObject);
                    break;
                case OBSWebSocketLibrary.Data.EventType.SceneItemAdded:
                    SceneItemAdded_Event((OBSWebSocketLibrary.Models.Events.SceneItemAdded)eventObject.MessageObject);
                    break;
                case OBSWebSocketLibrary.Data.EventType.SceneItemRemoved:
                    SceneItemRemoved_Event((OBSWebSocketLibrary.Models.Events.SceneItemRemoved)eventObject.MessageObject);
                    break;
                case OBSWebSocketLibrary.Data.EventType.SourceDestroyed:
                    SourceDestroyed_Event((OBSWebSocketLibrary.Models.Events.SourceDestroyed)eventObject.MessageObject);
                    break;
                case OBSWebSocketLibrary.Data.EventType.SceneItemTransformChanged:
                    SceneItemTransformChanged_Event((OBSWebSocketLibrary.Models.Events.SceneItemTransformChanged)eventObject.MessageObject);
                    break;
                case OBSWebSocketLibrary.Data.EventType.SourceVolumeChanged:
                    SourceVolumeChanged_Event((OBSWebSocketLibrary.Models.Events.SourceVolumeChanged)eventObject.MessageObject);
                    break;
                case OBSWebSocketLibrary.Data.EventType.SourceMuteStateChanged:
                    SourceMuteStateChanged_Event((OBSWebSocketLibrary.Models.Events.SourceMuteStateChanged)eventObject.MessageObject);
                    break;
                case OBSWebSocketLibrary.Data.EventType.SourceAudioSyncOffsetChanged:
                    SourceAudioSyncOffsetChanged_Event((OBSWebSocketLibrary.Models.Events.SourceAudioSyncOffsetChanged)eventObject.MessageObject);
                    break;
                case OBSWebSocketLibrary.Data.EventType.SourceRenamed:
                    SourceRenamed_Event((OBSWebSocketLibrary.Models.Events.SourceRenamed)eventObject.MessageObject);
                    break;
                case OBSWebSocketLibrary.Data.EventType.SceneItemVisibilityChanged:
                    SceneItemVisibilityChanged_Event((OBSWebSocketLibrary.Models.Events.SceneItemVisibilityChanged)eventObject.MessageObject);
                    break;
                case OBSWebSocketLibrary.Data.EventType.SceneItemLockChanged:
                    SceneItemLockChanged_Event((OBSWebSocketLibrary.Models.Events.SceneItemLockChanged)eventObject.MessageObject);
                    break;
                case OBSWebSocketLibrary.Data.EventType.SceneCollectionChanged:
                    await Obs_Get(OBSWebSocketLibrary.Data.RequestType.GetSourcesList).ConfigureAwait(true);
                    break;
                case OBSWebSocketLibrary.Data.EventType.SourceAudioMixersChanged:
                    SourceAudioMixersChanged_Event((OBSWebSocketLibrary.Models.Events.SourceAudioMixersChanged)eventObject.MessageObject);
                    break;
                case OBSWebSocketLibrary.Data.EventType.SourceFilterAdded:
                    SourceFilterAdded_Event((OBSWebSocketLibrary.Models.Events.SourceFilterAdded)eventObject.MessageObject);
                    break;
                case OBSWebSocketLibrary.Data.EventType.SourceFilterRemoved:
                    SourceFilterRemoved_Event((OBSWebSocketLibrary.Models.Events.SourceFilterRemoved)eventObject.MessageObject);
                    break;
                case OBSWebSocketLibrary.Data.EventType.SourceFilterVisibilityChanged:
                    SourceFilterVisibilityChanged_Event((OBSWebSocketLibrary.Models.Events.SourceFilterVisibilityChanged)eventObject.MessageObject);
                    break;
                case OBSWebSocketLibrary.Data.EventType.SourceFiltersReordered:
                    SourceFiltersReordered_Event((OBSWebSocketLibrary.Models.Events.SourceFiltersReordered)eventObject.MessageObject);
                    break;
                case OBSWebSocketLibrary.Data.EventType.SceneItemSelected:
                    OBSWebSocketLibrary.Models.Events.SceneItemSelected itemSelectedEvent = eventObject.MessageObject as OBSWebSocketLibrary.Models.Events.SceneItemSelected;
                    Trace.WriteLine($"Item {itemSelectedEvent.ItemName} has been selected in scene {itemSelectedEvent.SceneName}");
                    break;
                case OBSWebSocketLibrary.Data.EventType.SceneItemDeselected:
                    OBSWebSocketLibrary.Models.Events.SceneItemDeselected itemDeselectedEvent = eventObject.MessageObject as OBSWebSocketLibrary.Models.Events.SceneItemDeselected;
                    Trace.WriteLine($"Item {itemDeselectedEvent.ItemName} has been deselected in scene {itemDeselectedEvent.SceneName}");
                    break;
            }
        }

        private async Task PopulateSceneItemSources(IList<OBSWebSocketLibrary.Models.TypeDefs.SceneItem> sceneItems, OBSWebSocketLibrary.Models.TypeDefs.Scene scene)
        {
            foreach (OBSWebSocketLibrary.Models.TypeDefs.SceneItem sceneItem in sceneItems)
            {
                sceneItem.Source = (OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType)obsSourceDictionary.GetValueOrDefault(sceneItem.Name);
                if (sceneItem.GroupChildren != null)
                {
                    await PopulateSceneItemSources(sceneItem.GroupChildren, scene).ConfigureAwait(true);
                }
                obsSceneItemSceneDictionary[sceneItem.Id] = scene;
                await Obs_Get(OBSWebSocketLibrary.Data.RequestType.GetSceneItemProperties, sceneItem.Name, scene.Name).ConfigureAwait(true);
                await Obs_Get(OBSWebSocketLibrary.Data.RequestType.GetAudioMonitorType, sceneItem.Name).ConfigureAwait(true);
            }
        }

        private void Websocket_Reply_ContextSwitch(object sender, OBSWebSocketLibrary.Models.TypeDefs.ObsReply replyObject)
        {
            _Context.Send(
                x => Websocket_Reply(replyObject),
                null);
        }

        private async void Websocket_Reply(OBSWebSocketLibrary.Models.TypeDefs.ObsReply replyObject)
        {
            switch (replyObject.RequestType)
            {
                case OBSWebSocketLibrary.Data.RequestType.GetCurrentScene:
                    currentScene = replyObject.MessageObject as OBSWebSocketLibrary.Models.TypeDefs.Scene;
                    await PopulateSceneItemSources(currentScene.Sources, currentScene).ConfigureAwait(true);
                    await UpdateSceneInformation().ConfigureAwait(true);
                    break;
                case OBSWebSocketLibrary.Data.RequestType.GetSceneList:
                    ReadOnlyMemory<char> currentSceneName = (replyObject.MessageObject as OBSWebSocketLibrary.Models.RequestReplies.GetSceneList).CurrentScene.AsMemory();
                    sceneList.Clear();
                    foreach (OBSWebSocketLibrary.Models.TypeDefs.Scene scene in (replyObject.MessageObject as OBSWebSocketLibrary.Models.RequestReplies.GetSceneList).Scenes)
                    {
                        sceneList.Add(scene);
                        await PopulateSceneItemSources(scene.Sources, scene).ConfigureAwait(true);
                    }
                    currentScene = sceneList.First(x => x.Name == currentSceneName.ToString());
                    await UpdateSceneInformation().ConfigureAwait(true);
                    break;
                case OBSWebSocketLibrary.Data.RequestType.GetSourceTypesList:
                    sourceTypes = (OBSWebSocketLibrary.Models.RequestReplies.GetSourceTypesList)replyObject.MessageObject;
                    await Obs_Get(OBSWebSocketLibrary.Data.RequestType.GetSourcesList).ConfigureAwait(true);
                    break;
                case OBSWebSocketLibrary.Data.RequestType.GetSourcesList:
                    OBSWebSocketLibrary.Models.RequestReplies.GetSourcesList sourcesList = (OBSWebSocketLibrary.Models.RequestReplies.GetSourcesList)replyObject.MessageObject;
                    foreach (OBSWebSocketLibrary.Models.TypeDefs.ObsReplySource source in sourcesList.Sources)
                    {
                        await Obs_Get(OBSWebSocketLibrary.Data.RequestType.GetSourceSettings, source.Name).ConfigureAwait(true);
                    }
                    await GetDeviceIdsForSources().ConfigureAwait(true);
                    await Obs_Get(OBSWebSocketLibrary.Data.RequestType.GetSceneList).ConfigureAwait(true);
                    await Obs_Get(OBSWebSocketLibrary.Data.RequestType.GetTransitionList).ConfigureAwait(true);
                    break;
                case OBSWebSocketLibrary.Data.RequestType.GetSourceSettings:
                    OBSWebSocketLibrary.Models.RequestReplies.GetSourceSettings sourceSettings = (OBSWebSocketLibrary.Models.RequestReplies.GetSourceSettings)replyObject.MessageObject;
                    OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType newSource = (OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType)sourceSettings.SourceSettingsObj;
                    newSource.Type = sourceTypes.Types.First(x => x.TypeId == sourceSettings.SourceType);
                    obsSourceDictionary[sourceSettings.SourceName] = newSource;
                    await Obs_Get(OBSWebSocketLibrary.Data.RequestType.GetSourceFilters, sourceSettings.SourceName).ConfigureAwait(true);
                    break;
                case OBSWebSocketLibrary.Data.RequestType.GetSourceFilters:
                    OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType sourceToModify = obsSourceDictionary[(replyObject.RequestMetadata.OriginalRequestData as OBSWebSocketLibrary.Models.Requests.GetSourceFilters).SourceName] as OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType;
                    foreach (OBSWebSocketLibrary.Models.TypeDefs.ObsReplyFilter filter in (replyObject.MessageObject as OBSWebSocketLibrary.Models.RequestReplies.GetSourceFilters).Filters)
                    {
                        sourceToModify.Filters.Add(filter);
                    }
                    break;
                case OBSWebSocketLibrary.Data.RequestType.GetTransitionList:
                    OBSWebSocketLibrary.Models.RequestReplies.GetTransitionList transitionList = (OBSWebSocketLibrary.Models.RequestReplies.GetTransitionList)replyObject.MessageObject;
                    foreach (OBSWebSocketLibrary.Models.TypeDefs.ObsTransitionName transition in transitionList.Transitions)
                    {
                        //  Trace.WriteLine($"{transition.Name}");
                    }
                    break;
                case OBSWebSocketLibrary.Data.RequestType.GetSceneItemProperties:
                    OBSWebSocketLibrary.Models.RequestReplies.GetSceneItemProperties itemProps = (OBSWebSocketLibrary.Models.RequestReplies.GetSceneItemProperties)replyObject.MessageObject;
                    OBSWebSocketLibrary.Models.TypeDefs.SceneItem existingSceneItem = GetSceneItemFromSceneItemId(itemProps.ItemId, null);
                    existingSceneItem.Transform = itemProps;
                    break;
                case OBSWebSocketLibrary.Data.RequestType.GetAudioMonitorType:
                    OBSWebSocketLibrary.Models.Requests.GetAudioMonitorType requestSent = replyObject.RequestMetadata.OriginalRequestData as OBSWebSocketLibrary.Models.Requests.GetAudioMonitorType;
                    (obsSourceDictionary[requestSent.SourceName] as OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType).MonitorType = (replyObject.MessageObject as OBSWebSocketLibrary.Models.RequestReplies.GetAudioMonitorType).MonitorType;
                    break;
                default:
                    break;
            }
        }

        private OBSWebSocketLibrary.Models.TypeDefs.SceneItem GetSceneItemFromSceneItemId(int sceneItemId, ObservableCollection<OBSWebSocketLibrary.Models.TypeDefs.SceneItem> sceneItems)
        {
            /*
             * 
             * DANGER: Untested Recursion
             * 
             */
            OBSWebSocketLibrary.Models.TypeDefs.SceneItem returnValue = null;
            if (sceneItems == null)
            {
                OBSWebSocketLibrary.Models.TypeDefs.Scene firstScene = obsSceneItemSceneDictionary[sceneItemId];
                returnValue = GetSceneItemFromSceneItemId(sceneItemId, firstScene.Sources);
            }
            if (returnValue == null && sceneItems != null)
            {
                returnValue = sceneItems.Where(x => x.Id == sceneItemId).First();
            }

            if (returnValue == null)
            {
                foreach (OBSWebSocketLibrary.Models.TypeDefs.SceneItem nextSceneItem in sceneItems.Where(x => x.GroupChildren.Count > 0).ToArray())
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
            while (webSocket.WaitingForReplyForType(OBSWebSocketLibrary.Data.RequestType.GetSourceSettings))
            {
                await Task.Delay(250).ConfigureAwait(false);
            }

            foreach (OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType source in obsSourceDictionary.Values)
            {
                switch (OBSWebSocketLibrary.Data.ObsTypes.ObsTypeNameDictionary[source.Type.TypeId])
                {
                    case OBSWebSocketLibrary.Data.SourceType.WasapiOutputCapture:
                    case OBSWebSocketLibrary.Data.SourceType.WasapiInputCapture:
                    case OBSWebSocketLibrary.Data.SourceType.DShowInput:
                        break;
                    default:
                        continue;
                }
                OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.DependencyProperties dependencies = source.Dependencies;
                if (source.Type.TypeId == OBSWebSocketLibrary.Data.ObsTypes.ObsTypeNameDictionary.First(x => x.Value == OBSWebSocketLibrary.Data.SourceType.DShowInput).Key)
                {
                    ReadOnlyMemory<char> deviceName = ((OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.DShowInput)source).AudioDeviceId.AsMemory();
                    dependencies.AudioDeviceId = AudioInterfaceCollection.GetAudioInterfaceByName(deviceName[0..^1].ToString()).ID;
                }
                dependencies.AudioInterface = AudioInterfaceCollection.GetAudioInterfaceById(dependencies.AudioDeviceId);
                //Trace.WriteLine($"{sourceReply.SourceName} -> {sourceReply.SourceType} -> device_id: {audioInterface?.ID} AKA {audioInterface?.FriendlyName}");
                // WASAPI and DirectShow source types should reference an audio device
                if (!dependencies.HasAudioInterface)
                {
                    dependencies.DependencyProblem = true;
                    continue;
                }
            }
        }

        #region obs-events

        private void Heartbeat_Event(OBSWebSocketLibrary.Models.Events.Heartbeat messageObject)
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

        private void SwitchScenes_Event(OBSWebSocketLibrary.Models.Events.SwitchScenes messageObject)
        {
            if (sceneList.Any(x => x.Name == messageObject.SceneName))
            {
                currentScene = sceneList.First(x => x.Name == messageObject.SceneName);
                UpdateSceneInformation();
            }
        }

        private void SourceOrderChanged_Event(OBSWebSocketLibrary.Models.Events.SourceOrderChanged messageObject)
        {
            if (messageObject.SceneName != currentScene.Name)
            {
                return;
            }
            List<int> collectionOrderList = messageObject.SceneItems.Select(x => x.ItemId).ToList();
            ListCollectionView listCollection = (ListCollectionView)CollectionViewSource.GetDefaultView(lbSourceList.ItemsSource);
            listCollection.CustomSort = new Utils.OrderSceneItemsByListOfIds(collectionOrderList);
        }

        private void SceneItemRemoved_Event(OBSWebSocketLibrary.Models.Events.SceneItemRemoved messageObject)
        {
            OBSWebSocketLibrary.Models.TypeDefs.SceneItem sceneItemToRemove = sceneList.First(x => x.Name == messageObject.SceneName).Sources.First(x => x.Name == messageObject.ItemName);
            sceneList.First(x => x.Name == messageObject.SceneName).Sources.Remove(sceneItemToRemove);
        }

        private void SourceCreated_Event(OBSWebSocketLibrary.Models.Events.SourceCreated messageObject)
        {
            OBSWebSocketLibrary.Data.SourceType sourceType = OBSWebSocketLibrary.Data.ObsTypes.ObsTypeNameDictionary[messageObject.SourceKind];
            object createdSource = OBSWebSocketLibrary.Data.SourceTypeSettings.GetInstanceOfType(sourceType);
            (createdSource as OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType).Type = sourceTypes.Types.First(x => x.TypeId == messageObject.SourceKind);
            obsSourceDictionary[messageObject.SourceName] = createdSource;
        }

        private void SourceDestroyed_Event(OBSWebSocketLibrary.Models.Events.SourceDestroyed messageObject)
        {
            obsSourceDictionary.Remove(messageObject.SourceName);
        }

        private void SceneItemAdded_Event(OBSWebSocketLibrary.Models.Events.SceneItemAdded messageObject)
        {
            if (!obsSourceDictionary.TryGetValue(messageObject.ItemName, out object source))
            {
                return;
            }
            OBSWebSocketLibrary.Models.TypeDefs.SceneItem newSceneItem = new OBSWebSocketLibrary.Models.TypeDefs.SceneItem()
            {
                Name = messageObject.ItemName,
                Id = messageObject.ItemId,
                Source = (OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType)source
            };
            newSceneItem.Type = newSceneItem.Source.Type.TypeId;
            if (sceneList.Any(x => x.Name == messageObject.SceneName))
            {
                sceneList.First(x => x.Name == messageObject.SceneName).Sources.Insert(0, newSceneItem);
            }
        }

        private void SceneItemTransformChanged_Event(OBSWebSocketLibrary.Models.Events.SceneItemTransformChanged messageObject)
        {
            if (sceneList.Any(x => x.Name == messageObject.SceneName))
            {
                OBSWebSocketLibrary.Models.TypeDefs.SceneItem existingScene = sceneList.First(x => x.Name == messageObject.SceneName).Sources.First(x => x.Name == messageObject.ItemName);
                existingScene.Transform = messageObject.Transform;
            }
        }

        private void SourceVolumeChanged_Event(OBSWebSocketLibrary.Models.Events.SourceVolumeChanged messageObject)
        {
            (obsSourceDictionary[messageObject.SourceName] as OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType).Volume = messageObject.Volume;
        }

        private void SourceMuteStateChanged_Event(OBSWebSocketLibrary.Models.Events.SourceMuteStateChanged messageObject)
        {
            (obsSourceDictionary[messageObject.SourceName] as OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType).Muted = messageObject.Muted;
        }

        private void SourceAudioSyncOffsetChanged_Event(OBSWebSocketLibrary.Models.Events.SourceAudioSyncOffsetChanged messageObject)
        {
            (obsSourceDictionary[messageObject.SourceName] as OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType).SyncOffset = messageObject.SyncOffset;
        }

        private void SourceRenamed_Event(OBSWebSocketLibrary.Models.Events.SourceRenamed messageObject)
        {
            if (!obsSourceDictionary.ContainsKey(messageObject.PreviousName))
            {
                return;
            }
            obsSourceDictionary[messageObject.NewName] = obsSourceDictionary[messageObject.PreviousName];
            (obsSourceDictionary[messageObject.NewName] as OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType).Name = messageObject.NewName;
            obsSourceDictionary.Remove(messageObject.PreviousName);
        }

        private void SceneItemVisibilityChanged_Event(OBSWebSocketLibrary.Models.Events.SceneItemVisibilityChanged messageObject)
        {
            OBSWebSocketLibrary.Models.TypeDefs.SceneItem existingScene = sceneList.First(x => x.Name == messageObject.SceneName).Sources.First(x => x.Name == messageObject.ItemName);
            existingScene.Render = messageObject.ItemVisible;
        }

        private void SceneItemLockChanged_Event(OBSWebSocketLibrary.Models.Events.SceneItemLockChanged messageObject)
        {
            OBSWebSocketLibrary.Models.TypeDefs.SceneItem existingScene = sceneList.First(x => x.Name == messageObject.SceneName).Sources.First(x => x.Name == messageObject.ItemName);
            existingScene.Locked = messageObject.ItemLocked;
        }

        private void SourceAudioMixersChanged_Event(OBSWebSocketLibrary.Models.Events.SourceAudioMixersChanged messageObject)
        {
            (obsSourceDictionary[messageObject.SourceName] as OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType).Mixers = messageObject.Mixers;
            (obsSourceDictionary[messageObject.SourceName] as OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType).HexMixersValue = messageObject.HexMixersValue;
        }

        private void SourceFilterAdded_Event(OBSWebSocketLibrary.Models.Events.SourceFilterAdded messageObject)
        {
            OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType source = (OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType)obsSourceDictionary[messageObject.SourceName];
            OBSWebSocketLibrary.Models.TypeDefs.FilterTypes.BaseFilter filter = (OBSWebSocketLibrary.Models.TypeDefs.FilterTypes.BaseFilter)messageObject.FilterSettingsObj;
            source.Filters.Add(filter);
        }

        private void SourceFilterRemoved_Event(OBSWebSocketLibrary.Models.Events.SourceFilterRemoved messageObject)
        {
            OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType source = (OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType)obsSourceDictionary[messageObject.SourceName];
            OBSWebSocketLibrary.Models.TypeDefs.FilterTypes.BaseFilter filter = source.Filters.FirstOrDefault(x => x.Name == messageObject.FilterName);
            if (filter != default)
            {
                source.Filters.Remove(filter);
            }
        }

        private void SourceFilterVisibilityChanged_Event(OBSWebSocketLibrary.Models.Events.SourceFilterVisibilityChanged messageObject)
        {
            OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType source = (OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType)obsSourceDictionary[messageObject.SourceName];
            OBSWebSocketLibrary.Models.TypeDefs.FilterTypes.BaseFilter filter = source.Filters.First(x => x.Name == messageObject.FilterName);
            filter.Enabled = messageObject.FilterEnabled;
        }

        private void SourceFiltersReordered_Event(OBSWebSocketLibrary.Models.Events.SourceFiltersReordered messageObject)
        {
            OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType source = (OBSWebSocketLibrary.Models.TypeDefs.SourceTypes.BaseType)obsSourceDictionary[messageObject.SourceName];
            List<string> collectionOrderList = messageObject.Filters.Select(x => x.Name).ToList();
            for (int i = 0; i < collectionOrderList.Count; i++)
            {
                OBSWebSocketLibrary.Models.TypeDefs.FilterTypes.BaseFilter filter = source.Filters.First(x => x.Name == collectionOrderList[i]);
                source.Filters.Move(source.Filters.IndexOf(filter), i);
            }
        }

        #endregion

        #region obs-requests

        /// <summary>
        /// Sends an OBS Request without any request parameters.
        /// </summary>
        /// <param name="requestType">The request type constant.</param>
        /// <returns>The Guid for the request.</returns>
        private async ValueTask<Guid> Obs_Get(OBSWebSocketLibrary.Data.RequestType requestType)
        {
            return await webSocket.ObsSend(OBSWebSocketLibrary.Data.Request.GetInstanceOfType(requestType)).ConfigureAwait(true);
        }

        /// <summary>
        /// Sends an OBS Request with a single request parameter.
        /// </summary>
        /// <param name="requestType">The request type constant.</param>
        /// <param name="name">The primary request parameter - see method for request type assumptions.</param>
        /// <returns>The Guid for the request.</returns>
        private async ValueTask<Guid> Obs_Get(OBSWebSocketLibrary.Data.RequestType requestType, string name)
        {
            object request = OBSWebSocketLibrary.Data.Request.GetInstanceOfType(requestType);
            switch (requestType)
            {
                case OBSWebSocketLibrary.Data.RequestType.GetSceneItemProperties:
                    (request as OBSWebSocketLibrary.Models.Requests.GetSceneItemProperties).Item = name;
                    break;
                case OBSWebSocketLibrary.Data.RequestType.GetSourceSettings:
                    (request as OBSWebSocketLibrary.Models.Requests.GetSourceSettings).SourceName = name;
                    break;
                case OBSWebSocketLibrary.Data.RequestType.GetSourceFilters:
                    (request as OBSWebSocketLibrary.Models.Requests.GetSourceFilters).SourceName = name;
                    break;
                case OBSWebSocketLibrary.Data.RequestType.GetAudioMonitorType:
                    (request as OBSWebSocketLibrary.Models.Requests.GetAudioMonitorType).SourceName = name;
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
        private async ValueTask<Guid> Obs_Get(OBSWebSocketLibrary.Data.RequestType requestType, string name, string name2)
        {
            object request = OBSWebSocketLibrary.Data.Request.GetInstanceOfType(requestType);
            switch (requestType)
            {
                case OBSWebSocketLibrary.Data.RequestType.GetSceneItemProperties:
                    (request as OBSWebSocketLibrary.Models.Requests.GetSceneItemProperties).Item = name;
                    (request as OBSWebSocketLibrary.Models.Requests.GetSceneItemProperties).SceneName = name2;
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
