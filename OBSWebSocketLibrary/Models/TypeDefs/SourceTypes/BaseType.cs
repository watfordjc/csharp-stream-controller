﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using StreamController.SharedModels;

namespace OBSWebSocketLibrary.Models.TypeDefs.SourceTypes
{
    public class BaseType : INotifyPropertyChanged
    {
        private bool muted;
        private double volume;
        private int syncOffset;
        private string name;
        private string monitorType;
        private Mixer[] mixers;
        private string hexMixersValue;

        public BaseType()
        {
            Dependencies = new DependencyProperties();
            Filters = new ObservableCollection<FilterTypes.BaseFilter>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [JsonIgnore]
        public bool Muted {
            get { return muted; }
            set
            {
                muted = value;
                NotifyPropertyChanged();
            }
        }
        [JsonIgnore]
        public double Volume {
            get { return volume; }
            set
            {
                volume = value;
                NotifyPropertyChanged();
            }
        }
        [JsonIgnore]
        public int SyncOffset {
            get { return syncOffset; }
            set
            {
                syncOffset = value;
                NotifyPropertyChanged();
            }
        }
        [JsonIgnore]
        public string Name {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged();
            }
        }
        [JsonIgnore]
        public string MonitorType
        {
            get { return monitorType; }
            set
            {
                monitorType = value;
                NotifyPropertyChanged();
            }
        }
        [JsonIgnore]
        public Mixer[] Mixers
        {
            get { return mixers; }
            set
            {
                mixers = value;
                NotifyPropertyChanged();
            }
        }
        [JsonIgnore]
        public string HexMixersValue
        {
            get { return hexMixersValue; }
            set
            {
                hexMixersValue = value;
                NotifyPropertyChanged();
            }
        }
        [JsonIgnore]
        public ObservableCollection<FilterTypes.BaseFilter> Filters { get; set; }

        [JsonIgnore]
        public OBSWebSocketLibrary.Models.RequestReplies.GetSourceTypesList.Type Type { get; set; }

        [JsonIgnore]
        public DependencyProperties Dependencies { get; set; }

        public class DependencyProperties : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            private bool dependencyProblem;
            private string audioDeviceId;
            private AudioInterface audioInterface;
            private string videoDeviceId;
            private string[] filePaths;
            private string[] uris;

            public bool DependencyProblem
            {
                get { return dependencyProblem; }
                set
                {
                    dependencyProblem = value;
                    NotifyPropertyChanged();
                }
            }

            public string AudioDeviceId
            {
                get { return audioDeviceId; }
                set
                {
                    audioDeviceId = value;
                    NotifyPropertyChanged();
                }
            }

            public AudioInterface AudioInterface
            {
                get { return audioInterface; }
                set
                {
                    audioInterface = value;
                    // FriendlyName is equal to ID if FriendlyName isn't available due to interface state (e.g. NotPresent)
                    DependencyProblem = audioInterface.ID == audioInterface.FriendlyName;
                    NotifyPropertyChanged();
                    audioInterface.PropertyChanged += AudioInterface_PropertyChanged;
                }
            }

            private void AudioInterface_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == null)
                {
                    if (audioInterface.State != NAudio.CoreAudioApi.DeviceState.Active)
                    {
                        DependencyProblem = true;
                    }
                    else
                    {
                        DependencyProblem = false;
                    }
                }
            }

            public bool HasAudioInterface
            {
                get { return !String.IsNullOrEmpty(AudioDeviceId); }
            }

            public string VideoDeviceId
            {
                get { return videoDeviceId; }
                set
                {
                    videoDeviceId = value;
                    NotifyPropertyChanged();
                }
            }
            public bool HasVideoInterface
            {
                get { return !String.IsNullOrEmpty(VideoDeviceId); }
            }

            public string[] FilePaths
            {
                get { return filePaths; }
                set
                {
                    filePaths = value;
                    NotifyPropertyChanged();
                }
            }
            public bool HasFiles
            {
                get { return FilePaths != null && FilePaths.Length != 0; }
            }

            public string[] Uris
            {
                get { return uris; }
                set
                {
                    uris = value;
                    NotifyPropertyChanged();
                }
            }
            public bool HasURIs
            {
                get { return Uris != null && Uris.Length != 0; }
            }
        }
    }
}
