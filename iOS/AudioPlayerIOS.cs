using System;
using System.Diagnostics;
using Xamarin.Forms;
using XFStreamingAudio.iOS;
using AVFoundation;
using MediaPlayer;
using Foundation;
using System.Collections.Generic;
using Xamarin;
//using AI.XamarinSDK.Abstractions;

[assembly: Dependency(typeof(AudioPlayerIOS))]
namespace XFStreamingAudio.iOS
{
    public class AudioPlayerIOS : NSObject, IAudioPlayer
    {
        AVPlayer avPlayer;
        //        NSError error;
        readonly IntPtr itemContext = new IntPtr(1);
        readonly IntPtr playerContext = new IntPtr(2);

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

            NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.ItemFailedToPlayToEndTimeNotification, 
                (notification) =>
                {
                    Console.WriteLine("Received AVPlayerItem.ItemFailedToPlayToEndTimeNotification", notification);
                    Dictionary<string, string> itemFailedProperties = new Dictionary<string, string>();
                    itemFailedProperties.Add("DateTime.UtcNow", DateTime.UtcNow.ToString());
                    itemFailedProperties.Add("AVPlayer.Status", avPlayer?.Status.ToString());
                    itemFailedProperties.Add("AVPlayerItem.Status", avPlayer?.CurrentItem?.Status.ToString());
                    itemFailedProperties.Add("AVPlayer.Error", avPlayer?.Error?.ToString() ?? "isNull");
                    itemFailedProperties.Add("AVPlayerItem.Error", 
                        avPlayer?.CurrentItem?.Error?.ToString() ?? "isNull");
                    //TelemetryManager.TrackEvent("ItemFailedToPlayToEndTime", itemFailedProperties);
                    Stop();
                    MessagingCenter.Send<LostStreamMessage>(new LostStreamMessage(), "LostStream");
                });
            NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.PlaybackStalledNotification, 
                (notification) =>
                {
                    Console.WriteLine("Received AVPlayerItem.PlaybackStalledNotification", notification);
                    Dictionary<string, string> playbackStalledProperties = new Dictionary<string, string>();
                    playbackStalledProperties.Add("DateTime.UtcNow", DateTime.UtcNow.ToString());
                    playbackStalledProperties.Add("AVPlayer.Status", avPlayer?.Status.ToString());
                    playbackStalledProperties.Add("AVPlayerItem.Status", avPlayer?.CurrentItem?.Status.ToString());
                    playbackStalledProperties.Add("AVPlayer.Error", avPlayer?.Error?.ToString() ?? "isNull");
                    playbackStalledProperties.Add("AVPlayerItem.Error", 
                        avPlayer?.CurrentItem?.Error?.ToString() ?? "isNull");
                    //TelemetryManager.TrackEvent("PlaybackStalled", playbackStalledProperties);
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
            Debug.WriteLine("Start playing " + source);
            AVAsset asset = AVAsset.FromUrl(source);
            AVPlayerItem playerItem = new AVPlayerItem(asset);
            avPlayer = new AVPlayer(playerItem);

//            playerItem.AddObserver(observer: this,
//                keyPath: new NSString("playbackLikelyToKeepUp"),
//                options: NSKeyValueObservingOptions.New,
//                context: itemContext);
//            playerItem.AddObserver(observer: this,
//                keyPath: new NSString("status"),
//                options: NSKeyValueObservingOptions.New,
//                context: itemContext);
//            avPlayer.AddObserver(observer: this,
//                keyPath: new NSString("status"),
//                options: NSKeyValueObservingOptions.New,
//                context: playerContext);

            avPlayer.Play();
        }

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            switch (keyPath)
            {
                case "playbackLikelyToKeepUp":
                    if (change.ValueForKeyPath(new NSString("new")).Description == "0")
                    {
                        Debug.WriteLine("playbackLikelyToKeepUp == {0}", false);
                        Dictionary<string, string> playbackLikelyToKeepUpProperties = new Dictionary<string, string>();
                        playbackLikelyToKeepUpProperties.Add("DateTime.UtcNow", DateTime.UtcNow.ToString());
                        playbackLikelyToKeepUpProperties.Add("AVPlayer.Status", avPlayer.Status.ToString());
                        playbackLikelyToKeepUpProperties.Add("AVPlayerItem.Status", 
                            avPlayer.CurrentItem.Status.ToString());
                        //TelemetryManager.TrackEvent("PlaybackNotLikelyToKeepUp", playbackLikelyToKeepUpProperties);
                    }
                    break;
                case "status":
                    if (avPlayer.Status == AVPlayerStatus.Failed)
                    {
                        Dictionary<string, string> statusProperties = new Dictionary<string, string>();
                        statusProperties.Add("DateTime.UtcNow", DateTime.UtcNow.ToString());
                        statusProperties.Add("AVPlayer.Status", avPlayer.Status.ToString());
                        statusProperties.Add("AVPlayerItem.Status", avPlayer.CurrentItem.Status.ToString());
                        statusProperties.Add("AVPlayerItem.Error", avPlayer.CurrentItem.Error.ToString());
                        statusProperties.Add("AVPlayer.Error", avPlayer.Error.ToString());
                        statusProperties.Add("Context", context.ToString());
                        //TelemetryManager.TrackEvent("AVPlayerStatus.Failed", statusProperties);
                    }
                    break;
                default:
                    break;
            }
        }

        public void Stop()
        {
            Debug.WriteLine("Stop playing");

//            avPlayer?.CurrentItem.RemoveObserver(observer: this, keyPath: new NSString("playbackLikelyToKeepUp"));
//            avPlayer?.CurrentItem.RemoveObserver(observer: this, keyPath: new NSString("status"));
//            avPlayer?.RemoveObserver(observer: this, keyPath: new NSString("status"));

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
