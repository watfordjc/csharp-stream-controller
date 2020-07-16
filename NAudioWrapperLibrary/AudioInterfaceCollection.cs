﻿using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using NAudio;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace uk.JohnCook.dotnet.NAudioWrapperLibrary
{

    public sealed class AudioInterfaceCollection : ObservableCollection<MMDevice>, IDisposable
    {
        private readonly SynchronizationContext _Context;
        private static readonly MMDeviceEnumerator _Enumerator = new MMDeviceEnumerator();
        private static AudioEndpointNotificationCallback _NotificationCallback = null;
        private static IMMNotificationClient _NotificationClient;
        private SharedModels.DeviceApplicationPreferences deviceApplicationPreferences;
        private SharedModels.ApplicationDevicePreferences applicationDevicePreferences;

        public AudioInterface DefaultRender { get; private set; }
        public AudioInterface DefaultCapture { get; private set; }
        public static ObservableCollection<AudioInterface> Devices { get; } = new ObservableCollection<AudioInterface>();
        public bool DevicesAreEnumerated { get; private set; }

        private static readonly Lazy<AudioInterfaceCollection> lazySingleton =
            new Lazy<AudioInterfaceCollection>(
                () => new AudioInterfaceCollection()
            );
        private bool disposedValue;

        public static AudioInterfaceCollection Instance { get { return lazySingleton.Value; } }

        public static void RegisterEndpointNotificationCallback(IMMNotificationClient notificationClient)
        {
            _Enumerator.RegisterEndpointNotificationCallback(notificationClient);
        }

        private AudioInterfaceCollection()
        {
            _Context = SynchronizationContext.Current;
            _NotificationCallback = new AudioEndpointNotificationCallback(_Context);
            _NotificationClient = (IMMNotificationClient)_NotificationCallback;
            Initialise();
        }

        private async void Initialise()
        {
            await PopulateInterfaces().ConfigureAwait(false);
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
            }
            UpdateDefaultDevice(DataFlow.Render, _Enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console).ID);
            UpdateDefaultDevice(DataFlow.Capture, _Enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console).ID);
            DevicesAreEnumerated = true;
            NotifyCollectionEnumerated();
            RestoreDeviceApplicationPreferences();
            if (Processes.ProcessesAreEnumerated)
            {
                Processes_CollectionEnumerated(this, EventArgs.Empty);
            }
            Processes.CollectionChanged += Processes_CollectionChanged;
        }

        private void RestoreDeviceApplicationPreferences()
        {
            deviceApplicationPreferences = new SharedModels.DeviceApplicationPreferences
            {
                Devices = new List<SharedModels.DeviceApplicationPreference>()
            };
            applicationDevicePreferences = new SharedModels.ApplicationDevicePreferences
            {
                Applications = new List<SharedModels.ApplicationDevicePreference>()
            };
        }

        private void Processes_CollectionEnumerated(object sender, EventArgs e)
        {
            foreach (ObservableProcess process in Processes)
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
        }

        private void ProcessAdded(ObservableProcess process)
        {
            if (process == null) { throw new ArgumentNullException(nameof(process)); }

            SharedModels.ApplicationDevicePreference applicationDevicePreference = applicationDevicePreferences.Applications.FirstOrDefault(x => x.Name == process.ProcessName);
            if (applicationDevicePreference == null)
            {
                applicationDevicePreference = new SharedModels.ApplicationDevicePreference();
            }
            AudioInterface preferredRender = GetApplicationDevicePreference(DataFlow.Render, applicationDevicePreference, process);
            if (preferredRender != null)
            {
                ChangeDefaultApplicationDevice(preferredRender, process.Id);
            }
            AudioInterface preferredCapture = GetApplicationDevicePreference(DataFlow.Capture, applicationDevicePreference, process);
            if (preferredCapture != null)
            {
                ChangeDefaultApplicationDevice(preferredCapture, process.Id);
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
            AudioInterface audioInterface = GetDefaultApplicationDevice(dataFlow, process.Id);
            if (audioInterface != null)
            {
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

        public static AudioInterface GetDefaultApplicationDevice(DataFlow dataFlow, int processId)
        {
            EarTrumpet.DataModel.WindowsAudio.Internal.AudioPolicyConfig audioPolicyConfig = new EarTrumpet.DataModel.WindowsAudio.Internal.AudioPolicyConfig(dataFlow);
            return GetAudioInterfaceById(audioPolicyConfig.GetDefaultEndPoint(processId));
        }

        public static void ChangeDefaultApplicationDevice(AudioInterface audioInterface, int processId)
        {
            if (audioInterface == null) { throw new ArgumentNullException(nameof(audioInterface)); }

            EarTrumpet.DataModel.WindowsAudio.Internal.AudioPolicyConfig audioPolicyConfig = new EarTrumpet.DataModel.WindowsAudio.Internal.AudioPolicyConfig(audioInterface.DataFlow);
            audioPolicyConfig.SetDefaultEndPoint(audioInterface.ID, processId);
        }

        public static void ClearAllApplicationDefaultDevices(DataFlow dataFlow)
        {
            EarTrumpet.DataModel.WindowsAudio.Internal.AudioPolicyConfig audioPolicyConfig = new EarTrumpet.DataModel.WindowsAudio.Internal.AudioPolicyConfig(dataFlow);
            audioPolicyConfig.ClearDefaultEndPoints();
        }

        public static void ToggleDefaultApplicationDevice(int processId)
        {
            AudioInterface renderDevice = GetDefaultApplicationDevice(DataFlow.Render, processId);
            if (renderDevice != null)
            {
                Trace.WriteLine($"Toggling PID {processId} render device to default and back to {renderDevice.FriendlyName}.");
                ChangeDefaultApplicationDevice(Instance.DefaultRender, processId);
                ChangeDefaultApplicationDevice(renderDevice, processId);
            }
            AudioInterface captureDevice = GetDefaultApplicationDevice(DataFlow.Capture, processId);
            if (captureDevice != null)
            {
                Trace.WriteLine($"Toggling PID {processId} capture device to default and back to {captureDevice.FriendlyName}.");
                ChangeDefaultApplicationDevice(Instance.DefaultCapture, processId);
                ChangeDefaultApplicationDevice(captureDevice, processId);
            }
        }

        public static void ToggleAllDefaultApplicationDevice()
        {
            foreach (Process process in Process.GetProcesses())
            {
                if (String.IsNullOrEmpty(process.MainWindowTitle)) { continue; }
                ToggleDefaultApplicationDevice(process.Id);
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

        // TODO: Implement methods to propagate events
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
            }

            void IMMNotificationClient.OnDeviceRemoved(string deviceId)
            {
                mContext.Send(
                    x => Devices.Remove(GetAudioInterfaceById(deviceId))
                , null);
            }

            void IMMNotificationClient.OnDeviceStateChanged(string deviceId, DeviceState newState)
            {
                GetAudioInterfaceById(deviceId).SetProperties(newState);
            }

            void IMMNotificationClient.OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
            {
                GetAudioInterfaceById(pwstrDeviceId).NotifyMMAudioPropertyChanged(mContext, key);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~AudioInterfaces()
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
    }
}
