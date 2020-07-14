using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace uk.JohnCook.dotnet.NAudioWrapperLibrary.AudioDeviceCmdlets
{
    [ComImport, Guid("870af99c-171d-4f9e-af0d-e63df40c2bc9")]
    internal class CPolicyConfigClient
    {
    }

    public class PolicyConfigClient
    {
        private readonly IPolicyConfig _PolicyConfig;
        private readonly IPolicyConfigVista _PolicyConfigVista;
        private readonly IPolicyConfig10 _PolicyConfig10;

        public PolicyConfigClient()
        {
            _PolicyConfig = new CPolicyConfigClient() as IPolicyConfig;
            if (_PolicyConfig != null) { return; }

            _PolicyConfigVista = new CPolicyConfigClient() as IPolicyConfigVista;
            if (_PolicyConfigVista != null) { return; }

            Debug.Assert(!(_PolicyConfig == null && _PolicyConfigVista == null), "Have Microsoft changed the Guid for IPolicyConfig again?");
            _PolicyConfig10 = new CPolicyConfigClient() as IPolicyConfig10;
        }

        public void SetDefaultEndpoint(string deviceId, NAudio.CoreAudioApi.Role role)
        {
            if (_PolicyConfig != null)
            {
                Marshal.ThrowExceptionForHR(_PolicyConfig.SetDefaultEndpoint(deviceId, role));
                return;
            }
            if (_PolicyConfigVista != null)
            {
                Marshal.ThrowExceptionForHR(_PolicyConfigVista.SetDefaultEndpoint(deviceId, role));
                return;
            }
            if (_PolicyConfig10 != null)
            {
                Marshal.ThrowExceptionForHR(_PolicyConfig10.SetDefaultEndpoint(deviceId, role));
            }
        }
    }
}