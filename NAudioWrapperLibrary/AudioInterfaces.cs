using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NAudio;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using Stream_Controller.SharedModels;

namespace NAudioWrapperLibrary
{
    public sealed class AudioInterfaces : ObservableCollection<MMDevice>, IDisposable
    {
        private static readonly MMDeviceEnumerator _Enumerator = new MMDeviceEnumerator();
        private static readonly ObservableCollection<AudioInterface> _Devices = new ObservableCollection<AudioInterface>();
        private static readonly AudioEndpointNotificationCallback _NotificationCallback = new AudioEndpointNotificationCallback();
        private static readonly IMMNotificationClient _NotificationClient = (IMMNotificationClient)_NotificationCallback;
        public AudioInterface DefaultRender { get; private set; }
        public AudioInterface DefaultCapture { get; private set; }

        private static readonly Lazy<AudioInterfaces> lazySingleton =
            new Lazy<AudioInterfaces>(
                () => new AudioInterfaces()
            );
        private bool disposedValue;

        public static AudioInterfaces Instance { get { return lazySingleton.Value; } }

        private AudioInterfaces()
        {
            Initialise();
        }

        private async void Initialise()
        {
            await PopulateInterfaces();
            Trace.WriteLine($"Default render device: {DefaultRender.FriendlyName}");
            Trace.WriteLine($"Default capture device: {DefaultCapture.FriendlyName}");
        }

        private async Task PopulateInterfaces()
        {
            MMDeviceCollection collection = null;
            await Task.Run(
                () => collection = _Enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.All)
                ).ConfigureAwait(false);
            _Enumerator.RegisterEndpointNotificationCallback(_NotificationClient);
            foreach (MMDevice device in collection)
            {
                AudioInterface audioDevice = new AudioInterface
                {
                    Device = device,
                    FriendlyName = device.State switch
                    {
                        DeviceState.NotPresent => null,
                        _ => device.FriendlyName
                    }
                };
                _Devices.Add(audioDevice);
            }
            UpdateDefaultDevice(DataFlow.Render, _Enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console).ID);
            UpdateDefaultDevice(DataFlow.Capture, _Enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console).ID);
        }

        private void UpdateDefaultDevice(DataFlow flow, string defaultDeviceId)
        {
            if (flow == DataFlow.Render) {
                Instance.DefaultRender = (from AudioInterface in _Devices
                                          where AudioInterface.ID == defaultDeviceId
                                          select AudioInterface).First();
            } else if (flow == DataFlow.Capture) {
                Instance.DefaultCapture = (from AudioInterface in _Devices
                                           where AudioInterface.ID == defaultDeviceId
                                           select AudioInterface).First();
            }
        }

        // TODO: Implement methods to propagate events
        private class AudioEndpointNotificationCallback : IMMNotificationClient
        {
            void IMMNotificationClient.OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
            {
                if (role != Role.Console) { return; }
                Instance.UpdateDefaultDevice(flow, defaultDeviceId);
            }

            void IMMNotificationClient.OnDeviceAdded(string pwstrDeviceId)
            {
                MMDevice audioDevice = _Enumerator.GetDevice(pwstrDeviceId);
                AudioInterface device = new AudioInterface
                {
                    Device = audioDevice,
                    FriendlyName = audioDevice.State switch
                    {
                        DeviceState.NotPresent => null,
                        _ => audioDevice.FriendlyName
                    }
                };
                _Devices.Add(device);
            }

            void IMMNotificationClient.OnDeviceRemoved(string deviceId)
            {
                AudioInterface audioDevice = (from AudioInterface in _Devices
                                              where AudioInterface.ID == deviceId
                                              select AudioInterface).First();
                _Devices.Remove(audioDevice);
            }

            void IMMNotificationClient.OnDeviceStateChanged(string deviceId, DeviceState newState)
            {
                throw new NotImplementedException();
            }

            void IMMNotificationClient.OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
            {
                throw new NotImplementedException();
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
