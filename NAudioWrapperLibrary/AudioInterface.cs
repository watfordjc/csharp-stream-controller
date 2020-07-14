﻿using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Text;
using NAudio.CoreAudioApi.Interfaces;
using System.Threading;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace uk.JohnCook.dotnet.NAudioWrapperLibrary
{
    public class AudioInterface : INotifyPropertyChanged
    {
        private MMDevice _Device;

        public string ID { get; private set; }
        public DataFlow DataFlow { get; private set; }
        public DeviceState State { get; private set; }
        public string FriendlyName { get; set; }
        public double Volume { get; set; }
        public bool Muted { get; set; }
        public Guid VolumeNotificationGuid { get; set; }
        public string AudioDeviceName { get; set; }
        public bool IsActive { get; private set; }
        public bool IsDefaultRender {  get { return AudioInterfaceCollection.Instance.DefaultRender == this; } }
        public bool IsDefaultCapture { get { return AudioInterfaceCollection.Instance.DefaultCapture == this; } }

        public MMDevice Device
        {
            get { return _Device; }
            // When Device is set, populate the additional properties
            set
            {
                _Device = value ?? throw new ArgumentNullException(nameof(value));
                ID = value.ID;
                DataFlow = value.DataFlow;
                SetProperties(value.State);
            }
        }

        public AudioInterface()
        {
            AudioInterfaceCollection.Instance.DefaultDeviceChange += Instance_DefaultDeviceChange;
        }


        public void SetProperties(DeviceState state)
        {
            // If previous DeviceState was Active, remove listener.
            if (State == DeviceState.Active)
            {
                Device.AudioEndpointVolume.OnVolumeNotification -= VolumeChanged;
            }
            // Set new DeviceState.
            State = state;
            // Set FriendlyName if possible, otherwise set it to ID.
            if (state == DeviceState.Active || state == DeviceState.Unplugged)
            {
                FriendlyName = Device.FriendlyName;
            } else
            {
                FriendlyName = Device.ID;
            }
            // If DeviceState is Active, more properties exist.
            if (state == DeviceState.Active)
            {
                Volume = Math.Round(Device.AudioEndpointVolume.MasterVolumeLevelScalar * 100f);
                Device.AudioEndpointVolume.OnVolumeNotification += VolumeChanged;
                Muted = Device.AudioEndpointVolume.Mute;
                IsActive = true;
            } else
            {
                IsActive = false;
            }
            // Notify all properties have changed.
            OnPropertyChanged(null);
        }

        private void Instance_DefaultDeviceChange(object sender, DataFlow dataFlow)
        {
            switch (dataFlow)
            {
                case DataFlow.Render:
                    OnPropertyChanged(nameof(IsDefaultRender));
                    break;
                case DataFlow.Capture:
                    OnPropertyChanged(nameof(IsDefaultCapture));
                    break;
                default:
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void VolumeChanged(AudioVolumeNotificationData data)
        {
            if (data == null) { throw new ArgumentNullException(nameof(data)); }

            // Update the Volume and notify the change.
            Volume = Math.Round(data.MasterVolume * 100f);
            OnPropertyChanged("Volume");
            // Update Muted and notify the change.
            Muted = data.Muted;
            OnPropertyChanged("Muted");
        }

        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void NotifyMMAudioPropertyChanged(SynchronizationContext context, PropertyKey key)
        {
            // TODO: Is property name format correct?
            PropertyChanged?.Invoke(context, new PropertyChangedEventArgs("Device.Properties[" + key + "]"));
        }
    }
}