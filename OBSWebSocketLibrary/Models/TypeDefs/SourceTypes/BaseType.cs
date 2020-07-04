using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Stream_Controller.SharedModels;

namespace OBSWebSocketLibrary.Models.TypeDefs.SourceTypes
{
    public class BaseType : OBSWebSocketLibrary.Models.RequestReplies.GetSourceTypesList.Type
    {
        public BaseType()
        {
            Dependencies = new DependencyProperties();
        }

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