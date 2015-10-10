using System;
using System.Diagnostics;
using Xamarin.Forms;
using XFStreamingAudio.Droid;
using Android.Media;
using Android.Content;
using XFStreamingAudio.Droid.Services;
using Android.OS;

[assembly: Dependency(typeof(AudioPlayerAndroid))]
namespace XFStreamingAudio.Droid
{
    public class AudioPlayerAndroid : IAudioPlayer
    {
        public AudioPlayerAndroid()
        {
        }

        #region IAudioPlayer implementation

        public void Play(Uri source)
        {
            var context = Forms.Context.ApplicationContext;
            var intent = new Intent(context, typeof(StreamingBackgroundService));
            intent.SetAction(StreamingBackgroundService.ActionPlay);
            intent.PutExtra("source", source.AbsoluteUri);
            Forms.Context.StartService(intent);
        }

        public void Stop()
        {
            var context = Forms.Context.ApplicationContext;
            var intent = new Intent(context, typeof(StreamingBackgroundService));
            intent.SetAction(StreamingBackgroundService.ActionStop);
            Forms.Context.StartService(intent);
        }

        public bool IsPlaying
        { 
            get
            {
                return ((MainActivity)Forms.Context).IsPlaying;
            }
        }

        public bool PlaybackLikelyToKeepUp
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public double DurationLoaded
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool PlaybackBufferFull
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
