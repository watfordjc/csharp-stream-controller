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
using StreamController.SharedModels;

namespace NAudioWrapperLibrary
{

    public sealed class AudioInterfaces : ObservableCollection<MMDevice>, IDisposable
    {
        private readonly SynchronizationContext _Context;
        private static readonly MMDeviceEnumerator _Enumerator = new MMDeviceEnumerator();
        private static readonly ObservableCollection<AudioInterface> _Devices = new ObservableCollection<AudioInterface>();
        private static AudioEndpointNotificationCallback _NotificationCallback = null;
        private static IMMNotificationClient _NotificationClient;

        public AudioInterface DefaultRender { get; private set; }
        public AudioInterface DefaultCapture { get; private set; }
        public static ObservableCollection<AudioInterface> Devices { get { return _Devices; } }

        private static readonly Lazy<AudioInterfaces> lazySingleton =
            new Lazy<AudioInterfaces>(
                () => new AudioInterfaces()
            );
        private bool disposedValue;

        public static AudioInterfaces Instance { get { return lazySingleton.Value; } }

        public static void RegisterEndpointNotificationCallback(IMMNotificationClient notificationClient)
        {
            _Enumerator.RegisterEndpointNotificationCallback(notificationClient);
        }

        private AudioInterfaces()
        {
            _Context = SynchronizationContext.Current;
            _NotificationCallback = new AudioEndpointNotificationCallback(_Context);
            _NotificationClient = (IMMNotificationClient)_NotificationCallback;
            Initialise();
        }

        private async void Initialise()
        {
            await PopulateInterfaces();
        }

        private async Task PopulateInterfaces()
        {
            MMDeviceCollection collection = null;
            await Task.Run(
                () => collection = _Enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.All)
                ).ContinueWith(
            result => DevicesEnumerated(collection)
            );
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
                    x => Devices.Add(audioDevice)
                , null);
            }
            UpdateDefaultDevice(DataFlow.Render, _Enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console).ID);
            UpdateDefaultDevice(DataFlow.Capture, _Enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console).ID);
            OnDeviceCollectionEnumerated(true);
        }

        void OnDeviceCollectionEnumerated(bool e)
        {
            _Context.Send(
                x => DeviceCollectionEnumerated?.Invoke(this, e)
            , null);
        }

        public delegate DataFlow OnDefaultDeviceChanged();
        public delegate bool OnDevicesEnumerated();

        public event EventHandler<DataFlow> DefaultDeviceChange;
        public event EventHandler<bool> DeviceCollectionEnumerated;

        void NotifyDefaultDeviceChange(DataFlow flow)
        {
            _Context.Send(
                x => DefaultDeviceChange?.Invoke(this, flow)
            , null);
        }

        private void UpdateDefaultDevice(DataFlow flow, string defaultDeviceId)
        {
            if (flow == DataFlow.Render)
            {
                Instance.DefaultRender = GetAudioInterfaceById(defaultDeviceId);
                NotifyDefaultDeviceChange(flow);
            }
            else if (flow == DataFlow.Capture)
            {
                Instance.DefaultCapture = GetAudioInterfaceById(defaultDeviceId);
                NotifyDefaultDeviceChange(flow);
            }
        }

        public static AudioInterface GetAudioInterfaceById(string deviceId)
        {
            return _Devices.Where(device => device.ID == deviceId).FirstOrDefault();
        }

        public static AudioInterface GetAudioInterfaceByName(string deviceName)
        {
            return _Devices.Where(device => device.FriendlyName == deviceName).FirstOrDefault();
        }
        
        public static AudioInterface GetAudioInterfaceByVolumeNotificationGuid(Guid guid)
        {
            return _Devices.Where(device => device.VolumeNotificationGuid == guid).FirstOrDefault();
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
                    x => _Devices.Add(device)
                , null);
            }

            void IMMNotificationClient.OnDeviceRemoved(string deviceId)
            {
                mContext.Send(
                    x => _Devices.Remove(GetAudioInterfaceById(deviceId))
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
