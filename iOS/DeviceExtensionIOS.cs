using System;
using UIKit;
using Xamarin.Forms;
using XFStreamingAudio.iOS;

[assembly: Dependency(typeof(DeviceExtensionIOS))]
namespace XFStreamingAudio.iOS
{
    public class DeviceExtensionIOS : IDeviceExtension
    {
        public DeviceExtensionIOS()
        {
        }

        #region IDeviceExtension implementation

        public bool CanOpenUrl(Uri uri)
        {
            return UIApplication.SharedApplication.CanOpenUrl(uri);
        }

        #endregion
    }
}
