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
    public class StreamingBackgroundService : Service, AudioManager.IOnAudioFocusChangeListener
    {
        const string TAG = "KVMR";

        // Actions
        public const string ActionPlay = "com.xamarin.action.PLAY";
        public const string ActionStop = "com.xamarin.action.STOP";

        private StreamingBackgroundServiceBinder binder;
        private AACPlayer player;
        private AudioManager audioManager;
        private WifiManager wifiManager;
        private WifiManager.WifiLock wifiLock;
        private MusicBroadcastReceiver headphonesUnpluggedReceiver;
        bool isPlaying;

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
            binder = new StreamingBackgroundServiceBinder(this);
            return binder;
        }

        public bool IsPlaying
        {
            get
            {
                return isPlaying;
            }
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

        //        private void IntializePlayer()
        //        {

        //            // Tell our player to stream music
        //            player.SetAudioStreamType(Stream.Music);
        //
        //            // Wake mode will be partial to keep the CPU still running under lock screen
        //            player.SetWakeMode(ApplicationContext, WakeLockFlags.Partial);
        //
        //            // When we have prepared the song start playback
        //            player.Prepared += (sender, args) => player.Start();
        //
        //            // Apparently called if network stalls for too long
        //            player.Completion += (sender, args) =>
        //            {
        //                Log.Debug(TAG, "MediaPlayer.Completion");
        //                Stop();
        //                var message = new LostStreamMessage();
        //                MessagingCenter.Send(message, "LostStream");
        //            };
        //
        //            player.Error += (sender, args) =>
        //            {
        //                // Playback error
        //                Log.Debug(TAG, "MediaPlayer.Error - What: " + args.What + ", Extra: " + args.Extra);
        //                Stop();  // This will clean up and reset properly.
        //            };
        //
        //            player.Info += (sender, e) =>
        //            {
        //                Log.Debug(TAG, "MediaPlayback.Info - What: " + e.What + ", Extra: " + e.Extra);
        //                if (e.What == MediaInfo.BufferingStart)
        //                {
        //                    var message = new BufferingStartMessage();
        //                    MessagingCenter.Send(message, "BufferingStart");
        //                }
        //                else if (e.What == MediaInfo.BufferingEnd)
        //                {
        //                    var message = new BufferingEndMessage();
        //                    MessagingCenter.Send(message, "BufferingEnd");
        //                }
        //            };
        //        }

        private void Play(string source)
        {
            Log.Debug(TAG, "StreamingBackgroundService.Play()");

//            AACPlayerCallback playerCallback = new AACPlayerCallback();
            player = new AACPlayer();

            player.PlayAsync(source);
            isPlaying = true;

            var focusResult = audioManager.RequestAudioFocus(this, Stream.Music, AudioFocus.Gain);
            Log.Debug(TAG, "StreamingBackgroundService.Play() RequestAudioFocus result: " + focusResult);
            if (focusResult != AudioFocusRequest.Granted)
            {
                // TODO: Revert UI back to "Play", because not granted audio focus
                // TODO: Don't play if not granted focus.  (Is it possible to play on Android if not given focus?)
                return;
            }
                    
            RegisterReceiver(headphonesUnpluggedReceiver, new IntentFilter(AudioManager.ActionAudioBecomingNoisy));
            Log.Debug(TAG, "RegisterReceiver for headphones unplugged");

            AquireWifiLock();
            StartForeground();
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
            player = null;
            isPlaying = false;

            UnregisterReceiver(headphonesUnpluggedReceiver);
            Log.Debug(TAG, "UnregisterReceiver for headphones unplugged");

            var focusResult = audioManager.AbandonAudioFocus(this);
            Log.Debug(TAG, "StreamingBackgroundService.Stop() AbandonAudioFocus result: " + focusResult);
            if (focusResult != AudioFocusRequest.Granted)
            {
                Log.Debug(TAG, "Could not abandon audio focus");
            }
         
            StopForeground(true);
            ReleaseWifiLock();
            var audioBeginInterruptionMessage = new AudioBeginInterruptionMessage();
            MessagingCenter.Send(audioBeginInterruptionMessage, "AudioBeginInterruption");
            var bufferingEndMessage = new BufferingEndMessage();
            MessagingCenter.Send(bufferingEndMessage, "BufferingEnd");
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

                    if (!isPlaying)
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
                    if (isPlaying)
                    {
//                        player.SetVolume(.1f, .1f);//turn it down!
                    }
                    break;
            }
        }
    }
}
