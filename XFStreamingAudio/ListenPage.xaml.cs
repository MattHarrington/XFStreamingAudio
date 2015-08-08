using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;

namespace XFStreamingAudio
{
    public partial class ListenPage : ContentPage
    {
        IAudioPlayer audioPlayer;
        readonly Uri source = new Uri("http://live2.artoflogic.com:8190/kvmr");
        //        readonly Uri source = new Uri("http://misc.artoflogic.com/302");

        public ListenPage()
        {
            InitializeComponent();
            playStopBtn.Clicked += OnPlayStopBtnClicked;
            audioPlayer = DependencyService.Get<IAudioPlayer>();
            MessagingCenter.Subscribe<Page>(this, "AudioInterrupted", (sender) =>
                {
                    Debug.WriteLine("Audio was interrupted");
                    playStopBtn.Text = "Play";
                });
            MessagingCenter.Subscribe<Page>(this, "RemoteControlTogglePlayPause", 
                OnRemoteControlTogglePlayPause);
            MessagingCenter.Subscribe<Page>(this, "RemoteControlPauseOrStop", 
                OnRemoteControlPauseOrStop);
            MessagingCenter.Subscribe<Page>(this, "RemoteControlPlayOrPreviousTrackOrNextTrack", 
                OnRemoteControlPlayOrPreviousTrackOrNextTrack);
            MessagingCenter.Subscribe<Page>(this, "HeadphonesUnplugged", 
                OnHeadphonesUnplugged);
        }

        protected override void OnAppearing()
        {
            Debug.WriteLine("ListenPage.OnAppearing()");
        }

        void OnHeadphonesUnplugged(object sender)
        {
            Debug.WriteLine("OnHeadphonesUnplugged()");
            audioPlayer.Stop();
            Device.BeginInvokeOnMainThread(() => playStopBtn.Text = "Play");
        }

        void OnRemoteControlPlayOrPreviousTrackOrNextTrack(object sender)
        {
            Debug.WriteLine("OnRemoteControlPlayOrPreviousTrackOrNextTrack()");
            if (!audioPlayer.IsPlaying)
            {
                audioPlayer.Play(source);
                playStopBtn.Text = "Stop";
            }
        }

        void OnRemoteControlPauseOrStop(object sender)
        {
            Debug.WriteLine("OnRemoteControlPauseOrStop()");
            audioPlayer.Stop();
            playStopBtn.Text = "Play";
        }

        void OnRemoteControlTogglePlayPause(object sender)
        {
            Debug.WriteLine("OnRemoteControlTogglePlayPause()");
            if (!audioPlayer.IsPlaying)
            {
                audioPlayer.Play(source);
                playStopBtn.Text = "Stop";
            }
            else
            {
                audioPlayer.Stop();
                playStopBtn.Text = "Play";
            }
        }

        void OnPlayStopBtnClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("OnPlayStopBtnClicked");
            if (!audioPlayer.IsPlaying)
            {
                audioPlayer.Play(source);
                playStopBtn.Text = "Stop";
            }
            else
            {
                audioPlayer.Stop();
                playStopBtn.Text = "Play";
            }
        }
    }
}
