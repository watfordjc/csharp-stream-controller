using uk.JohnCook.dotnet.NAudioWrapperLibrary.EarTrumpet.Interop.MMDeviceAPI;

namespace uk.JohnCook.dotnet.NAudioWrapperLibrary.EarTrumpet.Interop.Helpers
{
    public class AudioPolicyConfigFactory
    {
        public static IAudioPolicyConfigFactory Create()
        {
            var iid = typeof(IAudioPolicyConfigFactory).GUID;
            Combase.RoGetActivationFactory("Windows.Media.Internal.AudioPolicyConfig", ref iid, out object factory);
            return (IAudioPolicyConfigFactory)factory;
        }
    }
}
