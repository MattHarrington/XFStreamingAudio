using System;
using Android.App;
using Android.Content;
using Android.Media;
using XFStreamingAudio.Droid.Services;
using Xamarin.Forms;

namespace XFStreamingAudio.Droid
{
    /// <summary>
    /// This is a simple intent receiver that is used to stop playback
    /// when audio become noisy, such as the user unplugged headphones
    /// </summary>
    [BroadcastReceiver]
    [IntentFilter(new []{ AudioManager.ActionAudioBecomingNoisy })]
    public class MusicBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action != AudioManager.ActionAudioBecomingNoisy)
                return;

            // Signal the service to stop
            var ctx = Forms.Context.ApplicationContext;
            var stopIntent = new Intent(ctx, typeof(StreamingBackgroundService));
            Forms.Context.StopService(stopIntent);
        }
    }
}
