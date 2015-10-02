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
            mediaPlayer = new MediaPlayer();
            await mediaPlayer.SetDataSourceAsync(Android.App.Application.Context, 
                Android.Net.Uri.Parse(source.AbsoluteUri));
            mediaPlayer.Prepare();
            mediaPlayer.Start();
        }

        public void Stop()
        {
            mediaPlayer.Stop();
            mediaPlayer.Release();
            mediaPlayer = null;
        }

        public bool IsPlaying
        {
            get
            {
                if (mediaPlayer != null)
                {
                    return mediaPlayer.IsPlaying;
                }
                else
                {
                    return false;
                }
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

