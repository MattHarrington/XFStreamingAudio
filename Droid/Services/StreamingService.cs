using Android.App;
using System;
using Android.Content;
using Android.Media;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Xamarin.Forms;
using Android.Util;
using Com.Spoledge.Aacdecoder;
using Android.Media.Session;

namespace XFStreamingAudio.Droid.Services
{
    [Service]
    [IntentFilter(new[] { ActionPlay, ActionStop, ActionHeadphonesUnplugged })]
    public class StreamingService : Service, AudioManager.IOnAudioFocusChangeListener, IPlayerCallback
    {
        const string TAG = "KVMR";

        public const string ActionPlay = "org.kvmr.player.action.PLAY";
        public const string ActionStop = "org.kvmr.player.action.STOP";
        public const string ActionHeadphonesUnplugged = "org.kvmr.player.action.HEADPHONES_UNPLUGGED";

        public bool IsPlaying { get; private set; }

        StreamingServiceBinder binder;
        AACPlayer player;
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
                    }
                    else
                    {
                        Log.Debug(TAG, "MediaCallback start playing. source: " + source ?? "null");
                        Play(source);
                        var message = new RemoteControlPlayMessage();
                        MessagingCenter.Send(message, "RemoteControlPlay");
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
            if (intent.Action == ActionHeadphonesUnplugged)
            {
                Log.Debug(TAG, "Headphones unplugged.  source: " + source);
            }
            else
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

        #region IPlayerCallback implementation

        public void PlayerAudioTrackCreated(Android.Media.AudioTrack track)
        {
        }

        public void PlayerException(Java.Lang.Throwable ex)
        {
            Log.Debug(TAG, "AACPlayerCallback.PlayerException: " + ex);  
        }

        public void PlayerMetadata(string key, string value)
        {
        }

        /**
        * This method is called periodically by PCMFeed.
        *
        * @param isPlaying false means that the PCM data are being buffered,
        *          but the audio is not playing yet
        *
        * @param audioBufferSizeMs the buffered audio data expressed in milliseconds of playing
        * @param audioBufferCapacityMs the total capacity of audio buffer expressed in milliseconds of playing
        */
        public void PlayerPCMFeedBuffer(bool isPlaying, int audioBufferSizeMs, int audioBufferCapacityMs)
        {
//            Log.Debug(TAG, "AACPlayerCallback.PlayerPCMFeedBuffer() isPlaying = " + isPlaying +
//                ", audioBufferSizeMs = " + audioBufferSizeMs + ", audioBufferCapacityMs = " + audioBufferCapacityMs);
//            if (!isPlaying)
//            {
//                var message = new BufferingStartMessage();
//                MessagingCenter.Send(message, "BufferingStart");
//            }
//            else
//            {
//                var message = new BufferingEndMessage();
//                MessagingCenter.Send(message, "BufferingEnd");
//            }
        }

        public void PlayerStarted()
        {
            Log.Debug(TAG, "AACPlayerCallback.PlayerStarted()");    
            IsPlaying = true;
            RegisterReceiver(headphonesUnpluggedReceiver, new IntentFilter(AudioManager.ActionAudioBecomingNoisy));
            Log.Debug(TAG, "RegisterReceiver for headphones unplugged");
            var playerStartedMessage = new PlayerStartedMessage();
            MessagingCenter.Send(playerStartedMessage, "PlayerStarted");
        }

        public void PlayerStopped(int perf)
        {
            Log.Debug(TAG, "AACPlayerCallback.PlayerStopped(). perf: " + perf + "%");
            player = null;
            IsPlaying = false;
            StopForeground(true);

            // Unregister receiver for headphones unplugged
            Log.Debug(TAG, "UnregisterReceiver for headphones unplugged");
            UnregisterReceiver(headphonesUnpluggedReceiver);

            ReleaseWifiLock();

            // Hides ActivityIndicator on ListenPage
            var bufferingEndMessage = new BufferingEndMessage();
            MessagingCenter.Send(bufferingEndMessage, "BufferingEnd");

            // Abandon audio focus
            var focusResult = audioManager.AbandonAudioFocus(this);
            Log.Debug(TAG, "StreamingBackgroundService.Stop() AbandonAudioFocus result: " + focusResult);
            if (focusResult != AudioFocusRequest.Granted)
            {
                Log.Debug(TAG, "Could not abandon audio focus");
            }

            // Enables playStopBtn on ListenPage
            var playerStoppedMessage = new PlayerStoppedMessage();
            MessagingCenter.Send(playerStoppedMessage, "PlayerStopped");

            // Sets playStopBtn to play icon
            var audioBeginInterruptionMessage = new AudioBeginInterruptionMessage();
            MessagingCenter.Send(audioBeginInterruptionMessage, "AudioBeginInterruption");

            if (!stopBtnWasClicked)
            {
                // Stop button was not clicked.  Callback called because stream was lost.
                Log.Debug(TAG, "Send LostStream message");
                var lostStreamMessage = new LostStreamMessage();
                MessagingCenter.Send(lostStreamMessage, "LostStream");
            }
        }

        #endregion

        private void Play(string source)
        {
            Log.Debug(TAG, "StreamingBackgroundService.Play()");

            var focusResult = audioManager.RequestAudioFocus(this, Stream.Music, AudioFocus.Gain);
            Log.Debug(TAG, "StreamingBackgroundService.Play() RequestAudioFocus result: " + focusResult);
            if (focusResult == AudioFocusRequest.Granted)
            {
                stopBtnWasClicked = false;
                player = new AACPlayer(this);
                player.PlayAsync(source);
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
            stopBtnWasClicked = true;
            player.Stop();
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
            if (player != null)
            {
                player.Stop();
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
                    if (player == null)
                    {
                       
                    }

                    if (!IsPlaying)
                    {
                        
                    }

//                    player.SetVolume(1.0f, 1.0f);  // Turn it up!
                    break;
                case AudioFocus.Loss:
                    Log.Debug(TAG, "AudioFocus.Loss");
                    player.Stop();
                    break;
                case AudioFocus.LossTransient:
                    Log.Debug(TAG, "AudioFocus.LossTransient");
                    //We have lost focus for a short time, but likely to resume so pause
                    player.Stop();
                    break;
                case AudioFocus.LossTransientCanDuck:
                    //We have lost focus but should till play at a muted 10% volume
                    Log.Debug(TAG, "AudioFocus.LossTransientCanDuck");
                    if (IsPlaying)
                    {
//                        player.SetVolume(.1f, .1f);//turn it down!

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

            public override void OnCommand(string command, Bundle args, ResultReceiver cb)
            {
                base.OnCommand(command, args, cb);
                Log.Debug(TAG, "MediaSessionCallback OnCommand");
            }
        }
    }
}
