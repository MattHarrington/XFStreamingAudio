using System;
using Xamarin.Forms;
using XFStreamingAudio.Droid;

[assembly: Dependency(typeof(DeviceExtensionAndroid))]
namespace XFStreamingAudio.Droid
{
    public class DeviceExtensionAndroid : IDeviceExtension
    {
        public DeviceExtensionAndroid()
        {
        }

        #region IDeviceExtension implementation

        public bool CanOpenUrl(Uri uri)
        {
            return false;  // TODO: Use Intent to launch Facebook or Twitter apps, if installed
        }

        #endregion
    }
}
