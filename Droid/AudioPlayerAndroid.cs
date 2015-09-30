using System;
using System.Diagnostics;
using Xamarin.Forms;
using XFStreamingAudio.Droid;
using Android.Media;

[assembly: Dependency(typeof(AudioPlayerAndroid))]
namespace XFStreamingAudio.Droid
{
    public class AudioPlayerAndroid : IAudioPlayer
    {
        MediaPlayer mediaPlayer;

        public AudioPlayerAndroid()
        {
        }

        #region IAudioPlayer implementation

        public async void Play(Uri source)
        {
            Debug.WriteLine("AudioPlayerAndroid.Play()");
            mediaPlayer = new MediaPlayer();
//            mediaPlayer = MediaPlayer.Create(Android.App.Application.Context, 
//                Android.Net.Uri.Parse("http://live2.artoflogic.com:8190/kvmr"));
            await mediaPlayer.SetDataSourceAsync(Android.App.Application.Context, 
                Android.Net.Uri.Parse("http://live2.artoflogic.com:8190/kvmr"));
            mediaPlayer.Prepare();
            mediaPlayer.Start();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public bool IsPlaying
        {
            get
            {
                return false;
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

