using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using Stream_Controller.SharedModels;

namespace OBSWebSocketLibrary.Models.TypeDefs.SourceTypes
{
    public class BaseType
    {
        public BaseType()
        {
            Dependencies = new DependencyProperties();
        }
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
                get { return AudioDeviceId != null && AudioDeviceId != String.Empty; }
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
                get { return VideoDeviceId != null && VideoDeviceId != String.Empty; }
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
