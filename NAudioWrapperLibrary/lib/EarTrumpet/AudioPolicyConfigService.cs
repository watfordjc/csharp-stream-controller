using uk.JohnCook.dotnet.NAudioWrapperLibrary.EarTrumpet.Interop;
using uk.JohnCook.dotnet.NAudioWrapperLibrary.EarTrumpet.Interop.Helpers;
using uk.JohnCook.dotnet.NAudioWrapperLibrary.EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Diagnostics;
using NAudio.CoreAudioApi;
using NAudio.Utils;
using System.Runtime.InteropServices;
using System.Linq;

namespace uk.JohnCook.dotnet.NAudioWrapperLibrary.EarTrumpet.DataModel.WindowsAudio.Internal
{
    class AudioPolicyConfig
    {
        private const string DEVINTERFACE_AUDIO_RENDER = "#{e6327cad-dcec-4949-ae8a-991e976a79d2}";
        private const string DEVINTERFACE_AUDIO_CAPTURE = "#{2eef81be-33fa-4800-9670-1cd474972c3f}";
        private const string MMDEVAPI_TOKEN = @"\\?\SWD#MMDEVAPI#";

        private IAudioPolicyConfigFactory _sharedPolicyConfig;
        private readonly DataFlow _flow;

        public AudioPolicyConfig(DataFlow flow)
        {
            _flow = flow;
        }

        private void EnsurePolicyConfig()
        {
            if (_sharedPolicyConfig == null)
            {
                _sharedPolicyConfig = AudioPolicyConfigFactory.Create();
            }
        }

        private string GenerateDeviceId(string deviceId)
        {
            return $"{MMDEVAPI_TOKEN}{deviceId}{(_flow == DataFlow.Render ? DEVINTERFACE_AUDIO_RENDER : DEVINTERFACE_AUDIO_CAPTURE)}";
        }

        private static string UnpackDeviceId(string deviceId)
        {
            if (deviceId.StartsWith(MMDEVAPI_TOKEN, StringComparison.Ordinal)) deviceId = deviceId.Remove(0, MMDEVAPI_TOKEN.Length);
            if (deviceId.EndsWith(DEVINTERFACE_AUDIO_RENDER, StringComparison.Ordinal)) deviceId = deviceId.Remove(deviceId.Length - DEVINTERFACE_AUDIO_RENDER.Length);
            if (deviceId.EndsWith(DEVINTERFACE_AUDIO_CAPTURE, StringComparison.Ordinal)) deviceId = deviceId.Remove(deviceId.Length - DEVINTERFACE_AUDIO_CAPTURE.Length);
            return deviceId;
        }

        public void SetDefaultEndPoint(string deviceId, int processId)
        {
            EnsurePolicyConfig();

            IntPtr hstring = IntPtr.Zero;

            if (!string.IsNullOrWhiteSpace(deviceId))
            {
                var str = GenerateDeviceId(deviceId);
                NativeMethods.WindowsCreateString(str, (uint)str.Length, out hstring);
            }

            int hr1, hr2;
            hr1 = _sharedPolicyConfig.SetPersistedDefaultAudioEndpoint((uint)processId, _flow, Role.Multimedia, hstring);
            hr2 = _sharedPolicyConfig.SetPersistedDefaultAudioEndpoint((uint)processId, _flow, Role.Console, hstring);

            if (hr1 != HResult.S_OK || hr2 != HResult.S_OK)
            {
                Exception ex1 = Marshal.GetExceptionForHR(hr1);
                Exception ex2 = Marshal.GetExceptionForHR(hr2);
                if (ex1.GetType() == typeof(ArgumentException) || ex2.GetType() == typeof(ArgumentException))
                {
                    ProcessCollection.Processes.FirstOrDefault(x => x.Id == processId).AudioDeviceTogglingNeeded = true;
                }
                else
                {
                    // Print error or throw exception? Print error for now.
                    Trace.WriteLine($"Error in {nameof(SetDefaultEndPoint)} for {processId}: MultimediaResult={ex1}, ConsoleResult={ex2}");
                }
            }
        }

        public string GetDefaultEndPoint(int processId)
        {
            int hr;
            EnsurePolicyConfig();

            hr = _sharedPolicyConfig.GetPersistedDefaultAudioEndpoint((uint)processId, _flow, Role.Multimedia | Role.Console, out string deviceId);
            if (hr == HResult.S_OK)
            {
                return UnpackDeviceId(deviceId);
            }
            else
            {
                Exception ex = Marshal.GetExceptionForHR(hr);
                // If a process doesn't have a persisted default audio endpoint for a DataFlow direction, an ArgumentException is raised. Ignore it.
                if (ex.GetType() == typeof(ArgumentException)) { return null; }
                else
                {
                    // Some other exception type has occurred but we don't know why.
                    Trace.WriteLine($"{ex}");
                    return null;
                }
            }
        }

        public void ClearDefaultEndPoints()
        {
            EnsurePolicyConfig();
            Marshal.ThrowExceptionForHR(_sharedPolicyConfig.ClearAllPersistedApplicationDefaultEndpoints());
        }
    }
}
