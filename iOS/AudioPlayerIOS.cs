using System;
using System.Diagnostics;
using Xamarin.Forms;
using XFStreamingAudio.iOS;
using AVFoundation;

[assembly: Dependency(typeof(AudioPlayerIOS))]
namespace XFStreamingAudio.iOS
{
    public class AudioPlayerIOS : IAudioPlayer
    {
        AVPlayer player;

        public AudioPlayerIOS()
        {
            AVAudioSession audioSession = AVAudioSession.SharedInstance();
            audioSession.SetCategory(AVAudioSessionCategory.Playback);
            audioSession.SetActive(true);
        }

        #region IAudioPlayer implementation

        public void Play(Uri source)
        {
            Debug.WriteLine("Start playing");
            player = new AVPlayer(source);
            player.Play();
        }

        public void Stop()
        {
            Debug.WriteLine("Stop playing");
        }

        #endregion
    }
}

