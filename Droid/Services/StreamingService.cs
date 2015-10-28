using Android.App;
using System;
using Android.Content;
using Android.Media;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Xamarin.Forms;
using Android.Util;
using Android.Media.Session;
using Com.Google.Android.Exoplayer;
using Com.Google.Android.Exoplayer.Upstream;
using Com.Google.Android.Exoplayer.Extractor;
using Com.Google.Android.Exoplayer.Util;

namespace XFStreamingAudio.Droid.Services
{
    [Service]
    [IntentFilter(new[] { ActionPlay, ActionStop, ActionHeadphonesUnplugged })]
    public class StreamingService : Service, AudioManager.IOnAudioFocusChangeListener
    {
        const string TAG = "KVMR";

        public const string ActionPlay = "org.kvmr.player.action.PLAY";
        public const string ActionStop = "org.kvmr.player.action.STOP";
        public const string ActionHeadphonesUnplugged = "org.kvmr.player.action.HEADPHONES_UNPLUGGED";

        const int BUFFER_SEGMENT_SIZE = 64 * 1024;
        const int BUFFER_SEGMENT_COUNT = 256;

        public bool IsPlaying { get; private set; }

        IExoPlayer mediaPlayer;
        MediaCodecAudioTrackRenderer audioRenderer;
        StreamingServiceBinder binder;
        AudioManager audioManager;
        WifiManager wifiManager;
        WifiManager.WifiLock wifiLock;
        MusicBroadcastReceiver headphonesUnpluggedReceiver;
        const int NotificationId = 1;
        bool stopBtnWasClicked;
        string source;
        MediaSession mediaSession;
        MediaSessionCallback mediaCallback;

        /// <summary>
        /// OnCreate() detects some of our managers
        /// </summary>
        public override void OnCreate()
        {
            Log.Debug(TAG, "StreamingService.OnCreate()");
            base.OnCreate();

            audioManager = (AudioManager)GetSystemService(AudioService);
            wifiManager = (WifiManager)GetSystemService(WifiService);
            headphonesUnpluggedReceiver = new MusicBroadcastReceiver();
            if (Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                mediaSession = new MediaSession(this, "KVMRMediaSession");

                mediaCallback = new MediaSessionCallback();
                mediaCallback.OnPlayImpl = () =>
                {
                    if (IsPlaying)
                    {
                        Log.Debug(TAG, "MediaCallback stop playing");
                        Stop();
                        MessagingCenter.Send<PlayerStoppedMessage>(new PlayerStoppedMessage(), "PlayerStopped");
                    }
                    else
                    {
                        Log.Debug(TAG, "MediaCallback start playing. source: " + source ?? "null");
                        Play(source);
                        MessagingCenter.Send(new RemoteControlPlayMessage(), "RemoteControlPlay");
                    }
                };

                mediaSession.SetCallback(mediaCallback);
                mediaSession.SetFlags(MediaSessionFlags.HandlesMediaButtons |
                    MediaSessionFlags.HandlesTransportControls);

                PlaybackState state = new PlaybackState.Builder()
                .SetActions(PlaybackState.ActionPlay | PlaybackState.ActionPlayPause
                                          | PlaybackState.ActionPause | PlaybackState.ActionStop)
                .Build();
                mediaSession.SetPlaybackState(state);
                mediaSession.Active = true;
            }
        }

        /// <summary>
        /// Allows activities to bind to StreamingService
        /// </summary>
        public override IBinder OnBind(Intent intent)
        {
            binder = new StreamingServiceBinder(this);
            return binder;
        }

        [Obsolete("deprecated")]
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Debug(TAG, "OnStartCommand() intent: " + intent.Action ?? "null");
            // "source" not included in this intent, and already set
            if (String.IsNullOrEmpty(source))
            {
                source = intent.GetStringExtra("source");
            }
            switch (intent.Action)
            {
                case ActionPlay:
                    Play(source);
                    break;
                case ActionHeadphonesUnplugged:
                case ActionStop:
                    Stop();
                    break;
            }

            return StartCommandResult.NotSticky;
        }

        private void Play(string source)
        {
            Log.Debug(TAG, "StreamingBackgroundService.Play()");

            var focusResult = audioManager.RequestAudioFocus(this, Stream.Music, AudioFocus.Gain);
            Log.Debug(TAG, "StreamingBackgroundService.Play() RequestAudioFocus result: " + focusResult);
            if (focusResult == AudioFocusRequest.Granted)
            {
                if (mediaPlayer == null)
                { 
                    mediaPlayer = ExoPlayerFactory.NewInstance(1);
                }

                RegisterReceiver(headphonesUnpluggedReceiver, new IntentFilter(AudioManager.ActionAudioBecomingNoisy));
                Log.Debug(TAG, "RegisterReceiver for headphones unplugged");

                String userAgent = Util.GetUserAgent(this, "ExoPlayerDemo");
                Android.Net.Uri soundString = Android.Net.Uri.Parse(source);
                var allocator = new DefaultAllocator(BUFFER_SEGMENT_SIZE);
                var dataSource = new DefaultUriDataSource(this, userAgent);
                ExtractorSampleSource sampleSource = new ExtractorSampleSource(soundString, dataSource, allocator,
                                                         BUFFER_SEGMENT_COUNT * BUFFER_SEGMENT_SIZE);
                audioRenderer = new MediaCodecAudioTrackRenderer(sampleSource);
                mediaPlayer.Prepare(audioRenderer);
                mediaPlayer.PlayWhenReady = true;
                IsPlaying = true;
                AquireWifiLock();
                StartForeground();
            }
        }

        /// <summary>
        /// When we start on the foreground we will present a notification to the user
        /// When they press the notification it will take them to the main page so they can control the music
        /// </summary>
        private void StartForeground()
        {
            var pendingIntent = PendingIntent.GetActivity(ApplicationContext, 0,
                                    new Intent(ApplicationContext, typeof(MainActivity)),
                                    PendingIntentFlags.UpdateCurrent);

            var notification = new Notification
            {
                TickerText = new Java.Lang.String("KVMR streaming started"),
                Icon = Resource.Drawable.AndroidNotificationIcon
            };
            notification.Flags |= NotificationFlags.OngoingEvent;
            notification.SetLatestEventInfo(ApplicationContext, "KVMR",
                "Nevada City, California", pendingIntent);
            StartForeground(NotificationId, notification);
        }

        private void Stop()
        {
            Log.Debug(TAG, "StreamingBackgroundService.Stop()");
            mediaPlayer.Stop();
            IsPlaying = false;
            StopForeground(true);

            // Unregister receiver for headphones unplugged
            Log.Debug(TAG, "UnregisterReceiver for headphones unplugged");
            UnregisterReceiver(headphonesUnpluggedReceiver);

            // Abandon audio focus
            var focusResult = audioManager.AbandonAudioFocus(this);
            Log.Debug(TAG, "StreamingBackgroundService.Stop() AbandonAudioFocus result: " + focusResult);
            if (focusResult != AudioFocusRequest.Granted)
            {
                Log.Debug(TAG, "Could not abandon audio focus");
            }

            ReleaseWifiLock();
        }

        /// <summary>
        /// Lock the wifi so we can still stream under lock screen
        /// </summary>
        private void AquireWifiLock()
        {
            if (wifiLock == null)
            {
                wifiLock = wifiManager.CreateWifiLock(WifiMode.Full, "kvmr_wifi_lock");
            } 
            wifiLock.Acquire();
        }

        /// <summary>
        /// This will release the wifi lock if it is no longer needed
        /// </summary>
        private void ReleaseWifiLock()
        {
            if (wifiLock == null)
            {
                return;
            }
            wifiLock.Release();
            wifiLock = null;
        }

        /// <summary>
        /// Properly cleanup of your player by releasing resources
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
            Log.Debug(TAG, "StreamingBackgroundService.OnDestroy()");
            if (mediaPlayer != null)
            {
                mediaPlayer.Stop();
            }
            mediaSession.Release();
        }

        /// <summary>
        /// For a good user experience we should account for when audio focus has changed.
        /// There is only one audio output there may be several media services trying to use it so
        /// we should act correctly based on this.  "duck" to be quiet and when we gain go full.
        /// All applications are encouraged to follow this, but are not enforced.
        /// </summary>
        /// <param name="focusChange"></param>
        public void OnAudioFocusChange(AudioFocus focusChange)
        {
            Log.Debug(TAG, "StreamingBackgroundService.OnAudioFocusChange() focusChange = " + focusChange);
            switch (focusChange)
            {
                case AudioFocus.Gain:
                    Log.Debug(TAG, "AudioFocus.Gain");
                    if (mediaPlayer == null)
                    { 
                        mediaPlayer = ExoPlayerFactory.NewInstance(1);
                    }
                    if (!IsPlaying)
                    {
                        Play(source);
                        MessagingCenter.Send<PlayerStartedMessage>(new PlayerStartedMessage(), "PlayerStarted");
                    }
                    mediaPlayer.SendMessage(audioRenderer, MediaCodecAudioTrackRenderer.MsgSetVolume, 1.0f);
                    break;
                case AudioFocus.Loss:
                    Log.Debug(TAG, "AudioFocus.Loss");
                    Stop();
                    MessagingCenter.Send<PlayerStoppedMessage>(new PlayerStoppedMessage(), "PlayerStopped");
                    break;
                case AudioFocus.LossTransient:
                    Log.Debug(TAG, "AudioFocus.LossTransient");
                    // We have lost focus for a short time, but likely to resume
                    Stop();
                    MessagingCenter.Send<PlayerStoppedMessage>(new PlayerStoppedMessage(), "PlayerStopped");
                    break;
                case AudioFocus.LossTransientCanDuck:
                    // We have lost focus but should till play at a muted 10% volume
                    Log.Debug(TAG, "AudioFocus.LossTransientCanDuck");
                    if (IsPlaying)
                    {
                        mediaPlayer.SendMessage(audioRenderer, MediaCodecAudioTrackRenderer.MsgSetVolume, 0.1f);
                    }
                    break;
            }
        }

        class MediaSessionCallback : MediaSession.Callback
        {
            public Action OnPlayImpl { get; set; }

            public override void OnPlay()
            {
//                base.OnPlay();
                Log.Debug(TAG, "MediaSessionCallback.OnPlay()");
                OnPlayImpl();
            }
        }
    }
}
