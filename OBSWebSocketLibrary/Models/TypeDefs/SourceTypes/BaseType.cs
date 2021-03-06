﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using uk.JohnCook.dotnet.StreamController.SharedModels;

namespace uk.JohnCook.dotnet.OBSWebSocketLibrary.TypeDefs
{
    public class BaseType : INotifyPropertyChanged
    {
        private bool muted;
        private double volume;
        private int syncOffset;
        private string name;
        private string monitorType;
        private IList<ObsWsMixer> mixers;
        private string hexMixersValue;

        public BaseType()
        {
            Dependencies = new DependencyProperties();
            Filters = new ObservableCollection<BaseFilter>();
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
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<ObsWsMixer> Mixers
#pragma warning restore CA2227 // Collection properties should be read only
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
#pragma warning disable CA2227 // Collection properties should be read only
        public ObservableCollection<BaseFilter> Filters { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        [JsonIgnore]
        public TypeDefs.ObsWsReplyType Type { get; set; }

        [JsonIgnore]
        public DependencyProperties Dependencies { get; set; }
    }
}
