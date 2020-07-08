using StreamController.SharedModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace OBSWebSocketLibrary.Models.TypeDefs.SourceTypes
{
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
        private IList<string> filePaths;
        private IList<string> uris;

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
                audioInterface = value ?? throw new ArgumentNullException(nameof(value));
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

#pragma warning disable CA2227 // Collection properties should be read only
        public IList<string> FilePaths
#pragma warning restore CA2227 // Collection properties should be read only
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
            get { return FilePaths != null && FilePaths.Count != 0; }
        }

#pragma warning disable CA2227 // Collection properties should be read only
        public IList<string> Uris
#pragma warning restore CA2227 // Collection properties should be read only
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
            get { return Uris != null && Uris.Count != 0; }
        }
    }
}
