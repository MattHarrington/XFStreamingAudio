using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Connectivity.Plugin;
using Xamarin.Forms;
using System.Text;

namespace XFStreamingAudio
{
    public partial class ListenPage : ContentPage
    {
        IAudioPlayer audioPlayer;
        Uri source;
        bool useHighBandwidth;
        readonly Uri sourceHighBandwidth;
        readonly Uri sourceLowBandwidth;
        const string playIcon = "\u25b6\uFE0E";
        const string stopIcon = "\u25a0";

        public ListenPage()
        {
            InitializeComponent();

            if (Device.OS == TargetPlatform.iOS)
            {
                sourceHighBandwidth = new Uri("http://live.kvmr.org:8000/aac96");
                sourceLowBandwidth = new Uri("http://live.kvmr.org:8000/aac32");
            }
            else if (Device.OS == TargetPlatform.Android)
            {
                sourceHighBandwidth = new Uri("http://live2.kvmr.org:8190/kvmr");
                sourceLowBandwidth = new Uri("http://live.kvmr.org:8000/dial");
            }

            useHighBandwidth = Helpers.Settings.BandwidthSwitchState;
            if (useHighBandwidth)
            {
                source = sourceHighBandwidth;
            }
            else
            {
                source = sourceLowBandwidth;
            }

            playStopBtn.Clicked += OnPlayStopBtnClicked;

            TapGestureRecognizer launchSettingsImageTGR = new TapGestureRecognizer();
            launchSettingsImageTGR.Tapped += DisplaySettings;
            launchSettingsImage.GestureRecognizers.Add(launchSettingsImageTGR);

            audioPlayer = DependencyService.Get<IAudioPlayer>();

            MessagingCenter.Subscribe<AudioBeginInterruptionMessage>(this, "AudioBeginInterruption", 
                OnAudioBeginInterruption);
            MessagingCenter.Subscribe<Page>(this, "HeadphonesUnplugged",
                OnHeadphonesUnplugged);
            if (Device.OS == TargetPlatform.iOS)
            {
                MessagingCenter.Subscribe<Page>(this, "AudioEndInterruption",
                    OnAudioEndInterruption);
                MessagingCenter.Subscribe<Page>(this, "RemoteControlTogglePlayPause",
                    OnRemoteControlTogglePlayPause);
                MessagingCenter.Subscribe<Page>(this, "RemoteControlPauseOrStop",
                    OnRemoteControlPauseOrStop);
                MessagingCenter.Subscribe<Page>(this, "RemoteControlPlayOrPreviousTrackOrNextTrack",
                    OnRemoteControlPlayOrPreviousTrackOrNextTrack);
                MessagingCenter.Subscribe<Page>(this, "BandwidthSwitchToggled",
                    OnBandwidthSwitchToggled);
            }
            else if (Device.OS == TargetPlatform.Android)
            {
                MessagingCenter.Subscribe<BufferingStartMessage>(this, "BufferingStart", 
                    OnBufferingStart);
                MessagingCenter.Subscribe<BufferingEndMessage>(this, "BufferingEnd", 
                    OnBufferingEnd);
                MessagingCenter.Subscribe<RemoteControlPlayMessage>(this, "RemoteControlPlay", 
                    OnRemoteControlPlay);
                MessagingCenter.Subscribe<PlayerStartedMessage>(this, "PlayerStarted", 
                    OnPlayerStarted);
                MessagingCenter.Subscribe<PlayerStoppedMessage>(this, "PlayerStopped", 
                    OnPlayerStopped);
            }
        }

        void OnRemoteControlPlay(RemoteControlPlayMessage obj)
        {
            Device.BeginInvokeOnMainThread(() => playStopBtn.Text = stopIcon);
        }

        void OnPlayerStarted(PlayerStartedMessage obj)
        {
            Device.BeginInvokeOnMainThread(() => playStopBtn.Text = stopIcon);
        }

        void OnPlayerStopped(PlayerStoppedMessage obj)
        {
            Device.BeginInvokeOnMainThread(() => playStopBtn.Text = playIcon);
        }

        void OnBufferingStart(BufferingStartMessage obj)
        {
            Device.BeginInvokeOnMainThread(() => bufferingIndicator.IsRunning = true);
        }

        void OnBufferingEnd(BufferingEndMessage obj)
        {
            Device.BeginInvokeOnMainThread(() => bufferingIndicator.IsRunning = false);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (audioPlayer.IsPlaying)  // Required for Android, since playback is in background service
            {
                playStopBtn.Text = stopIcon;
            }
            else
            {
                playStopBtn.Text = playIcon;
            }
        }

        async void DisplaySettings(object sender, EventArgs e)
        {
            if (Device.OS == TargetPlatform.iOS)
            {
                Device.OpenUri(new Uri("app-settings:"));
            }
            else if (Device.OS == TargetPlatform.Android)
            {
                string highBandwidthChoice;
                string lowBandwidthChoice;
                if (source == sourceHighBandwidth)
                {
                    highBandwidthChoice = "High (currently selected)";
                    lowBandwidthChoice = "Low";
                }
                else
                {
                    highBandwidthChoice = "High";
                    lowBandwidthChoice = "Low (currently selected)";
                }

                var action = await DisplayActionSheet("Fidelity", "Cancel", null, 
                                 highBandwidthChoice, lowBandwidthChoice);
                if (action == highBandwidthChoice && source == sourceLowBandwidth)
                {
                    Helpers.Settings.BandwidthSwitchState = true;
                    source = sourceHighBandwidth;
                    if (audioPlayer.IsPlaying)
                    {
                        audioPlayer.Stop();
                        audioPlayer.Play(source);
                        playStopBtn.Text = stopIcon;
                    }
                }
                else if (action == lowBandwidthChoice && source == sourceHighBandwidth)
                {
                    Helpers.Settings.BandwidthSwitchState = false;
                    source = sourceLowBandwidth;
                    if (audioPlayer.IsPlaying)
                    {
                        audioPlayer.Stop();
                        audioPlayer.Play(source);
                        playStopBtn.Text = stopIcon;
                    }
                }
            }
        }

        void OnBandwidthSwitchToggled(object sender)
        {
            useHighBandwidth = Helpers.Settings.BandwidthSwitchState;
            if (useHighBandwidth)
            {
                source = sourceHighBandwidth;
            }
            else
            {
                source = sourceLowBandwidth;
            }
            if (audioPlayer.IsPlaying)
            {
                audioPlayer.Stop();
                audioPlayer.Play(source);
            }
        }

        void DiagnosticsBtn_Clicked(object sender, EventArgs e)
        {
            if (!audioPlayer.IsPlaying)
            {
                // Avoid null reference exceptions if not playing
                DisplayAlert("Diagnostics", "Stream not playing", "OK");
                return;
            }
            StringBuilder alertMessage = new StringBuilder();
            alertMessage.AppendLine(String.Format("Buffer: {0} sec", audioPlayer.DurationLoaded));
            alertMessage.AppendLine(String.Format("ConnectionType: {0}", 
                    CrossConnectivity.Current.ConnectionTypes.FirstOrDefault()));
            alertMessage.AppendLine(String.Format("PlaybackLikelyToKeepUp: {0}", audioPlayer.PlaybackLikelyToKeepUp));
            DisplayAlert("Diagnostics", alertMessage.ToString(), "OK");
            Debug.WriteLine("AVPlayerItem.PlaybackBufferFull = {0}", audioPlayer.PlaybackBufferFull);
            Debug.WriteLine("AVPlayerItem.PlaybackLikelyToKeepUp = {0}", audioPlayer.PlaybackLikelyToKeepUp);
            Debug.WriteLine("AVPlayerItem loaded duration = {0}", audioPlayer.DurationLoaded);
            foreach (var connection in CrossConnectivity.Current.ConnectionTypes)
            {
                Debug.WriteLine("ConnectionType: {0}", connection);
            }
        }

        void OnAudioBeginInterruption(object sender)
        {
            Debug.WriteLine("Begin audio interruption");
            Device.BeginInvokeOnMainThread(() => playStopBtn.Text = playIcon);
        }

        void OnAudioEndInterruption(object sender)
        {
            Debug.WriteLine("End audio interruption");
        }

        void OnHeadphonesUnplugged(object sender)
        {
            Debug.WriteLine("OnHeadphonesUnplugged()");
            //audioPlayer.Stop();
            Device.BeginInvokeOnMainThread(() => playStopBtn.Text = playIcon);
        }

        async void OnRemoteControlPlayOrPreviousTrackOrNextTrack(object sender)
        {
            Debug.WriteLine("OnRemoteControlPlayOrPreviousTrackOrNextTrack()");
            var sourceReachable = await CrossConnectivity.Current.IsRemoteReachable(source.Host, source.Port);
            if (!audioPlayer.IsPlaying)
            {
                if (!sourceReachable)
                {
                    await DisplayAlert("Server Unreachable", "Check your network connection", "OK");
                    return;
                }
                audioPlayer.Play(source);
                playStopBtn.Text = stopIcon;
            }
        }

        void OnRemoteControlPauseOrStop(object sender)
        {
            Debug.WriteLine("OnRemoteControlPauseOrStop()");
            audioPlayer.Stop();
            playStopBtn.Text = playIcon;
        }

        async void OnRemoteControlTogglePlayPause(object sender)
        {
            Debug.WriteLine("OnRemoteControlTogglePlayPause()");
            if (!audioPlayer.IsPlaying)
            {
                var sourceReachable = await CrossConnectivity.Current.IsRemoteReachable(source.Host, source.Port);
                if (!sourceReachable)
                {
                    await DisplayAlert("Server Unreachable", "Check your network connection", "OK");
                    return;
                }
                audioPlayer.Play(source);
                playStopBtn.Text = stopIcon;
            }
            else
            {
                audioPlayer.Stop();
                playStopBtn.Text = playIcon;
            }
        }

        async void OnPlayStopBtnClicked(object sender, EventArgs e)
        {
            if (!audioPlayer.IsPlaying)
            {
                Debug.WriteLine("Start playing");
                var sourceReachable = await CrossConnectivity.Current.IsRemoteReachable(source.Host, source.Port);
                if (!sourceReachable)
                {
                    await DisplayAlert("Server Unreachable", "Check your network connection", "OK");
                    return;
                }

                audioPlayer.Play(source);
                playStopBtn.Text = stopIcon;
            }
            else
            {
                Debug.WriteLine("Stop playing");
                audioPlayer.Stop();
                playStopBtn.Text = playIcon;
            }
        }
    }
}
