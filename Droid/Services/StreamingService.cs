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

namespace XFStreamingAudio.Droid.Services
{
    [Service]
    [IntentFilter(new[] { ActionPlay, ActionStop })]
    public class StreamingService : Service, AudioManager.IOnAudioFocusChangeListener, IPlayerCallback
    {
        const string TAG = "KVMR";

        public const string ActionPlay = "com.xamarin.action.PLAY";
        public const string ActionStop = "com.xamarin.action.STOP";

        public bool IsPlaying { get; private set; }

        private StreamingServiceBinder binder;
        private AACPlayer player;
        private AudioManager audioManager;
        private WifiManager wifiManager;
        private WifiManager.WifiLock wifiLock;
        private MusicBroadcastReceiver headphonesUnpluggedReceiver;
        private const int NotificationId = 1;

        /// <summary>
        /// OnCreate() detects some of our managers
        /// </summary>
        public override void OnCreate()
        {
            base.OnCreate();
            //Find our audio and notificaton managers
            audioManager = (AudioManager)GetSystemService(AudioService);
            wifiManager = (WifiManager)GetSystemService(WifiService);
            headphonesUnpluggedReceiver = new MusicBroadcastReceiver();
        }

        /// <summary>
        /// Allows activities to bind to StreamingBackgroundService
        /// </summary>
        public override IBinder OnBind(Intent intent)
        {
            binder = new StreamingServiceBinder(this);
            return binder;
        }
            
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            string source = intent.GetStringExtra("source") ?? String.Empty;
            switch (intent.Action)
            {
                case ActionPlay:
                    Play(source);
                    break;
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
            UnregisterReceiver(headphonesUnpluggedReceiver);
            ReleaseWifiLock();
            var bufferingEndMessage = new BufferingEndMessage();
            MessagingCenter.Send(bufferingEndMessage, "BufferingEnd");
            Log.Debug(TAG, "UnregisterReceiver for headphones unplugged");
            var playerStoppedMessage = new PlayerStoppedMessage();
            MessagingCenter.Send(playerStoppedMessage, "PlayerStopped");
            var audioBeginInterruptionMessage = new AudioBeginInterruptionMessage();
            MessagingCenter.Send(audioBeginInterruptionMessage, "AudioBeginInterruption");
        }

        #endregion

        private void Play(string source)
        {
            Log.Debug(TAG, "StreamingBackgroundService.Play()");

            var focusResult = audioManager.RequestAudioFocus(this, Stream.Music, AudioFocus.Gain);
            Log.Debug(TAG, "StreamingBackgroundService.Play() RequestAudioFocus result: " + focusResult);
            if (focusResult == AudioFocusRequest.Granted)
            {
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

            player.Stop();

            var focusResult = audioManager.AbandonAudioFocus(this);
            Log.Debug(TAG, "StreamingBackgroundService.Stop() AbandonAudioFocus result: " + focusResult);
            if (focusResult != AudioFocusRequest.Granted)
            {
                Log.Debug(TAG, "Could not abandon audio focus");
            }
         
            StopForeground(true);
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
        }

        /// <summary>
        /// For a good user experience we should account for when audio focus has changed.
        /// There is only 1 audio output there may be several media services trying to use it so
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
                    Stop();
                    break;
                case AudioFocus.LossTransient:
                    Log.Debug(TAG, "AudioFocus.LossTransient");
                    //We have lost focus for a short time, but likely to resume so pause
                    Stop();
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
    }
}
