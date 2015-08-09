using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;
using Connectivity.Plugin;
using System.Threading.Tasks;

namespace XFStreamingAudio
{
    public partial class ListenPage : ContentPage
    {
        IAudioPlayer audioPlayer;
        readonly Uri source = new Uri("http://live2.artoflogic.com:8190/kvmr");
        //        readonly Uri source = new Uri("http://misc.artoflogic.com/302");
        const string playIcon = "\u25b6\uFE0E";
        const string stopIcon = "\u25a0";

        public ListenPage()
        {
            InitializeComponent();
            playStopBtn.Clicked += OnPlayStopBtnClicked;
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
        }

        void OnAudioBeginInterruption(object sender)
        {
            Debug.WriteLine("Begin audio interruption");
            playStopBtn.Text = playIcon;
        }

        void OnAudioEndInterruption(object sender)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                DisplayAlert("Network Unreachable", "Check your network connection", "OK");
                return;
            }
            Debug.WriteLine("End audio interruption");
            audioPlayer.Play(source);
            playStopBtn.Text = stopIcon;
        }

        protected override void OnAppearing()
        {
            Debug.WriteLine("ListenPage.OnAppearing()");
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
