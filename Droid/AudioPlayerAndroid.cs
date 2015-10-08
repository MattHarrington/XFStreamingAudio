using System;
using System.Diagnostics;
using Xamarin.Forms;
using XFStreamingAudio.Droid;
using Android.Media;
using Android.Content;
using XFStreamingAudio.Droid.Services;

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
            //MainActivity.SendAudioCommand(StreamingBackgroundService.ActionPlay);
            var context = Forms.Context.ApplicationContext;
            var intent = new Intent(context, typeof(StreamingBackgroundService));
            intent.SetAction(StreamingBackgroundService.ActionPlay);
            intent.PutExtra("source", source.AbsoluteUri);
            Forms.Context.StartService(intent);
            IsPlaying = true;
        }

        public void Stop()
        {
            var context = Forms.Context.ApplicationContext;
            var intent = new Intent(context, typeof(StreamingBackgroundService));
            Forms.Context.StopService(intent);
            IsPlaying = false;
        }

        private bool _isPlaying;

        public bool IsPlaying
        { 
            get
            {
                Debug.WriteLine("IsPlaying = {0}", _isPlaying);
                return _isPlaying;
            }

            set
            {
                _isPlaying = value;
                Debug.WriteLine("Set IsPlaying = {0}", _isPlaying);
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

