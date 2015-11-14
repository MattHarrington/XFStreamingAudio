using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Connectivity.Plugin;
using Xamarin.Forms;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace XFStreamingAudio
{
    public partial class ListenPage : ContentPage
    {
        IAudioPlayer audioPlayer;
        Uri source;
        //        bool useHighBandwidth;
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
                sourceLowBandwidth = new Uri("https://www.kvmr.org/json/kvmr-32-mp3.json");
                sourceHighBandwidth = new Uri("https://www.kvmr.org/json/kvmr-64-mp3.json");
            }

//            useHighBandwidth = Helpers.Settings.BandwidthSwitchState;
//            if (useHighBandwidth)
//            {
//                source = sourceHighBandwidth;
//            }
//            else
//            {
//                source = sourceLowBandwidth;
//            }

            playStopBtn.Clicked += OnPlayStopBtnClicked;

            TapGestureRecognizer launchSettingsImageTGR = new TapGestureRecognizer();
            launchSettingsImageTGR.Tapped += DisplaySettings;
            launchSettingsImage.GestureRecognizers.Add(launchSettingsImageTGR);

            audioPlayer = DependencyService.Get<IAudioPlayer>();

            MessagingCenter.Subscribe<AudioBeginInterruptionMessage>(this, "AudioBeginInterruption", 
                OnAudioBeginInterruption);
            MessagingCenter.Subscribe<HeadphonesUnpluggedMessage>(this, "HeadphonesUnplugged", 
                OnHeadphonesUnplugged);
            if (Device.OS == TargetPlatform.iOS)
            {
                MessagingCenter.Subscribe<LostStreamMessage>(this, "LostStream", 
                    OnLostStream);
                MessagingCenter.Subscribe<Page>(this, "AudioEndInterruption",
                    OnAudioEndInterruption);
                MessagingCenter.Subscribe<Page>(this, "RemoteControlTogglePlayPause",
                    OnRemoteControlTogglePlayPause);
                MessagingCenter.Subscribe<Page>(this, "RemoteControlPauseOrStop",
                    OnRemoteControlPauseOrStop);
                MessagingCenter.Subscribe<Page>(this, "RemoteControlPlayOrPreviousTrackOrNextTrack",
                    OnRemoteControlPlayOrPreviousTrackOrNextTrack);
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

        async Task<Uri> GetSource()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string response;
            Uri jspfSource;

            if (Helpers.Settings.BandwidthSwitchState == true)
            {
                jspfSource = sourceHighBandwidth;
            }
            else
            {
                jspfSource = sourceLowBandwidth;
            }
            // TODO: Check network connectivity, or check if response is valid, or both
            response = await client.GetStringAsync(jspfSource);
            JSPF jspfResponse = JsonConvert.DeserializeObject<JSPF>(response);
            return new Uri(jspfResponse.playlist.track.FirstOrDefault().location.FirstOrDefault());
        }

        void OnLostStream(LostStreamMessage obj)
        {
            Device.BeginInvokeOnMainThread(() => playStopBtn.Text = playIcon);
            Device.BeginInvokeOnMainThread(async () => await DisplayAlert("Lost Stream", "Check network connection", "OK"));
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
                if (Helpers.Settings.BandwidthSwitchState == true)
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
                if (action == highBandwidthChoice && Helpers.Settings.BandwidthSwitchState == false)
                {
                    Helpers.Settings.BandwidthSwitchState = true;
//                    source = sourceHighBandwidth;
                    if (audioPlayer.IsPlaying)
                    {
                        audioPlayer.Stop();
                        audioPlayer.Play(source);
                        playStopBtn.Text = stopIcon;
                    }
                }
                else if (action == lowBandwidthChoice && Helpers.Settings.BandwidthSwitchState == true)
                {
                    Helpers.Settings.BandwidthSwitchState = false;
//                    source = sourceLowBandwidth;
                    if (audioPlayer.IsPlaying)
                    {
                        audioPlayer.Stop();
                        audioPlayer.Play(source);
                        playStopBtn.Text = stopIcon;
                    }
                }
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
            if (Device.OS == TargetPlatform.iOS)
            {
                audioPlayer.Stop();
            }
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
            var watch = Stopwatch.StartNew();
            source = await GetSource();
            Debug.WriteLine("Source: {0}", source.AbsoluteUri);
            watch.Stop();
            Debug.WriteLine("ElapsedMilliseconds: {0}", watch.ElapsedMilliseconds);
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
