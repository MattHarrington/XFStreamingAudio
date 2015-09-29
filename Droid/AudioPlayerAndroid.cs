using System;
using Xamarin.Forms;
using XFStreamingAudio.Droid;

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
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public bool IsPlaying
        {
            get
            {
                throw new NotImplementedException();
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

