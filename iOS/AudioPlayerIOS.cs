using System;
using System.Diagnostics;
using Xamarin.Forms;
using XFStreamingAudio.iOS;
using AVFoundation;
using MediaPlayer;
using Foundation;
using System.Collections.Generic;

[assembly: Dependency(typeof(AudioPlayerIOS))]
namespace XFStreamingAudio.iOS
{
    public class AudioPlayerIOS : IAudioPlayer
    {
        AVPlayer avPlayer;

        public AudioPlayerIOS()
        {
            AVAudioSession audioSession = AVAudioSession.SharedInstance();
            audioSession.SetCategory(AVAudioSessionCategory.Playback);
            audioSession.BeginInterruption += AudioSession_BeginInterruption;
            audioSession.EndInterruption += AudioSession_EndInterruption;
            audioSession.SetActive(true);

            MPNowPlayingInfo nowPlayingInfo = new MPNowPlayingInfo();
            nowPlayingInfo.AlbumTitle = "California";
            nowPlayingInfo.Artist = "Nevada City";
            nowPlayingInfo.Title = "KVMR";
            MPNowPlayingInfoCenter.DefaultCenter.NowPlaying = nowPlayingInfo;

            // Not clear if these are necessary.  Commenting them out
            // seems to have no effect.
            MPRemoteCommandCenter rcc = MPRemoteCommandCenter.Shared;
            rcc.SeekBackwardCommand.Enabled = false;
            rcc.SeekForwardCommand.Enabled = false;
            rcc.NextTrackCommand.Enabled = false;
            rcc.PreviousTrackCommand.Enabled = false;
            rcc.SkipBackwardCommand.Enabled = false;
            rcc.SkipForwardCommand.Enabled = false;

            // You must enable a command so that others can be disabled?
            // See http://stackoverflow.com/a/28925369.
            rcc.PlayCommand.Enabled = true;
            rcc.PlayCommand.AddTarget(arg =>
                {
                    return MPRemoteCommandHandlerStatus.Success;
                });
        }

        void AudioSession_BeginInterruption(object sender, EventArgs e)
        {
            Page mp = Xamarin.Forms.Application.Current.MainPage;
            var currentPage = ((TabbedPage)mp).CurrentPage;
            MessagingCenter.Send<Page>((Page)currentPage, "AudioBeginInterruption");
            avPlayer?.Dispose();  // will be null if other audio source starts before user has hit play
        }

        void AudioSession_EndInterruption(object sender, EventArgs e)
        {
            Page mp = Xamarin.Forms.Application.Current.MainPage;
            var currentPage = ((TabbedPage)mp).CurrentPage;
            MessagingCenter.Send<Page>((Page)currentPage, "AudioEndInterruption");
        }

        #region IAudioPlayer implementation

        public bool IsPlaying
        {
            get
            {
                if (avPlayer?.Rate > 0 && avPlayer.Error == null)
                {
                    // player thinks it's playing. NB: Rate can be > 0 even when it's not playing,
                    // e.g. if source cannot be reached.
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void Play(Uri source)
        {
            Debug.WriteLine("Start playing");
            AVAsset asset = AVAsset.FromUrl(source);
            AVPlayerItem playerItem = new AVPlayerItem(asset);
            avPlayer = new AVPlayer(playerItem);
            avPlayer.Play();
        }

        public void Stop()
        {
            Debug.WriteLine("Stop playing");
            avPlayer?.Pause();
            avPlayer?.Dispose();
        }


        public double DurationLoaded
        {
            get
            {
                NSValue[] ranges = avPlayer?.CurrentItem.LoadedTimeRanges;
                double start = ranges[0].CMTimeRangeValue.Start.Seconds;
                double duration = ranges[0].CMTimeRangeValue.Duration.Seconds;
                double currentTime = avPlayer.CurrentItem.CurrentTime.Seconds;
                Debug.WriteLine("start = {0}, duration = {1}, currentTime = {2}",
                    start, duration, currentTime);
                return Math.Round(start + duration - currentTime, 2);
            }
        }

        public bool PlaybackLikelyToKeepUp
        {
            get
            {
                return avPlayer.CurrentItem.PlaybackLikelyToKeepUp;
            }
        }

        public bool PlaybackBufferFull
        {
            get
            {
                return avPlayer.CurrentItem.PlaybackBufferFull;
            }
        }

        #endregion
    
    }
}
