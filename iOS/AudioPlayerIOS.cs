using System;
using System.Diagnostics;
using Xamarin.Forms;
using XFStreamingAudio.iOS;
using AVFoundation;
using MediaPlayer;
using Foundation;
using System.Collections.Generic;
using Xamarin;
using AI.XamarinSDK.Abstractions;

[assembly: Dependency(typeof(AudioPlayerIOS))]
namespace XFStreamingAudio.iOS
{
    public class AudioPlayerIOS : NSObject, IAudioPlayer
    {
        AVPlayer avPlayer;
        //        NSError error;

        public AudioPlayerIOS()
        {
            AVAudioSession audioSession = AVAudioSession.SharedInstance();
            audioSession.SetCategory(AVAudioSessionCategory.Playback);
            audioSession.BeginInterruption += OnAudioSessionBeginInterruption;
            audioSession.EndInterruption += OnAudioSessionEndInterruption;
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

        void OnAudioSessionBeginInterruption(object sender, EventArgs e)
        {
            MessagingCenter.Send<AudioBeginInterruptionMessage>(new AudioBeginInterruptionMessage(),
                "AudioBeginInterruption");
            avPlayer?.Dispose();  // will be null if other audio source starts before user has hit play
        }

        void OnAudioSessionEndInterruption(object sender, EventArgs e)
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

            playerItem.AddObserver(observer: this,
                keyPath: new NSString("playbackBufferEmpty"),
                options: NSKeyValueObservingOptions.New,
                context: IntPtr.Zero);
            playerItem.AddObserver(observer: this,
                keyPath: new NSString("playbackLikelyToKeepUp"),
                options: NSKeyValueObservingOptions.New,
                context: IntPtr.Zero);
            playerItem.AddObserver(observer: this,
                keyPath: new NSString("status"),
                options: NSKeyValueObservingOptions.New,
                context: IntPtr.Zero);
            playerItem.AddObserver(observer: this,
                keyPath: new NSString("error"),
                options: NSKeyValueObservingOptions.New,
                context: IntPtr.Zero);

            avPlayer = new AVPlayer(playerItem);
            avPlayer.Play();

            AVAudioSession audioSession = AVAudioSession.SharedInstance();
            Debug.WriteLine("IOBufferDuration = {0}", audioSession.IOBufferDuration);
            Debug.WriteLine("preferredIOBufferDuration = {0}", audioSession.PreferredIOBufferDuration);
        }

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
//            var str = String.Format ("The {0} property on {1}, the change is: {2}", 
//                keyPath, ofObject, change.Description);
//            Debug.WriteLine (str);
            switch (keyPath)
            {
                case "playbackBufferEmpty":
                    if (change.ValueForKeyPath(new NSString("new")).Description == "1")
                    {
                        Debug.WriteLine("playbackBufferEmpty == {0}", true);
                    }
                    break;
                case "playbackLikelyToKeepUp":
                    if (change.ValueForKeyPath(new NSString("new")).Description == "0")
                    {
                        Debug.WriteLine("playbackLikelyToKeepUp == {0}", false);
                        Dictionary<string, string> playbackLikelyToKeepUpProperties = new Dictionary<string, string>();
                        playbackLikelyToKeepUpProperties.Add("DateTime.UtcNow", DateTime.UtcNow.ToString());
                        playbackLikelyToKeepUpProperties.Add("AVPlayer.Status", avPlayer.Status.ToString());
                        playbackLikelyToKeepUpProperties.Add("AVPlayerItem.Status", 
                            avPlayer.CurrentItem.Status.ToString());
                        TelemetryManager.TrackEvent("PlaybackNotLikelyToKeepUp", playbackLikelyToKeepUpProperties);
                    }
                    break;
                case "status":
//                    TelemetryManager.TrackEvent("Playback status: " + avPlayer.Status);
                    if (avPlayer.Status == AVPlayerStatus.ReadyToPlay)
                    {
                        MessagingCenter.Send(new PlayerStartedMessage(), "PlayerStarted");
                    }
                    else if (avPlayer.Status == AVPlayerStatus.Failed)
                    {
                        Dictionary<string, string> statusProperties = new Dictionary<string, string>();
                        statusProperties.Add("DateTime.UtcNow", DateTime.UtcNow.ToString());
                        statusProperties.Add("AVPlayer.Status", avPlayer.Status.ToString());
                        statusProperties.Add("AVPlayerItem.Status", avPlayer.CurrentItem.Status.ToString());
                        TelemetryManager.TrackEvent("PlaybackFailed", statusProperties);
                    }
                    break;
                case "error":
                    Debug.WriteLine("Playback error: {0}", avPlayer.Error);
                    Dictionary<string, string> errorProperties = new Dictionary<string, string>();
                    errorProperties.Add("DateTime.UtcNow", DateTime.UtcNow.ToString());
                    errorProperties.Add("AVPlayer.Error", avPlayer.Error.ToString());
                    errorProperties.Add("AVPlayerItem.Error", avPlayer.CurrentItem.Error.ToString());
                    TelemetryManager.TrackEvent("PlaybackError", errorProperties);
                    break;
                default:
                    break;
            }
        }

        public void Stop()
        {
            Debug.WriteLine("Stop playing");

            avPlayer?.CurrentItem.RemoveObserver(observer: this, keyPath: new NSString("playbackBufferEmpty"));
            avPlayer?.CurrentItem.RemoveObserver(observer: this, keyPath: new NSString("playbackLikelyToKeepUp"));
            avPlayer?.CurrentItem.RemoveObserver(observer: this, keyPath: new NSString("status"));
            avPlayer?.CurrentItem.RemoveObserver(observer: this, keyPath: new NSString("error"));

            avPlayer?.Pause();
            avPlayer?.Dispose();
            var playerStoppedMessage = new PlayerStoppedMessage();
            MessagingCenter.Send(playerStoppedMessage, "PlayerStopped");
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
