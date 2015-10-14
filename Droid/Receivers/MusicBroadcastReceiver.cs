using System;
using Android.App;
using Android.Content;
using Android.Media;
using XFStreamingAudio.Droid.Services;
using Xamarin.Forms;
using Android.Util;

namespace XFStreamingAudio.Droid
{
    /// <summary>
    /// This is a simple intent receiver that is used to stop playback
    /// when audio become noisy, such as the user unplugged headphones
    /// </summary>
    public class MusicBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Log.Debug("KVMR", "MusicBroadcastReceiver.OnReceive() intent.Action = " + intent.Action);
            if (intent.Action != AudioManager.ActionAudioBecomingNoisy)
                return;

            // Signal the service to stop
            var stopIntent = new Intent(context, typeof(StreamingBackgroundService));
            stopIntent.SetAction(StreamingBackgroundService.ActionStop);
            Forms.Context.StartService(stopIntent);
            Page mp = Xamarin.Forms.Application.Current.MainPage;
            var currentPage = ((TabbedPage)mp).CurrentPage;
            MessagingCenter.Send<Page>((Page)currentPage, "HeadphonesUnplugged");
        }
    }
}
