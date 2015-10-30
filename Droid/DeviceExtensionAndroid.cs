using System;
using Xamarin.Forms;
using XFStreamingAudio.Droid;
using Android.Content;

[assembly: Dependency(typeof(DeviceExtensionAndroid))]
namespace XFStreamingAudio.Droid
{
    public class DeviceExtensionAndroid : IDeviceExtension
    {
        #region IDeviceExtension implementation

        public bool CanOpenUrl(Uri uri)
        {
            var pm = Forms.Context.ApplicationContext.PackageManager;
            var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(uri.AbsoluteUri));
            if (intent.ResolveActivity(pm) != null)
            {
                return true;
            }
            return false;
        }

        #endregion
    }
}
