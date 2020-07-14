using NAudio.CoreAudioApi;
using NAudio.Utils;
using System;
using System.Runtime.InteropServices;

namespace uk.JohnCook.dotnet.NAudioWrapperLibrary.EarTrumpet.Interop.MMDeviceAPI
{
    [Guid("2a59116d-6c4f-45e0-a74f-707e3fef9258")]
    [InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    public interface IAudioPolicyConfigFactory
    {
        // Disable CA/IDE warnings for non-implemented methods.
#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable IDE1006 // Naming Styles
        int __incomplete__add_CtxVolumeChange();
        int __incomplete__remove_CtxVolumeChanged();
        int __incomplete__add_RingerVibrateStateChanged();
        int __incomplete__remove_RingerVibrateStateChange();
        int __incomplete__SetVolumeGroupGainForId();
        int __incomplete__GetVolumeGroupGainForId();
        int __incomplete__GetActiveVolumeGroupForEndpointId();
        int __incomplete__GetVolumeGroupsForEndpoint();
        int __incomplete__GetCurrentVolumeContext();
        int __incomplete__SetVolumeGroupMuteForId();
        int __incomplete__GetVolumeGroupMuteForId();
        int __incomplete__SetRingerVibrateState();
        int __incomplete__GetRingerVibrateState();
        int __incomplete__SetPreferredChatApplication();
        int __incomplete__ResetPreferredChatApplication();
        int __incomplete__GetPreferredChatApplication();
        int __incomplete__GetCurrentChatApplications();
        int __incomplete__add_ChatContextChanged();
        int __incomplete__remove_ChatContextChanged();
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CA1707 // Identifiers should not contain underscores
        [PreserveSig]
        int SetPersistedDefaultAudioEndpoint(uint processId, DataFlow flow, Role role, IntPtr deviceId);
        [PreserveSig]
        int GetPersistedDefaultAudioEndpoint(uint processId, DataFlow flow, Role role, [Out, MarshalAs(UnmanagedType.HString)] out string deviceId);
        [PreserveSig]
        int ClearAllPersistedApplicationDefaultEndpoints();
    }
}
