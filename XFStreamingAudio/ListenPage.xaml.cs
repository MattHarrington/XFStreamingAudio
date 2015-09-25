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
        readonly Uri source96 = new Uri("https://www.kvmr.org/aac96.m3u");
        readonly Uri source32 = new Uri("https://www.kvmr.org/aac32.m3u");
        const string playIcon = "\u25b6\uFE0E";
        const string stopIcon = "\u25a0";

        public ListenPage()
        {
            InitializeComponent();
            useHighBandwidth = Helpers.Settings.BandwidthSwitchState;
            if (useHighBandwidth)
            {
                source = source96;
            }
            else
            {
                source = source32;
            }
            playStopBtn.Clicked += OnPlayStopBtnClicked;
            diagnosticsBtn.Clicked += DiagnosticsBtn_Clicked;

            TapGestureRecognizer launchSettingsImageTGR = new TapGestureRecognizer();
            launchSettingsImageTGR.Tapped += LaunchSystemSettings;
            launchSettingsImage.GestureRecognizers.Add(launchSettingsImageTGR);

            audioPlayer = DependencyService.Get<IAudioPlayer>();
            MessagingCenter.Subscribe<Page>(this, "AudioBeginInterruption",
                OnAudioBeginInterruption);
            MessagingCenter.Subscribe<Page>(this, "AudioEndInterruption",
                OnAudioEndInterruption);
            MessagingCenter.Subscribe<Page>(this, "RemoteControlTogglePlayPause",
                OnRemoteControlTogglePlayPause);
            MessagingCenter.Subscribe<Page>(this, "RemoteControlPauseOrStop",
                OnRemoteControlPauseOrStop);
            MessagingCenter.Subscribe<Page>(this, "RemoteControlPlayOrPreviousTrackOrNextTrack",
                OnRemoteControlPlayOrPreviousTrackOrNextTrack);
            MessagingCenter.Subscribe<Page>(this, "HeadphonesUnplugged",
                OnHeadphonesUnplugged);
            MessagingCenter.Subscribe<Page>(this, "BandwidthSwitchToggled",
                OnBandwidthSwitchToggled);
            CrossConnectivity.Current.ConnectivityChanged += ConnectivityChanged;
        }

        void LaunchSystemSettings(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("app-settings:"));
        }

        void OnBandwidthSwitchToggled(object sender)
        {
            useHighBandwidth = Helpers.Settings.BandwidthSwitchState;
            if (useHighBandwidth)
            {
                source = source96;
            }
            else
            {
                source = source32;
            }
            if (audioPlayer.IsPlaying)
            {
                audioPlayer.Stop();
                audioPlayer.Play(source);
            }
        }

        void ConnectivityChanged(object sender, Connectivity.Plugin.Abstractions.ConnectivityChangedEventArgs e)
        {
            Debug.WriteLine("IsConnected: {0}", e.IsConnected);
            foreach (var connection in CrossConnectivity.Current.ConnectionTypes)
            {
                Debug.WriteLine("ConnectionType: {0}", connection);
            }
//            DisplayAlert("Connectivity Changed", "Please restart stream", "OK");
            if (!e.IsConnected)
            {
                audioPlayer.Stop();
                playStopBtn.Text = playIcon;   
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
            playStopBtn.Text = playIcon;
        }

        void OnAudioEndInterruption(object sender)
        {
            Debug.WriteLine("End audio interruption");
        }

        void OnHeadphonesUnplugged(object sender)
        {
            Debug.WriteLine("OnHeadphonesUnplugged()");
            audioPlayer.Stop();
            Device.BeginInvokeOnMainThread(() => playStopBtn.Text = playIcon);
        }

        void OnRemoteControlPlayOrPreviousTrackOrNextTrack(object sender)
        {
            Debug.WriteLine("OnRemoteControlPlayOrPreviousTrackOrNextTrack()");
            if (!audioPlayer.IsPlaying)
            {
                if (!CrossConnectivity.Current.IsConnected)
                {
                    DisplayAlert("Network Unreachable", "Check your network connection", "OK");
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

        void OnRemoteControlTogglePlayPause(object sender)
        {
            Debug.WriteLine("OnRemoteControlTogglePlayPause()");
            if (!audioPlayer.IsPlaying)
            {
                if (!CrossConnectivity.Current.IsConnected)
                {
                    DisplayAlert("Network Unreachable", "Check your network connection", "OK");
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

        void OnPlayStopBtnClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("OnPlayStopBtnClicked");
            if (!audioPlayer.IsPlaying)
            {
                if (!CrossConnectivity.Current.IsConnected)
                {
                    DisplayAlert("Network Unreachable", "Check your network connection", "OK");
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
    }
}
