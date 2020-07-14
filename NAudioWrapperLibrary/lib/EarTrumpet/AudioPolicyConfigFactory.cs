using uk.JohnCook.dotnet.NAudioWrapperLibrary.EarTrumpet.Interop.MMDeviceAPI;

namespace uk.JohnCook.dotnet.NAudioWrapperLibrary.EarTrumpet.Interop.Helpers
{
    public static class AudioPolicyConfigFactory
    {
        public static IAudioPolicyConfigFactory Create()
        {
            var iid = typeof(IAudioPolicyConfigFactory).GUID;
            NativeMethods.RoGetActivationFactory("Windows.Media.Internal.AudioPolicyConfig", ref iid, out object factory);
            return (IAudioPolicyConfigFactory)factory;
        }
    }
}
