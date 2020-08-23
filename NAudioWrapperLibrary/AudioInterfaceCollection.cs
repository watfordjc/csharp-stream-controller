using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Win32;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace uk.JohnCook.dotnet.NAudioWrapperLibrary
{

    public sealed class AudioInterfaceCollection : ObservableCollection<MMDevice>
    {
        private readonly SynchronizationContext _Context;
        private static readonly MMDeviceEnumerator _Enumerator = new MMDeviceEnumerator();
        private static AudioEndpointNotificationCallback _NotificationCallback = null;
        private static IMMNotificationClient _NotificationClient;
        private static string appdataPath;
        private const string deviceApplicationPreferencesFilename = "DeviceApplicationPreferences.json";
        private const string applicationDevicePreferencesFilename = "ApplicationDevicePreferences.json";
        private SharedModels.DeviceApplicationPreferences deviceApplicationPreferences;
        private SharedModels.ApplicationDevicePreferences applicationDevicePreferences;
        private bool jsonDataDirty = false;
        private static readonly System.Timers.Timer jsonSaveTimer = new System.Timers.Timer(60000);

        public static AudioInterfaceCollection Instance { get { return lazySingleton.Value; } }
        public static ObservableCollection<AudioInterface> Devices { get; } = new ObservableCollection<AudioInterface>();
        public static ObservableCollection<AudioInterface> ActiveDevices { get; } = new ObservableCollection<AudioInterface>();
        public AudioInterface DefaultRender { get; private set; }
        public AudioInterface DefaultCapture { get; private set; }
        public bool DevicesAreEnumerated { get; private set; }

        private static readonly Lazy<AudioInterfaceCollection> lazySingleton =
            new Lazy<AudioInterfaceCollection>(
                () => new AudioInterfaceCollection()
            );


        public static void RegisterEndpointNotificationCallback(IMMNotificationClient notificationClient)
        {
            _Enumerator.RegisterEndpointNotificationCallback(notificationClient);
        }

        private AudioInterfaceCollection()
        {
            _Context = SynchronizationContext.Current;
            _NotificationCallback = new AudioEndpointNotificationCallback(_Context);
            _NotificationClient = (IMMNotificationClient)_NotificationCallback;

            string installDirectory = InstallPath();
            string runningDirectory = AppDomain.CurrentDomain.BaseDirectory;

            bool isInstalledVersion = installDirectory != null && runningDirectory == installDirectory;

            appdataPath = isInstalledVersion switch
            {
                true => Path.GetDirectoryName(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath) + "\\",
                false => runningDirectory
            };

            Initialise();
        }

        private static string InstallPath()
        {
            // Determine if program is installed from an installer
            // Registry key created by Setup project on installation:
            //  HKCU\Software\!(loc.ProgramCreatorCompany)\!(loc.ProductName)\Uninstall\installPath = [INSTALLFOLDER]
            //  HKCU\Software\John Cook\Stream Controller\Uninstall\installPath = C:\Program Files\John Cook\Stream Controller\

            // Get assembly information of main executable.
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo((Assembly.GetEntryAssembly() as Assembly).Location);
            // Convert assembly information into registry key
            //  * NB: ProductName uses PascalCase whereas Setup project (loc.ProductName) uses spaces
            string subkey = "Software\\" + fileVersionInfo.CompanyName + "\\" + System.Text.RegularExpressions.Regex.Replace(fileVersionInfo.ProductName, "(\\B[A-Z])", " $1") + "\\Uninstall";

            using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(subkey);
            return registryKey?.GetValue("installPath", null).ToString();
        }

        private async void Initialise()
        {
            await PopulateInterfaces().ConfigureAwait(false);
            jsonSaveTimer.Elapsed += JsonSaveTimer_Elapsed;
            jsonSaveTimer.Enabled = true;
            jsonSaveTimer.Start();
        }

        private async Task PopulateInterfaces()
        {
            MMDeviceCollection collection = null;
            await Task.Run(
                () => collection = _Enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.All)
                ).ContinueWith(
            result => DevicesEnumerated(collection), TaskScheduler.Default
            ).ConfigureAwait(false);
        }

        private void DevicesEnumerated(MMDeviceCollection collection)
        {
            _Enumerator.RegisterEndpointNotificationCallback(_NotificationClient);
            foreach (MMDevice device in collection)
            {
                AudioInterface audioDevice = new AudioInterface
                {
                    Device = device
                };
                _Context.Send(
                    x => Devices.Add(audioDevice),
                    null);
                if (audioDevice.IsActive)
                {
                    _Context.Send(
                        x => ActiveDevices.Add(audioDevice),
                        null);
                }
            }
            UpdateDefaultDevice(DataFlow.Render, _Enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console).ID);
            UpdateDefaultDevice(DataFlow.Capture, _Enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console).ID);
            DevicesAreEnumerated = true;
            NotifyCollectionEnumerated();
            RestoreDeviceApplicationPreferences();
            bool processesEnumerated = false;
            _Context.Send(
                x => processesEnumerated = ProcessCollection.Instance.ProcessesAreEnumerated,
                null);
            if (!processesEnumerated)
            {
                ProcessCollection.Instance.CollectionEnumerated += Processes_CollectionEnumerated;
            }
            else
            {
                Processes_CollectionEnumerated(this, EventArgs.Empty);
            }

            ProcessCollection.Instance.CollectionChanged += Processes_CollectionChanged;
        }

        private async void RestoreDeviceApplicationPreferences()
        {
            if (!Directory.Exists(appdataPath))
            {
                Directory.CreateDirectory(appdataPath);
            }
            if (File.Exists(appdataPath + deviceApplicationPreferencesFilename))
            {
                using FileStream deviceApplicationJsonFile = File.OpenRead(appdataPath + deviceApplicationPreferencesFilename);
                try
                {
                    deviceApplicationPreferences = await JsonSerializer.DeserializeAsync<SharedModels.DeviceApplicationPreferences>(deviceApplicationJsonFile).ConfigureAwait(true);
                }
                catch (JsonException e)
                {
                    Trace.WriteLine($"JSON Error: {e.Message} - {e.InnerException.Message}");
                }
            }
            if (deviceApplicationPreferences == null)
            {
                deviceApplicationPreferences = new SharedModels.DeviceApplicationPreferences
                {
                    Devices = new List<SharedModels.DeviceApplicationPreference>()
                };
            }
            if (File.Exists(appdataPath + applicationDevicePreferencesFilename))
            {
                using FileStream applicationDeviceJsonFile = File.OpenRead(appdataPath + applicationDevicePreferencesFilename);
                try
                {
                    applicationDevicePreferences = await JsonSerializer.DeserializeAsync<SharedModels.ApplicationDevicePreferences>(applicationDeviceJsonFile).ConfigureAwait(true);
                }
                catch (JsonException e)
                {
                    Trace.WriteLine($"JSON Error: {e.Message} - {e.InnerException.Message}");
                }
            }
            if (applicationDevicePreferences == null)
            {
                applicationDevicePreferences = new SharedModels.ApplicationDevicePreferences
                {
                    Applications = new List<SharedModels.ApplicationDevicePreference>()
                };
            }
        }

        private async void JsonSaveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!jsonDataDirty)
            {
                CheckApplicationDeviceChanges();
                return;
            }

            ReadOnlyMemory<char> deviceApplicationJson = JsonSerializer.Serialize(deviceApplicationPreferences).AsMemory();
            ReadOnlyMemory<char> applicationDeviceJson = JsonSerializer.Serialize(applicationDevicePreferences).AsMemory();

            using StreamWriter deviceApplicationJsonFile = new StreamWriter(appdataPath + deviceApplicationPreferencesFilename, false);
            using StreamWriter applicationDeviceJsonFile = new StreamWriter(appdataPath + applicationDevicePreferencesFilename, false);

            await deviceApplicationJsonFile.WriteAsync(deviceApplicationJson).ConfigureAwait(false);
            await applicationDeviceJsonFile.WriteAsync(applicationDeviceJson).ConfigureAwait(false);

            jsonDataDirty = false;

            CheckApplicationDeviceChanges();
        }

        private void CheckApplicationDeviceChanges()
        {
            foreach (ObservableProcess process in ProcessCollection.Processes)
            {
                AudioInterface windowsPreferredRender = GetDefaultApplicationDevice(DataFlow.Render, process);
                AudioInterface windowsPreferredCapture = GetDefaultApplicationDevice(DataFlow.Capture, process);
                SharedModels.ApplicationDevicePreference applicationDevicePreference = applicationDevicePreferences.Applications.FirstOrDefault(x => x.Name == process.ProcessName);
                string preferredRenderId = applicationDevicePreference?.Devices?.RenderDeviceId;
                string preferredCaptureId = applicationDevicePreference?.Devices?.CaptureDeviceId;
                bool windowsThinksDefault = windowsPreferredRender == null && windowsPreferredCapture == null;
                bool noPreferences = preferredRenderId == null && preferredCaptureId == null;
                // There is agreement that this process uses the default devices.
                if (noPreferences && windowsThinksDefault)
                {
                    continue;
                }
                // Windows doesn't think this process uses the default device and we don't have any preferences for the process.
                if (applicationDevicePreference == default && !windowsThinksDefault)
                {
                    ChangeDefaultApplicationDevice(windowsPreferredRender, process);
                    ChangeDefaultApplicationDevice(windowsPreferredCapture, process);
                }
                // Windows doesn't think this process uses the default device. We agree, but we disagree on the preferred audio device.
                else if (!windowsThinksDefault)
                {
                    bool changeNeeded = preferredRenderId != windowsPreferredRender?.ID;
                    changeNeeded = changeNeeded || preferredCaptureId != windowsPreferredCapture?.ID;
                    if (changeNeeded)
                    {
                        Trace.WriteLine($"Preference conflict detected between Windows Settings and Stream Controller for process {process.ProcessName}.");
                    }
                    if (process.AudioDeviceTogglingNeeded)
                    {
                        ToggleDefaultApplicationDevice(process);
                        process.AudioDeviceTogglingNeeded = false;
                    }
                }
                // Windows thinks the process uses the default device. We disagree and are waiting for the process to make a sound.
                else if (windowsThinksDefault)
                {
                    EarTrumpet.DataModel.WindowsAudio.Internal.AudioPolicyConfig audioPolicyConfig;
                    if (preferredRenderId != null)
                    {
                        audioPolicyConfig = new EarTrumpet.DataModel.WindowsAudio.Internal.AudioPolicyConfig(DataFlow.Render);
                        audioPolicyConfig.SetDefaultEndPoint(preferredRenderId, process.Id);
                    }
                    if (preferredCaptureId != null)
                    {
                        audioPolicyConfig = new EarTrumpet.DataModel.WindowsAudio.Internal.AudioPolicyConfig(DataFlow.Capture);
                        audioPolicyConfig.SetDefaultEndPoint(preferredCaptureId, process.Id);
                    }
                }
            }
        }

        private void Processes_CollectionEnumerated(object sender, EventArgs e)
        {
            foreach (ObservableProcess process in ProcessCollection.Processes)
            {
                ProcessAdded(process);
            }
        }

        private void Processes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ObservableProcess process in e.NewItems)
                {
                    ProcessAdded(process);
                }
            }
        }

        private void AddDeviceApplicationPreference(AudioInterface audioInterface, ObservableProcess process)
        {
            SharedModels.DeviceApplicationPreference deviceApplicationPreference = deviceApplicationPreferences.Devices.FirstOrDefault(x => x.Id == audioInterface.ID);
            if (deviceApplicationPreference == default)
            {
                deviceApplicationPreference = new SharedModels.DeviceApplicationPreference()
                {
                    Id = audioInterface.ID,
                    Applications = new List<string>()
                };
                deviceApplicationPreferences.Devices.Add(deviceApplicationPreference);
            }
            if (!deviceApplicationPreference.Applications.Contains(process.ProcessName))
            {
                deviceApplicationPreference.Applications.Add(process.ProcessName);
            }
            jsonDataDirty = true;
        }

        private void AddApplicationDevicePreference(ObservableProcess process, AudioInterface audioInterface)
        {
            SharedModels.ApplicationDevicePreference applicationDevicePreference = applicationDevicePreferences.Applications.FirstOrDefault(x => x.Name == process.ProcessName);
            if (applicationDevicePreference == default)
            {
                applicationDevicePreference = new SharedModels.ApplicationDevicePreference()
                {
                    Name = process.ProcessName,
                    Devices = new SharedModels.DefaultDevicePreference()
                };
                applicationDevicePreferences.Applications.Add(applicationDevicePreference);
            }
            _ = audioInterface.DataFlow switch
            {
                DataFlow.Render => applicationDevicePreference.Devices.RenderDeviceId = audioInterface.ID,
                DataFlow.Capture => applicationDevicePreference.Devices.CaptureDeviceId = audioInterface.ID,
                _ => null
            };
            jsonDataDirty = true;
        }

        private void RemoveOldApplicationDevicePreference(ObservableProcess process, AudioInterface audioInterface)
        {
            SharedModels.ApplicationDevicePreference applicationDevicePreference = applicationDevicePreferences.Applications.FirstOrDefault(x => x.Name == process.ProcessName);
            if (applicationDevicePreference == default) { return; }
            string previousPreferredInterface = null;
            switch (audioInterface.DataFlow)
            {
                case DataFlow.Render:
                    previousPreferredInterface = applicationDevicePreference.Devices.RenderDeviceId;
                    applicationDevicePreference.Devices.RenderDeviceId = null;
                    break;
                case DataFlow.Capture:
                    previousPreferredInterface = applicationDevicePreference.Devices.CaptureDeviceId;
                    applicationDevicePreference.Devices.CaptureDeviceId = null;
                    break;
                default:
                    return;
            }
            if (applicationDevicePreference.Devices.RenderDeviceId == null && applicationDevicePreference.Devices.CaptureDeviceId == null)
            {
                applicationDevicePreferences.Applications.Remove(applicationDevicePreference);
            }
            SharedModels.DeviceApplicationPreference deviceApplicationPreference = deviceApplicationPreferences.Devices.FirstOrDefault(x => x.Id == previousPreferredInterface);
            if (deviceApplicationPreference == default) { return; }
            deviceApplicationPreference.Applications.Remove(process.ProcessName);
            if (deviceApplicationPreference.Applications.Count == 0)
            {
                deviceApplicationPreferences.Devices.Remove(deviceApplicationPreference);
            }
        }

        private void ProcessAdded(ObservableProcess process)
        {
            if (process == null) { throw new ArgumentNullException(nameof(process)); }
            while (applicationDevicePreferences == null) {
                Task.Delay(250);
            }

            SharedModels.ApplicationDevicePreference applicationDevicePreference = applicationDevicePreferences.Applications.FirstOrDefault(x => x.Name == process.ProcessName);
            if (applicationDevicePreference == null)
            {
                applicationDevicePreference = new SharedModels.ApplicationDevicePreference();
            }
            AudioInterface preferredRender = GetApplicationDevicePreference(DataFlow.Render, applicationDevicePreference, process);
            if (preferredRender != null)
            {
                ChangeDefaultApplicationDevice(preferredRender, process);
            }
            AudioInterface preferredCapture = GetApplicationDevicePreference(DataFlow.Capture, applicationDevicePreference, process);
            if (preferredCapture != null)
            {
                ChangeDefaultApplicationDevice(preferredCapture, process);
            }
        }

        private AudioInterface GetApplicationDevicePreference(DataFlow dataFlow, SharedModels.ApplicationDevicePreference applicationDevicePreference, ObservableProcess process)
        {
            string preferredInterfaceId = dataFlow switch
            {
                DataFlow.Render => applicationDevicePreference?.Devices?.RenderDeviceId,
                DataFlow.Capture => applicationDevicePreference?.Devices?.CaptureDeviceId,
                _ => null
            };
            if (preferredInterfaceId != null)
            {
                return GetAudioInterfaceById(preferredInterfaceId);
            }
            AudioInterface audioInterface = GetDefaultApplicationDevice(dataFlow, process);
            if (audioInterface != null)
            {
                RemoveOldApplicationDevicePreference(process, audioInterface);
                AddApplicationDevicePreference(process, audioInterface);
                AddDeviceApplicationPreference(audioInterface, process);
                return audioInterface;
            }
            return null;
        }

        public event EventHandler<DataFlow> DefaultDeviceChanged;
        public event EventHandler CollectionEnumerated;

        void NotifyDefaultDeviceChanged(DataFlow flow)
        {
            _Context.Send(
                x => DefaultDeviceChanged?.Invoke(this, flow)
            , null);
        }

        void NotifyCollectionEnumerated()
        {
            _Context.Send(
                x => CollectionEnumerated?.Invoke(this, EventArgs.Empty)
            , null);
        }

        public static void ChangeDefaultDevice(string defaultDeviceId)
        {
            AudioDeviceCmdlets.PolicyConfigClient policyConfigClient = new AudioDeviceCmdlets.PolicyConfigClient();
            policyConfigClient.SetDefaultEndpoint(defaultDeviceId, Role.Console);
        }

        public static AudioInterface GetDefaultApplicationDevice(DataFlow dataFlow, ObservableProcess process)
        {
            if (process == null) { throw new ArgumentNullException(nameof(process)); }

            EarTrumpet.DataModel.WindowsAudio.Internal.AudioPolicyConfig audioPolicyConfig = new EarTrumpet.DataModel.WindowsAudio.Internal.AudioPolicyConfig(dataFlow);
            return GetAudioInterfaceById(audioPolicyConfig.GetDefaultEndPoint(process.Id));
        }

        public static void ChangeDefaultApplicationDevice(AudioInterface audioInterface, ObservableProcess process)
        {
            if (audioInterface == null) { throw new ArgumentNullException(nameof(audioInterface)); }
            if (process == null) { throw new ArgumentNullException(nameof(process)); }

            EarTrumpet.DataModel.WindowsAudio.Internal.AudioPolicyConfig audioPolicyConfig = new EarTrumpet.DataModel.WindowsAudio.Internal.AudioPolicyConfig(audioInterface.DataFlow);
            audioPolicyConfig.SetDefaultEndPoint(audioInterface.ID, process.Id);
            Instance.RemoveOldApplicationDevicePreference(process, audioInterface);
            Instance.AddDeviceApplicationPreference(audioInterface, process);
            Instance.AddApplicationDevicePreference(process, audioInterface);
        }

        public static void ClearApplicationDefaultDevice(DataFlow dataFlow, ObservableProcess process)
        {
            if (process == null) { throw new ArgumentNullException(nameof(process)); }

            AudioInterface dataFlowInterface = dataFlow switch
            {
                DataFlow.Render => Instance.DefaultRender,
                DataFlow.Capture => Instance.DefaultCapture,
                _ => null
            };
            if (dataFlowInterface == null) { throw new ArgumentException($"{Enum.GetName(typeof(DataFlow),dataFlow)} is not {Enum.GetName(typeof(DataFlow), DataFlow.Render)} or {Enum.GetName(typeof(DataFlow), DataFlow.Capture)}.", nameof(dataFlow)); }

            EarTrumpet.DataModel.WindowsAudio.Internal.AudioPolicyConfig audioPolicyConfig = new EarTrumpet.DataModel.WindowsAudio.Internal.AudioPolicyConfig(dataFlow);
            audioPolicyConfig.SetDefaultEndPoint(String.Empty, process.Id);
            Instance.RemoveOldApplicationDevicePreference(process, dataFlowInterface);
        }

        public static void ClearAllApplicationDefaultDevices(DataFlow dataFlow)
        {
            EarTrumpet.DataModel.WindowsAudio.Internal.AudioPolicyConfig audioPolicyConfig = new EarTrumpet.DataModel.WindowsAudio.Internal.AudioPolicyConfig(dataFlow);
            audioPolicyConfig.ClearDefaultEndPoints();
            Instance.deviceApplicationPreferences.Devices.Clear();
            Instance.applicationDevicePreferences.Applications.Clear();
            Instance.jsonDataDirty = true;
        }

        public static void ToggleDefaultApplicationDevice(ObservableProcess process)
        {
            if (process == null) { throw new ArgumentNullException(nameof(process)); }

            AudioInterface renderDevice = GetDefaultApplicationDevice(DataFlow.Render, process);
            if (renderDevice != null)
            {
                Trace.WriteLine($"Toggling PID {process.Id} render device to default and back to {renderDevice.FriendlyName}.");
                ChangeDefaultApplicationDevice(Instance.DefaultRender, process);
                ChangeDefaultApplicationDevice(renderDevice, process);
            }
            AudioInterface captureDevice = GetDefaultApplicationDevice(DataFlow.Capture, process);
            if (captureDevice != null)
            {
                Trace.WriteLine($"Toggling PID {process.Id} capture device to default and back to {captureDevice.FriendlyName}.");
                ChangeDefaultApplicationDevice(Instance.DefaultCapture, process);
                ChangeDefaultApplicationDevice(captureDevice, process);
            }
        }

        public static void ToggleAllDefaultApplicationDevice()
        {
            foreach (ObservableProcess process in ProcessCollection.Processes)
            {
                if (String.IsNullOrEmpty(process.MainWindowTitle)) { continue; }
                ToggleDefaultApplicationDevice(process);
            }
        }

        private void UpdateDefaultDevice(DataFlow flow, string defaultDeviceId)
        {
            if (flow == DataFlow.Render)
            {
                Instance.DefaultRender = GetAudioInterfaceById(defaultDeviceId);
                NotifyDefaultDeviceChanged(flow);
            }
            else if (flow == DataFlow.Capture)
            {
                Instance.DefaultCapture = GetAudioInterfaceById(defaultDeviceId);
                NotifyDefaultDeviceChanged(flow);
            }
        }

        public static AudioInterface GetAudioInterfaceById(string deviceId)
        {
            return Devices.Where(device => device.ID == deviceId).FirstOrDefault();
        }

        public static AudioInterface GetAudioInterfaceByName(string deviceName)
        {
            return Devices.Where(device => device.FriendlyName == deviceName).FirstOrDefault();
        }

        public static AudioInterface GetAudioInterfaceByVolumeNotificationGuid(Guid notificationId)
        {
            return Devices.Where(device => device.VolumeNotificationGuid == notificationId).FirstOrDefault();
        }

        private class AudioEndpointNotificationCallback : IMMNotificationClient
        {
            private readonly SynchronizationContext mContext;

            public AudioEndpointNotificationCallback(SynchronizationContext context)
            {
                mContext = context;
            }

            void IMMNotificationClient.OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
            {
                if (role != Role.Console) { return; }
                Instance.UpdateDefaultDevice(flow, defaultDeviceId);
            }

            void IMMNotificationClient.OnDeviceAdded(string pwstrDeviceId)
            {
                AudioInterface device = new AudioInterface
                {
                    Device = _Enumerator.GetDevice(pwstrDeviceId)
                };
                mContext.Send(
                    x => Devices.Add(device)
                , null);
                if (device.IsActive)
                {
                    mContext.Send(
                        x => ActiveDevices.Add(device),
                        null);
                }
            }

            void IMMNotificationClient.OnDeviceRemoved(string deviceId)
            {
                AudioInterface audioInterface = GetAudioInterfaceById(deviceId);
                mContext.Send(
                    x => Devices.Remove(GetAudioInterfaceById(deviceId))
                , null);
                if (audioInterface.IsActive)
                {
                    mContext.Send(
                        x => ActiveDevices.Remove(audioInterface),
                        null);
                }
            }

            void IMMNotificationClient.OnDeviceStateChanged(string deviceId, DeviceState newState)
            {
                AudioInterface audioInterface = GetAudioInterfaceById(deviceId);
                if (audioInterface != null)
                {
                    audioInterface.SetProperties(newState);
                }

                if (newState != DeviceState.Active) {
                    if (ActiveDevices.Contains(audioInterface))
                    {
                        mContext.Send(
                            x => ActiveDevices.Remove(audioInterface),
                            null);
                    }
                    return; 
                }

                mContext.Send(
                    x => ActiveDevices.Add(audioInterface),
                    null);

                SharedModels.DeviceApplicationPreference deviceApplicationPreference = Instance.deviceApplicationPreferences.Devices.FirstOrDefault(x => x.Id == deviceId);
                if (deviceApplicationPreference == default) { return; }

                foreach (string applicationName in deviceApplicationPreference.Applications)
                {
                    ObservableProcess[] processes = ProcessCollection.Processes.Where(x => x.ProcessName == applicationName).ToArray();
                    foreach (ObservableProcess process in processes)
                    {
                        ChangeDefaultApplicationDevice(audioInterface, process);
                    }
                }
            }

            void IMMNotificationClient.OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
            {
                GetAudioInterfaceById(pwstrDeviceId).NotifyMMAudioPropertyChanged(mContext, key);
            }
        }
    }
}
