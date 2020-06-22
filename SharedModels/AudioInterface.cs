using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stream_Controller.SharedModels
{
    public class AudioInterface
    {
        // TODO: Design model for NAudio and obs-websocket

        public MMDevice Device { get; set; }
        public string ID => Device.ID;
        public string FriendlyName { get; set; }
        public string Audio_device_name { get; set; }
    }
}
