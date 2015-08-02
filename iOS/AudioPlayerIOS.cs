using System;
using System.Diagnostics;
using Xamarin.Forms;
using XFStreamingAudio.iOS;

[assembly: Dependency(typeof(AudioPlayerIOS))]
namespace XFStreamingAudio.iOS
{
    public class AudioPlayerIOS : IAudioPlayer
    {
        public AudioPlayerIOS()
        {
        }

        #region IAudioPlayer implementation

        public void Play(Uri source)
        {
            Debug.WriteLine("Start playing");
        }

        public void Stop()
        {
            Debug.WriteLine("Stop playing");
        }

        #endregion
    }
}

