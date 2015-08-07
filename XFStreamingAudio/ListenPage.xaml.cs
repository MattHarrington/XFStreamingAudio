using System;
using System.Collections.Generic;

using Xamarin.Forms;
using System.Diagnostics;

namespace XFStreamingAudio
{
    public partial class ListenPage : ContentPage
    {
        Button playStopBtn;
        IAudioPlayer audioPlayer;
        readonly Uri source = new Uri("http://live2.artoflogic.com:8190/kvmr");
        //        readonly Uri source = new Uri("http://misc.artoflogic.com/302");

        public ListenPage()
        {
            playStopBtn = new Button { Text = "Play", FontSize = 32 };
            Title = "Listen";
            Content = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                        {
                            new Label
                            {
                                XAlign = TextAlignment.Center,
                                FontSize = 48,
                                Text = "KVMR"
                            },
                            playStopBtn
                        }
                    };
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            audioPlayer = DependencyService.Get<IAudioPlayer>();
            playStopBtn.Clicked += OnPlayStopBtnClicked;
            MessagingCenter.Subscribe<MainPage>(this, "AudioInterrupted", (sender) =>
                {
                    Debug.WriteLine("Audio was interrupted");
                    playStopBtn.Text = "Play";
                });
            MessagingCenter.Subscribe<MainPage>(this, "RemoteControlTogglePlayPause", 
                OnRemoteControlTogglePlayPause);
            MessagingCenter.Subscribe<MainPage>(this, "RemoteControlPauseOrStop", 
                OnRemoteControlPauseOrStop);
            MessagingCenter.Subscribe<MainPage>(this, "RemoteControlPlayOrPreviousTrackOrNextTrack", 
                OnRemoteControlPlayOrPreviousTrackOrNextTrack);
            MessagingCenter.Subscribe<MainPage>(this, "HeadphonesUnplugged", 
                OnHeadphonesUnplugged);
        }

        void OnHeadphonesUnplugged(object sender)
        {
            Debug.WriteLine("Headphones unplugged");
            audioPlayer.Stop();
            Device.BeginInvokeOnMainThread(() => playStopBtn.Text = "Play");
        }

        void OnRemoteControlPlayOrPreviousTrackOrNextTrack(object sender)
        {
            if (!audioPlayer.IsPlaying)
            {
                audioPlayer.Play(source);
                playStopBtn.Text = "Stop";
            }
        }

        void OnRemoteControlPauseOrStop(object sender)
        {
            audioPlayer.Stop();
            playStopBtn.Text = "Play";
        }

        void OnRemoteControlTogglePlayPause(object sender)
        {
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

