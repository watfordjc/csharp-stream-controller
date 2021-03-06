﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using uk.JohnCook.dotnet.NAudioWrapperLibrary;
using uk.JohnCook.dotnet.OBSWebSocketLibrary;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.Data;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsEvents;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequestReplies;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequests;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.TypeDefs;
using uk.JohnCook.dotnet.StreamController.Models;
using uk.JohnCook.dotnet.WebSocketLibrary;

namespace uk.JohnCook.dotnet.StreamController
{
    public class ObsWebsocketConnection : IDisposable, INotifyPropertyChanged
    {
        #region Instantiation

        #region Properties and Variables

        private readonly SynchronizationContext _Context;
        private readonly AudioWorkarounds audioWorkarounds = new AudioWorkarounds();
        private readonly ChronoTimer chronoTimer = ChronoTimer.Instance;
        public static ObsWebsocketConnection Instance { get { return lazySingleton.Value; } }
        public ObsWsClient Client { get; private set; }
        public string ConnectionStatus { get; private set; } = Properties.Resources.text_disconnected;
        public string ConnectionError { get; private set; } = Properties.Resources.text_disconnected;
        public string ExtendedConnectionError { get; private set; } = String.Empty;
        public Brush ConnectionStatusBrush { get; private set; } = Brushes.Gray;

        public ObservableCollection<ObsScene> SceneList { get; } = new ObservableCollection<ObsScene>();
        public ObsScene CurrentScene { get; private set; }
        public List<int> SourceOrderList { get; } = new List<int>();
        public string NextScene { get; private set; } = String.Empty;
        public GetSourceTypesListReply SourceTypes { get; private set; }
        public Dictionary<string, object> ObsSourceDictionary { get; } = new Dictionary<string, object>();
        private Dictionary<int, ObsScene> ObsSceneItemSceneDictionary { get; } = new Dictionary<int, ObsScene>();

        private string TimezoneString { get; set; } = String.Empty;
        private string PreviousLocalClockWeatherAttrib1 { get; set; } = String.Empty;
        private string PreviousLocalClockWeatherAttrib2 { get; set; } = String.Empty;
        private int CurrentLocalClockWeatherRecord { get; set; }
        private bool FirstWeatherCycle { get; set; } = true;
        private readonly HttpClient httpClient = new HttpClient();
        private Models.WeatherData WeatherDataCollection { get; set; }

        public static readonly Brush PrimaryBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xE2, 0xC1, 0xEA));
        public static readonly Brush SecondaryBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xC5, 0xC0, 0xEB));
        private readonly System.Timers.Timer ReconnectCountdownTimer = new System.Timers.Timer(1000);
        private readonly SemaphoreSlim iconSemaphore = new SemaphoreSlim(1);
        private readonly TaskCompletionSource<bool> audioDevicesEnumerated = new TaskCompletionSource<bool>();
        private int ReconnectTimeRemaining;
        private bool disposedValue;

        private static readonly Lazy<ObsWebsocketConnection> lazySingleton = new Lazy<ObsWebsocketConnection>(
            () => new ObsWebsocketConnection()
            );

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private ObsWebsocketConnection()
        {
            _Context = SynchronizationContext.Current;
            Preferences.Default.PropertyChanged += Default_PropertyChanged;
            ReconnectCountdownTimer.Elapsed += ReconnectCountdownTimer_Elapsed;
            AudioInterfaceCollection.Instance.CollectionEnumerated += AudioDevicesEnumerated;
            if (AudioInterfaceCollection.Instance.DevicesAreEnumerated)
            {
                AudioDevicesEnumerated(this, EventArgs.Empty);
            }
            UpdateTimezoneString();
            ChronoTimer.Instance.MinuteChanged += FetchWeatherData;
            FetchWeatherData(this, DateTime.UtcNow);
        }

        private void AudioDevicesEnumerated(object sender, EventArgs e)
        {
            audioDevicesEnumerated.SetResult(true);
        }

        #endregion

        #region Preference changes

        private async void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(Preferences.Default.obs_settings_changed) || Preferences.Default.obs_settings_changed == false)
            {
                return;
            }

            if (ReconnectCountdownTimer.Enabled || Client.CanSend)
            {
                ReconnectCountdownTimer.Enabled = false;
            }

            UpdateTimezoneString();
            CreateClient();
            SystemTrayIcon.Instance.UpdateTrayIcon().ConfigureAwait(true).GetAwaiter();
            if (Client.AutoReconnect)
            {
                await Connect().ConfigureAwait(false);
            }
        }

        #endregion

        #region Set up connection

        /// <summary>
        /// Create a new ObsWsClient as Instance.Client
        /// </summary>
        public static void CreateClient()
        {
            if (Instance.Client != null)
            {
                Instance.Client.StateChange -= Instance.Client_StateChange;
                Instance.Client.ErrorState -= Instance.Client_Error;
                Instance.Client.OnObsEvent -= Instance.Client_Event;
                Instance.Client.OnObsReply -= Instance.Client_Reply;
                Instance.Client.Dispose();
            }
            Instance.Client = new ObsWsClient(new UriBuilder(
                                Preferences.Default.obs_uri_scheme,
                                Preferences.Default.obs_uri_host,
                                int.Parse(Preferences.Default.obs_uri_port, CultureInfo.InvariantCulture)
                                ).Uri)
            {
                PasswordPreference = Preferences.Default.obs_password,
                AutoReconnect = Preferences.Default.obs_auto_reconnect
            };
            Instance.Client.SetExponentialBackoff(Preferences.Default.obs_reconnect_min_seconds, Preferences.Default.obs_reconnect_max_minutes);
            Instance.Client.StateChange += Instance.Client_StateChange;
            Instance.Client.ErrorState += Instance.Client_Error;
            Instance.Client.OnObsEvent += Instance.Client_Event;
            Instance.Client.OnObsReply += Instance.Client_Reply;
        }

        /// <summary>
        /// Establish an obs-websocket connection
        /// </summary>
        public async Task Connect()
        {
            if (Client == null)
            {
                CreateClient();
            }
            if (Client.State == WebSocketState.Open)
            {
                return;
            }
            Client.AutoReconnect = Preferences.Default.obs_auto_reconnect;
            Client.SetExponentialBackoff(Preferences.Default.obs_reconnect_min_seconds, Preferences.Default.obs_reconnect_max_minutes);
            if (Client.AutoReconnect)
            {
                await Client.AutoReconnectConnectAsync().ConfigureAwait(true);
            }
            else
            {
                await Client.ConnectAsync().ConfigureAwait(true);
            }
        }

        public async Task Disconnect()
        {
            if (Client == null)
            {
                return;
            }
            Client.AutoReconnect = false;
            ReconnectCountdownTimer.Stop();
            await Client.DisconnectAsync(true).ConfigureAwait(false);
            ConnectionStatus = Properties.Resources.text_disconnected;
            NotifyPropertyChanged(nameof(ConnectionStatus));
            ConnectionError = Properties.Resources.window_audio_check_successfully_disconnected;
            NotifyPropertyChanged(nameof(ConnectionError));
            ExtendedConnectionError = String.Empty;
            NotifyPropertyChanged(nameof(ExtendedConnectionError));
        }

        public async Task Reconnect()
        {
            await Disconnect().ConfigureAwait(false);
            await Connect().ConfigureAwait(false);
        }

        /// <summary>
        /// React to Instance.ReconnectCountdownTimer Elapsed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReconnectCountdownTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ReconnectTimeRemaining--;
            if (ReconnectTimeRemaining > 0)
            {
                ConnectionStatus = String.Format(CultureInfo.CurrentCulture, Properties.Resources.window_audio_check_reconnect_delay_format, TimeSpan.FromSeconds(ReconnectTimeRemaining).ToString("c", CultureInfo.CurrentCulture));
                NotifyPropertyChanged(nameof(ConnectionStatus));
            }
        }

        /// <summary>
        /// React to websocket connection state changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="newState">New websocket connection state</param>
        private void Client_StateChange(object sender, WebSocketState newState)
        {
            if (newState == WebSocketState.Open)
            {
                ConnectionError = Properties.Resources.text_aok;
                NotifyPropertyChanged(nameof(ConnectionError));
                ExtendedConnectionError = String.Empty;
                NotifyPropertyChanged(nameof(ExtendedConnectionError));
                ReconnectCountdownTimer.Stop();
                ConnectionStatus = Properties.Resources.text_connected;
                NotifyPropertyChanged(nameof(ConnectionStatus));
                _ = ChangeStatusColor(newState).ConfigureAwait(false);
                ResetScenes();
            }
            else if (newState != WebSocketState.Connecting && Client.AutoReconnect)
            {
                ResetScenes();
                ReconnectCountdownTimer.Start();
                ConnectionStatus = String.Format(CultureInfo.CurrentCulture, Properties.Resources.window_audio_check_reconnect_delay_format, TimeSpan.FromSeconds(ReconnectTimeRemaining).ToString("c", CultureInfo.CurrentCulture));
                NotifyPropertyChanged(nameof(ConnectionStatus));
            }
            else if (newState == WebSocketState.Closed)
            {
                ResetScenes();
                ConnectionStatus = Properties.Resources.text_disconnected;
                NotifyPropertyChanged(nameof(ConnectionStatus));
                ConnectionError = Properties.Resources.window_audio_check_successfully_disconnected;
                NotifyPropertyChanged(nameof(ConnectionError));
            }
            else if (newState == WebSocketState.None)
            {
                _ = ChangeStatusColor(newState).ConfigureAwait(false);
            }
            else if (newState == WebSocketState.Connecting)
            {
                ResetScenes();
                ReconnectCountdownTimer.Stop();
                ConnectionStatus = Properties.Resources.window_audio_check_connecting;
                NotifyPropertyChanged(nameof(ConnectionStatus));
                if (Client.PasswordPreference != Preferences.Default.obs_password)
                {
                    Client.PasswordPreference = Preferences.Default.obs_password;
                }
                ConnectionError = Properties.Resources.window_audio_check_error_state_cleared;
                NotifyPropertyChanged(nameof(ConnectionError));
                ExtendedConnectionError = String.Empty;
                NotifyPropertyChanged(nameof(ExtendedConnectionError));
                _ = ChangeStatusColor(newState).ConfigureAwait(false);
            }
        }

        private void ResetScenes()
        {
            if (Client.CanSend)
            {
                FirstWeatherCycle = true;
                NotifyPropertyChanged(nameof(FirstWeatherCycle));
                CurrentLocalClockWeatherRecord = 0;
                PreviousLocalClockWeatherAttrib1 = String.Empty;
                NotifyPropertyChanged(nameof(PreviousLocalClockWeatherAttrib1));
                PreviousLocalClockWeatherAttrib2 = String.Empty;
                NotifyPropertyChanged(nameof(PreviousLocalClockWeatherAttrib2));
                return;
            }
            ChronoTimer.Instance.MinuteChanged -= UpdateLocalClock;
            ChronoTimer.Instance.MinuteChanged -= UpdateLocalWeather;
            ChronoTimer.Instance.SecondChanged -= UpdateLocalWeather;
            ChronoTimer.Instance.MinuteChanged -= SlideShowNextSlide;
            ChronoTimer.Instance.SecondChanged -= SlideShowNextSlide;
            _Context.Send(
                x => SceneList.Clear(),
                null);
            NotifyPropertyChanged(nameof(SceneList));
            CurrentScene = null;
            NotifyPropertyChanged(nameof(CurrentScene));
            SourceOrderList.Clear();
            NotifyPropertyChanged(nameof(SourceOrderList));
            NextScene = String.Empty;
            NotifyPropertyChanged(nameof(NextScene));
            SourceTypes = null;
            NotifyPropertyChanged(nameof(SourceTypes));
            ObsSourceDictionary.Clear();
            ObsSceneItemSceneDictionary.Clear();
        }

        private async void Client_Error(object sender, WsClientErrorMessage e)
        {
            if (e.ReconnectDelay > 0)
            {
                ReconnectTimeRemaining = e.ReconnectDelay;
            }
            else
            {
                ReconnectCountdownTimer.Stop();
                await Client.DisconnectAsync(false).ConfigureAwait(false);
            }
            if (e.Error != null)
            {
                ConnectionError = e.Error.Message;
                NotifyPropertyChanged(nameof(ConnectionError));
                ExtendedConnectionError = e.Error.InnerException?.Message;
                NotifyPropertyChanged(nameof(ExtendedConnectionError));
                _ = ChangeStatusColor(WebSocketState.Closed).ConfigureAwait(false);
            }
        }

        #endregion

        #region obs-websocket

        private async void Client_Event(object sender, ObsEventObject eventObject)
        {
            switch (eventObject.EventType)
            {
                case ObsEventType.Heartbeat:
                    await Heartbeat_Event((HeartbeatObsEvent)eventObject.MessageObject).ConfigureAwait(true);
                    break;
                case ObsEventType.SwitchScenes:
                    SwitchScenes_Event((SwitchScenesObsEvent)eventObject.MessageObject);
                    break;
                case ObsEventType.ScenesChanged:
                    break;
                case ObsEventType.TransitionBegin:
                    NextScene = ((TransitionBeginObsEvent)eventObject.MessageObject).ToScene;
                    NotifyPropertyChanged(nameof(NextScene));
                    break;
                case ObsEventType.TransitionEnd:
                case ObsEventType.TransitionVideoEnd:
                    NextScene = String.Empty;
                    NotifyPropertyChanged(nameof(NextScene));
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
                    NextScene = String.Empty;
                    NotifyPropertyChanged(nameof(NextScene));
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

        private async void Client_Reply(object sender, ObsReplyObject replyObject)
        {
            switch (replyObject.RequestType)
            {
                case ObsRequestType.GetAuthRequired:
                case ObsRequestType.Authenticate:
                    if (Client.CanSend)
                    {
                        await Obs_Get(ObsRequestType.GetSourceTypesList).ConfigureAwait(true);
                    }
                    break;
                case ObsRequestType.GetCurrentScene:
                    CurrentScene = replyObject.MessageObject as ObsScene;
                    await PopulateSceneItemSources(CurrentScene.Sources, CurrentScene).ConfigureAwait(true);
                    NotifyPropertyChanged(nameof(CurrentScene));
                    break;
                case ObsRequestType.GetSceneList:
                    ReadOnlyMemory<char> currentSceneName = (replyObject.MessageObject as GetSceneListReply).CurrentScene.AsMemory();
                    _Context.Send(
                        x => SceneList.Clear(),
                        null);
                    foreach (ObsScene scene in (replyObject.MessageObject as GetSceneListReply).Scenes)
                    {
                        _Context.Send(
                            x => SceneList.Add(scene),
                            null);
                        await PopulateSceneItemSources(scene.Sources, scene).ConfigureAwait(true);
                    }
                    Debug.Assert(SceneList.Any(x => x.Name == currentSceneName.ToString()), $"Scene {currentSceneName} wasn't added to sceneList.");
                    CurrentScene = SceneList.First(x => x.Name == currentSceneName.ToString());
                    NotifyPropertyChanged(nameof(CurrentScene));
                    break;
                case ObsRequestType.GetSourceTypesList:
                    SourceTypes = (GetSourceTypesListReply)replyObject.MessageObject;
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
                    Debug.Assert(SourceTypes.Types.Any(x => x.TypeId == sourceSettings.SourceType), $"Source type {sourceSettings.SourceType} isn't in list from server.");
                    newSource.Type = SourceTypes.Types.First(x => x.TypeId == sourceSettings.SourceType);
                    ObsSourceDictionary[newSource.Name] = newSource;
                    if (newSource.Name == Preferences.Default.obs_local_clock_source_name)
                    {
                        UpdateLocalClock(this, DateTime.UtcNow);
                        ChronoTimer.Instance.MinuteChanged -= UpdateLocalClock;
                        ChronoTimer.Instance.MinuteChanged += UpdateLocalClock;
                    }
                    else if (newSource.Name == Preferences.Default.obs_local_clock_weather_symbol_source_name)
                    {
                        ClearObsGdi2PlusText(newSource.Name);
                        ChronoTimer.Instance.MinuteChanged -= UpdateLocalWeather;
                        ChronoTimer.Instance.MinuteChanged += UpdateLocalWeather;
                        ChronoTimer.Instance.SecondChanged -= UpdateLocalWeather;
                        ChronoTimer.Instance.SecondChanged += UpdateLocalWeather;
                    }
                    else if (newSource.Name == Preferences.Default.obs_local_clock_weather_temp_source_name ||
                       newSource.Name == Preferences.Default.obs_local_clock_location_source_name ||
                       newSource.Name == Preferences.Default.obs_local_clock_weather_attrib1_source_name ||
                       newSource.Name == Preferences.Default.obs_local_clock_weather_attrib2_source_name)
                    {
                        ClearObsGdi2PlusText(newSource.Name);
                    }
                    else if (newSource.Name == Preferences.Default.obs_slideshow_source_name)
                    {
                        ChronoTimer.Instance.MinuteChanged -= SlideShowNextSlide;
                        ChronoTimer.Instance.MinuteChanged += SlideShowNextSlide;
                        ChronoTimer.Instance.SecondChanged -= SlideShowNextSlide;
                        ChronoTimer.Instance.SecondChanged += SlideShowNextSlide;
                    }
                    await Obs_Get(ObsRequestType.GetSourceFilters, sourceSettings.SourceName).ConfigureAwait(true);
                    break;
                case ObsRequestType.GetSourceFilters:
                    BaseType sourceToModify = ObsSourceDictionary[(replyObject.RequestMetadata.OriginalRequestData as GetSourceFiltersRequest).SourceName] as BaseType;
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
                    (ObsSourceDictionary[requestSent.SourceName] as BaseType).MonitorType = (replyObject.MessageObject as GetAudioMonitorTypeReply).MonitorType;
                    break;
                default:
                    break;
            }
        }

        private static void SlideShowNextSlide(object sender, DateTime e)
        {

            const UInt32 WM_KEYDOWN = 0x0100;
            Int32 VK_F20 = KeyInterop.VirtualKeyFromKey(Key.F20);

            // Next Slide is only available via Hotkeys in OBS Settings
            // Hard-coded to F20 for now until I find a way to send modifier keys without changing active window.
            if (e.Second == 0 || e.Second % Preferences.Default.obs_slideshow_next_scene_delay == 0)
            {
                ObservableProcess obsObservableProcess = ProcessCollection.Processes.Where(x => x.ProcessName == "obs64").FirstOrDefault();
                if (obsObservableProcess == default)
                {
                    return;
                }
                else
                {
                    Process obsProcess = Process.GetProcessById(obsObservableProcess.Id);
                    bool success = Utils.NativeMethods.PostMessage(obsProcess.MainWindowHandle, WM_KEYDOWN, VK_F20, 0);
                }
            }
        }

        /// <summary>
        /// Update the current timezone abbreviation
        /// </summary>
        private void UpdateTimezoneString()
        {
            if (Preferences.Default.local_timezone_use_utc_offset)
            {
                TimeSpan utcOffset = DateTimeOffset.Now.Offset;
                int utcHourOffset = utcOffset.Hours;
                string utcHourOffsetString = utcHourOffset > -1 ? $"+{utcHourOffset}" : utcHourOffset.ToString(CultureInfo.InvariantCulture);
                int utcMinuteOffset = utcOffset.Minutes;
                string utcMinuteOffsetString = utcMinuteOffset > 0 ? $":{utcMinuteOffset.ToString(CultureInfo.InvariantCulture)}" : "";
                TimezoneString = $"UTC{utcHourOffsetString}{utcMinuteOffsetString}";
            }
            else
            {
                // NB: WPF has no timezone abbreviation lookup table.
                if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now))
                {
                    TimezoneString = !string.IsNullOrEmpty(Preferences.Default.local_timezone_dst_abbreviation) ? Preferences.Default.local_timezone_dst_abbreviation : "?????";
                }
                else
                {
                    TimezoneString = !string.IsNullOrEmpty(Preferences.Default.local_timezone_abbreviation) ? Preferences.Default.local_timezone_abbreviation : "?????";
                }
            }
        }

        private async void UpdateLocalClock(object sender, DateTime e)
        {
            if (Client == null || !Client.CanSend)
            {
                return;
            }
            string localDisplayTime = String.Format(CultureInfo.CurrentCulture, Properties.Resources.obs_time_display_format, e.ToLocalTime().ToString(Properties.Resources.obs_time_string_format, CultureInfo.InvariantCulture), TimezoneString);
            SetTextGDIPlusPropertiesRequestTextPropertyOnly request = new SetTextGDIPlusPropertiesRequestTextPropertyOnly()
            {
                Source = Preferences.Default.obs_local_clock_source_name,
                Text = localDisplayTime
            };
            await Obs_Get(request).ConfigureAwait(true);
        }

        private async void ClearObsGdi2PlusText(string sourceName)
        {
            if (Client == null || !Client.CanSend)
            {
                return;
            }
            SetTextGDIPlusPropertiesRequestTextPropertyOnly request = new SetTextGDIPlusPropertiesRequestTextPropertyOnly()
            {
                Source = sourceName,
                Text = String.Empty
            };
            await Obs_Get(request).ConfigureAwait(true);
        }

        private async void FetchWeatherData(object sender, DateTime e)
        {
            // Early return if weather URI isn't set
            if (string.IsNullOrEmpty(Preferences.Default.obs_local_clock_weather_json_url))
            {
                return;
            }
            Uri weatherUri = new Uri(Preferences.Default.obs_local_clock_weather_json_url);
            // Early return if we already have data and aren't fetching this minute
            if (WeatherDataCollection?.Items.Count > 0 && (e.Minute == 0 || e.Minute % Preferences.Default.obs_local_clock_weather_json_url_fetch_delay > 0))
            {
                return;
            }
            // Fetch JSON from URI
            using HttpResponseMessage weatherdataResponse = await httpClient.GetAsync(weatherUri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            try
            {
                weatherdataResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                // How long should old data be used for?
                return;
            }
            // Parse JSON
            if (!(weatherdataResponse.Content.Headers.ContentType.MediaType == "application/json"))
            {
                return;
            }
            var responseStream = await weatherdataResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
            try
            {
                WeatherDataCollection = await JsonSerializer.DeserializeAsync<WeatherData>(responseStream).ConfigureAwait(false);
                if (CurrentLocalClockWeatherRecord == 0 && string.IsNullOrEmpty(PreviousLocalClockWeatherAttrib1))
                {
                    UpdateLocalWeather(this, DateTime.UtcNow);
                }
            }
            catch (JsonException)
            {
                Trace.WriteLine("Error parsing JSON.");
            }
        }

        private async void UpdateLocalWeather(object sender, DateTime e)
        {
            // Return early if not connected, no weather data, or nothing to change this second
            if (Client == null ||
                !Client.CanSend ||
                WeatherDataCollection == null ||
                WeatherDataCollection.Items.Count == 0 ||
                (!string.IsNullOrEmpty(PreviousLocalClockWeatherAttrib1) && e.Second % Preferences.Default.obs_local_clock_cycle_delay > 0)
                )
            {
                return;
            }
            // Set hold time (display duration) for local weather to the configured default
            int localWeatherHoldCount = Preferences.Default.obs_local_clock_cycle_delay;
            // Get the total duration of one cycle of weather data locations
            int localWeatherCycleTotalTime = WeatherDataCollection.Items.Count * Preferences.Default.obs_local_clock_cycle_delay;
            // If total duration of a cycle is less than a minute, increase display duration of our location and weather
            //  - i.e. if there are 10 weather data items, and we cycle every 5 seconds, we have a spare 10 seconds
            if (localWeatherCycleTotalTime < 60)
            {
                localWeatherHoldCount += 60 - localWeatherCycleTotalTime;
            }

            // FirstWeatherCycle ends at 0 seconds past the minute
            if (FirstWeatherCycle && e.Second == 0)
            {
                FirstWeatherCycle = false;
                NotifyPropertyChanged(nameof(FirstWeatherCycle));
            }
            // Return to first record at 0 seconds past the minute
            else if (!FirstWeatherCycle && e.Second == 0)
            {
                CurrentLocalClockWeatherRecord = 0;
            }
            // During FirstWeatherCycle there is nothing to do after initial run of this method
            // After FirstWeatherCycle there is nothing to do if we're holding at first record
            else if ((FirstWeatherCycle && !string.IsNullOrEmpty(PreviousLocalClockWeatherAttrib1)) ||
                (!FirstWeatherCycle && e.Second < localWeatherHoldCount))
            {
                return;
            }
            // After FirstWeatherCycle, switch to next record (we already returned early if e.Second % obs_local_clock_cycle_delay > 0)
            else if (!FirstWeatherCycle && e.Second >= localWeatherHoldCount)
            {
                CurrentLocalClockWeatherRecord++;
            }

            // JSON API text may contain HTML entities - &deg; is easier to type, &#xf002; won't usually look like a cloud, also avoids icon font hard-coding
            string weatherSymbolText = WebUtility.HtmlDecode(WeatherDataCollection.Items[CurrentLocalClockWeatherRecord].Symbol);
            string weatherTempText = WebUtility.HtmlDecode(WeatherDataCollection.Items[CurrentLocalClockWeatherRecord].Temperature);
            string weatherLocationText = WebUtility.HtmlDecode(WeatherDataCollection.Items[CurrentLocalClockWeatherRecord].Location);
            // Weather data attribution is currently stored as UTF-16 strings in user preferences
            string weatherAttrib1Text = Preferences.Default.obs_local_clock_weather_attrib1_text;
            string weatherAttrib2Text = Preferences.Default.obs_local_clock_weather_attrib2_text;

            List<SetTextGDIPlusPropertiesRequestTextPropertyOnly> requestList = new List<SetTextGDIPlusPropertiesRequestTextPropertyOnly>();

            // Add weather symbol update to list
            if (ObsSourceDictionary.ContainsKey(Preferences.Default.obs_local_clock_weather_symbol_source_name))
            {
                SetTextGDIPlusPropertiesRequestTextPropertyOnly weatherSymbol = new SetTextGDIPlusPropertiesRequestTextPropertyOnly()
                {
                    Source = Preferences.Default.obs_local_clock_weather_symbol_source_name,
                    Text = weatherSymbolText
                };
                requestList.Add(weatherSymbol);
            }
            // Add weather temperature update to list
            if (ObsSourceDictionary.ContainsKey(Preferences.Default.obs_local_clock_weather_temp_source_name))
            {
                SetTextGDIPlusPropertiesRequestTextPropertyOnly weatherTemp = new SetTextGDIPlusPropertiesRequestTextPropertyOnly()
                {
                    Source = Preferences.Default.obs_local_clock_weather_temp_source_name,
                    Text = weatherTempText
                };
                requestList.Add(weatherTemp);
            }
            // Add weather location update to list
            if (ObsSourceDictionary.ContainsKey(Preferences.Default.obs_local_clock_location_source_name))
            {
                SetTextGDIPlusPropertiesRequestTextPropertyOnly weatherLocation = new SetTextGDIPlusPropertiesRequestTextPropertyOnly()
                {
                    Source = Preferences.Default.obs_local_clock_location_source_name,
                    Text = weatherLocationText
                };
                requestList.Add(weatherLocation);
            }
            // Add weather data attribution line 1 update to list (if necessary)
            if (ObsSourceDictionary.ContainsKey(Preferences.Default.obs_local_clock_weather_attrib1_source_name))
            {
                if (weatherAttrib1Text != PreviousLocalClockWeatherAttrib1)
                {
                    SetTextGDIPlusPropertiesRequestTextPropertyOnly weatherAttrib1 = new SetTextGDIPlusPropertiesRequestTextPropertyOnly()
                    {
                        Source = Preferences.Default.obs_local_clock_weather_attrib1_source_name,
                        Text = weatherAttrib1Text
                    };
                    requestList.Add(weatherAttrib1);
                    PreviousLocalClockWeatherAttrib1 = weatherAttrib1Text;
                    NotifyPropertyChanged(PreviousLocalClockWeatherAttrib1);
                }
            }
            // Add weather data attribution line 2 update to list (if necessary)
            if (ObsSourceDictionary.ContainsKey(Preferences.Default.obs_local_clock_weather_attrib2_source_name))
            {
                if (weatherAttrib2Text != PreviousLocalClockWeatherAttrib2)
                {
                    SetTextGDIPlusPropertiesRequestTextPropertyOnly weatherAttrib2 = new SetTextGDIPlusPropertiesRequestTextPropertyOnly()
                    {
                        Source = Preferences.Default.obs_local_clock_weather_attrib2_source_name,
                        Text = weatherAttrib2Text
                    };
                    requestList.Add(weatherAttrib2);
                    PreviousLocalClockWeatherAttrib2 = weatherAttrib2Text;
                    NotifyPropertyChanged(PreviousLocalClockWeatherAttrib2);
                }
            }

            // Send all update requests in list
            foreach (SetTextGDIPlusPropertiesRequestTextPropertyOnly request in requestList)
            {
                _ = await Obs_Get(request).ConfigureAwait(true);
            }
            requestList.Clear();
        }

        private async Task PopulateSceneItemSources(IList<ObsSceneItem> sceneItems, ObsScene scene)
        {
            foreach (ObsSceneItem sceneItem in sceneItems)
            {
                sceneItem.Source = (BaseType)ObsSourceDictionary.GetValueOrDefault(sceneItem.Name);
                if (sceneItem.GroupChildren != null)
                {
                    await PopulateSceneItemSources(sceneItem.GroupChildren, scene).ConfigureAwait(true);
                }
                ObsSceneItemSceneDictionary[sceneItem.Id] = scene;
                await Obs_Get(ObsRequestType.GetSceneItemProperties, sceneItem.Name, scene.Name).ConfigureAwait(true);
                await Obs_Get(ObsRequestType.GetAudioMonitorType, sceneItem.Name).ConfigureAwait(true);
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
                ObsScene firstScene = ObsSceneItemSceneDictionary[sceneItemId];
                returnValue = GetSceneItemFromSceneItemId(sceneItemId, firstScene.Sources);
            }
            if (returnValue == null)
            {
                returnValue = sceneItems.Where(x => x.Id == sceneItemId).FirstOrDefault();
            }

            if (returnValue == null)
            {
                foreach (ObsSceneItem nextSceneItem in sceneItems.Where(x => x?.GroupChildren?.Count > 0).ToArray())
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
            while (Client.WaitingForReplyForType(ObsRequestType.GetSourceSettings))
            {
                await Task.Delay(250).ConfigureAwait(false);
            }

            foreach (BaseType source in ObsSourceDictionary.Values)
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
            AudioInterface audioInterface = AudioInterfaceCollection.GetAudioInterfaceById(dependencies.AudioDeviceId);
            if (audioInterface != null)
            {
                dependencies.AudioInterface = audioInterface;
            }
            //Trace.WriteLine($"{sourceReply.SourceName} -> {sourceReply.SourceType} -> device_id: {audioInterface?.ID} AKA {audioInterface?.FriendlyName}");
            // WASAPI and DirectShow source types should reference an audio device
            if (!dependencies.HasAudioInterface)
            {
                dependencies.DependencyProblem = true;
                return;
            }
        }

        #endregion

        #region obs-events

        private static async Task Heartbeat_Event(HeartbeatObsEvent messageObject)
        {
            await Instance.ChangeStatusColor(messageObject.Pulse ? PrimaryBrush : SecondaryBrush, true).ConfigureAwait(false);
        }

        private void SwitchScenes_Event(SwitchScenesObsEvent messageObject)
        {
            if (SceneList.Any(x => x.Name == messageObject.SceneName))
            {
                ChronoTimer.Instance.MinuteChanged -= SlideShowNextSlide;
                ChronoTimer.Instance.SecondChanged -= SlideShowNextSlide;
                CurrentScene = SceneList.First(x => x.Name == messageObject.SceneName);
                NotifyPropertyChanged(nameof(CurrentScene));
                if (CurrentScene.Sources.Where(x => x.Name == Preferences.Default.obs_slideshow_source_name).Any())
                {
                    ChronoTimer.Instance.MinuteChanged += SlideShowNextSlide;
                    ChronoTimer.Instance.SecondChanged += SlideShowNextSlide;
                }
            }
        }

        private void SourceOrderChanged_Event(SourceOrderChangedObsEvent messageObject)
        {
            if (messageObject.SceneName != CurrentScene.Name)
            {
                return;
            }
            SourceOrderList.Clear();
            SourceOrderList.AddRange(messageObject.SceneItems.Select(x => x.ItemId));
            NotifyPropertyChanged(nameof(SourceOrderList));
        }

        private void SceneItemRemoved_Event(SceneItemRemovedObsEvent messageObject)
        {
            if (!SceneList.Any(x => x.Name == messageObject.SceneName)) { return; }

            ObsScene scene = SceneList.First(x => x.Name == messageObject.SceneName);
            ObsSceneItem sceneItem = scene.Sources.FirstOrDefault(x => x.Name == messageObject.ItemName);
            if (sceneItem != default)
            {
                _Context.Send(
                    x => scene.Sources.Remove(sceneItem),
                    null);
            }
        }

        private void SourceCreated_Event(SourceCreatedObsEvent messageObject)
        {
            ObsSourceType sourceType = ObsTypes.ObsTypeNameDictionary[messageObject.SourceKind];
            object createdSource = messageObject.SourceSettingsObj;
            ObsWsReplyType replyType = SourceTypes.Types.FirstOrDefault(x => x.TypeId == messageObject.SourceKind);
            if (replyType == default) { return; }

            (createdSource as BaseType).Type = replyType;
            (createdSource as BaseType).Name = messageObject.SourceName;
            ObsSourceDictionary[messageObject.SourceName] = createdSource;
            GetDeviceIdForSource(createdSource as BaseType);
            if (sourceType == ObsSourceType.Scene)
            {
                ObsScene newScene = new ObsScene()
                {
                    Name = (createdSource as Scene).Name,
                    Sources = new ObservableCollection<ObsSceneItem>()
                };
                _Context.Send(
                    x => SceneList.Add(newScene),
                    null);
            }
        }

        private void SourceDestroyed_Event(SourceDestroyedObsEvent messageObject)
        {
            ObsSourceDictionary.Remove(messageObject.SourceName);
            if (messageObject.SourceName == Preferences.Default.obs_local_clock_source_name)
            {
                ChronoTimer.Instance.MinuteChanged -= UpdateLocalClock;
            }
            if (ObsTypes.ObsTypeNameDictionary[messageObject.SourceKind] == ObsSourceType.Scene)
            {
                ObsScene scene = SceneList.FirstOrDefault(x => x.Name == messageObject.SourceName);
                if (scene != default)
                {
                    _Context.Send(
                        x => SceneList.Remove(scene),
                        null);
                }
            }
        }

        private void SceneItemAdded_Event(SceneItemAddedObsEvent messageObject)
        {
            // Don't add scenes to themselves
            if (messageObject.SceneName == messageObject.ItemName) { return; }
            if (!ObsSourceDictionary.TryGetValue(messageObject.ItemName, out object source))
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
            if (SceneList.Any(x => x.Name == messageObject.SceneName))
            {
                _Context.Send(
                    x => SceneList.First(x => x.Name == messageObject.SceneName).Sources.Insert(0, newSceneItem),
                    null);
            }
        }

        private void SceneItemTransformChanged_Event(SceneItemTransformChangedObsEvent messageObject)
        {
            if (!SceneList.Any(x => x.Name == messageObject.SceneName)) { return; }

            ObsScene existingScene = SceneList.First(x => x.Name == messageObject.SceneName);
            ObsSceneItem existingSceneItem = existingScene.Sources.FirstOrDefault(x => x.Name == messageObject.ItemName);
            if (existingSceneItem != default)
            {
                existingSceneItem.Transform = messageObject.Transform;
            }
        }

        private void SourceVolumeChanged_Event(SourceVolumeChangedObsEvent messageObject)
        {
            (ObsSourceDictionary[messageObject.SourceName] as BaseType).Volume = messageObject.Volume;
        }

        private void SourceMuteStateChanged_Event(SourceMuteStateChangedObsEvent messageObject)
        {
            (ObsSourceDictionary[messageObject.SourceName] as BaseType).Muted = messageObject.Muted;
        }

        private void SourceAudioSyncOffsetChanged_Event(SourceAudioSyncOffsetChangedObsEvent messageObject)
        {
            (ObsSourceDictionary[messageObject.SourceName] as BaseType).SyncOffset = messageObject.SyncOffset;
        }

        private void SourceRenamed_Event(SourceRenamedObsEvent messageObject)
        {
            if (!ObsSourceDictionary.ContainsKey(messageObject.PreviousName))
            {
                return;
            }
            ObsSourceDictionary[messageObject.NewName] = ObsSourceDictionary[messageObject.PreviousName];
            (ObsSourceDictionary[messageObject.NewName] as BaseType).Name = messageObject.NewName;
            ObsSourceDictionary.Remove(messageObject.PreviousName);
        }

        private void SceneItemVisibilityChanged_Event(SceneItemVisibilityChangedObsEvent messageObject)
        {
            if (!SceneList.Any(x => x.Name == messageObject.SceneName)) { return; }

            ObsScene scene = SceneList.First(x => x.Name == messageObject.SceneName);
            ObsSceneItem sceneItem = scene.Sources.FirstOrDefault(x => x.Name == messageObject.ItemName);
            if (sceneItem != default)
            {
                sceneItem.Render = messageObject.ItemVisible;
            }
        }

        private void SceneItemLockChanged_Event(SceneItemLockChangedObsEvent messageObject)
        {
            if (!SceneList.Any(x => x.Name == messageObject.SceneName)) { return; }

            ObsScene scene = SceneList.First(x => x.Name == messageObject.SceneName);
            ObsSceneItem sceneItem = scene.Sources.FirstOrDefault(x => x.Name == messageObject.ItemName);
            if (sceneItem != default)
            {
                sceneItem.Locked = messageObject.ItemLocked;
            }
        }

        private void SourceAudioMixersChanged_Event(SourceAudioMixersChangedObsEvent messageObject)
        {
            (ObsSourceDictionary[messageObject.SourceName] as BaseType).Mixers = messageObject.Mixers;
            (ObsSourceDictionary[messageObject.SourceName] as BaseType).HexMixersValue = messageObject.HexMixersValue;
        }

        private void SourceFilterAdded_Event(SourceFilterAddedObsEvent messageObject)
        {
            BaseType source = (BaseType)ObsSourceDictionary[messageObject.SourceName];
            BaseFilter filter = (BaseFilter)messageObject.FilterSettingsObj;
            source.Filters.Add(filter);
        }

        private void SourceFilterRemoved_Event(SourceFilterRemovedObsEvent messageObject)
        {
            BaseType source = (BaseType)ObsSourceDictionary[messageObject.SourceName];
            BaseFilter filter = source.Filters.FirstOrDefault(x => x.Name == messageObject.FilterName);
            if (filter != default)
            {
                source.Filters.Remove(filter);
            }
        }

        private void SourceFilterVisibilityChanged_Event(SourceFilterVisibilityChangedObsEvent messageObject)
        {
            BaseType source = (BaseType)ObsSourceDictionary[messageObject.SourceName];
            BaseFilter filter = source.Filters.FirstOrDefault(x => x.Name == messageObject.FilterName);
            if (filter != default)
            {
                filter.Enabled = messageObject.FilterEnabled;
            }
        }

        private void SourceFiltersReordered_Event(SourceFiltersReorderedObsEvent messageObject)
        {
            BaseType source = (BaseType)ObsSourceDictionary[messageObject.SourceName];
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
            return await Client.ObsSend(ObsWsRequest.GetInstanceOfType(requestType)).ConfigureAwait(true);
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
            return await Client.ObsSend(request).ConfigureAwait(true);
        }

        private async ValueTask<Guid> Obs_Get(object request)
        {
            return await Client.ObsSend(request).ConfigureAwait(true);
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
            return await Client.ObsSend(request).ConfigureAwait(true);
        }

        #endregion

        #region Status colour changes

        private static readonly Dictionary<Brush, System.Drawing.Icon> brushToIconDictionary = new Dictionary<Brush, System.Drawing.Icon>() {
            { Brushes.Gray, Properties.Resources.icon_neutral },
            { PrimaryBrush, Properties.Resources.icon },
            { SecondaryBrush, Properties.Resources.icon_secondary },
            { Brushes.DarkGreen, Properties.Resources.icon_dark_green },
            { Brushes.DarkGoldenrod, Properties.Resources.icon_dark_golden_rod },
            { Brushes.Red, Properties.Resources.icon_red }
        };

        public async Task ChangeStatusColor(Brush brush1, bool returnToNeutral = true)
        {
            // Low priority changes are out of date after 250ms.
            bool haveSemaphore = await iconSemaphore.WaitAsync(250).ConfigureAwait(true);

            // If we haven't entered semaphore after 250ms, give up
            if (!haveSemaphore)
            {
                return;
            }

            // Use first colour for 0.25 seconds
            if (brush1 != null)
            {
                ConnectionStatusBrush = brush1;
                NotifyPropertyChanged(nameof(ConnectionStatusBrush));
                SystemTrayIcon.Instance.NotifyIcon.Icon = brushToIconDictionary[brush1];
                await Task.Delay(250).ConfigureAwait(true);
            }
            // Use second colour for 0.25 seconds
            if (returnToNeutral)
            {
                ConnectionStatusBrush = Brushes.Gray;
                NotifyPropertyChanged(nameof(ConnectionStatusBrush));
                SystemTrayIcon.Instance.NotifyIcon.Icon = Properties.Resources.icon_neutral;
                await Task.Delay(250).ConfigureAwait(true);
            }

            // Release semaphore to allow colour change
            iconSemaphore.Release();
        }

        private async Task ChangeStatusColor(WebSocketState state)
        {
            // High priority changes can wait forever.
            bool haveSemaphore = await iconSemaphore.WaitAsync(-1).ConfigureAwait(true);
            Debug.Assert(haveSemaphore, "haveSemaphore should be true at this point.");

            // Change status colour based on connection status
            ConnectionStatusBrush = state switch
            {
                WebSocketState.Open => Brushes.DarkGreen,
                WebSocketState.Connecting => Brushes.DarkGoldenrod,
                _ => Brushes.Red
            };
            NotifyPropertyChanged(nameof(ConnectionStatusBrush));

            // Change system tray icon based on connection status
            SystemTrayIcon.Instance.NotifyIcon.Icon = brushToIconDictionary[ConnectionStatusBrush];
            // Maintain colour for one second
            await Task.Delay(1000).ConfigureAwait(true);

            // Release semaphore to allow colour change
            iconSemaphore.Release();
        }

        #endregion

        #region dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    ReconnectCountdownTimer.Dispose();
                    iconSemaphore.Dispose();
                    audioWorkarounds.Dispose();
                    httpClient.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ObsWebsocketConnection()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
