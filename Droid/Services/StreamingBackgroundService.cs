using Android.App;
using System;
using Android.Content;
using Android.Media;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Xamarin.Forms;
using Android.Util;

namespace XFStreamingAudio.Droid.Services
{
    [Service]
    [IntentFilter(new[] { ActionPlay, ActionPause, ActionStop })]
    public class StreamingBackgroundService : Service, AudioManager.IOnAudioFocusChangeListener
    {
        const string TAG = "KVMR";

        // Actions
        public const string ActionPlay = "com.xamarin.action.PLAY";
        public const string ActionPause = "com.xamarin.action.PAUSE";
        public const string ActionStop = "com.xamarin.action.STOP";

        private StreamingBackgroundServiceBinder binder;
        private MediaPlayer player;
        private AudioManager audioManager;
        private WifiManager wifiManager;
        private WifiManager.WifiLock wifiLock;
        private bool paused;
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
            binder = new StreamingBackgroundServiceBinder(this);
            return binder;
        }

        public bool IsPlaying
        {
            get
            {
                return player.IsPlaying;
            }
        }

        public override int OnStartCommand(Intent intent, int startCommandflags, int startId)
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
                case ActionPause:
                    Pause();
                    break;
            }

            return (int)StartCommandResult.NotSticky;
        }

        private void IntializePlayer()
        {
            player = new MediaPlayer();

            // Tell our player to stream music
            player.SetAudioStreamType(Stream.Music);

            // Wake mode will be partial to keep the CPU still running under lock screen
            player.SetWakeMode(ApplicationContext, WakeLockFlags.Partial);

            // When we have prepared the song start playback
            player.Prepared += (sender, args) => player.Start();

            // When we have reached the end of the song stop ourselves, 
            // however you could signal next track here.
            player.Completion += (sender, args) =>
            {
                Log.Debug(TAG, "MediaPlayer.Completion");
                Stop();
            };

            player.Error += (sender, args) =>
            {
                // Playback error
                Log.Debug(TAG, "MediaPlayer.Error - What: " + args.What + ", Extra: " + args.Extra);
                var message = new AudioBeginInterruptionMessage();
                MessagingCenter.Send(message, "AudioBeginInterruption");
                Stop();  // This will clean up and reset properly.
            };

            player.Info += (sender, e) =>
            {
                Log.Debug(TAG, "MediaPlayback.Info - What: " + e.What + ", Extra: " + e.Extra);
                if (e.What == MediaInfo.BufferingStart)
                {
                    var message = new BufferingStart();
                    MessagingCenter.Send(message, "BufferingStart");
                }
                else if (e.What == MediaInfo.BufferingEnd)
                {
                    var message = new BufferingEnd();
                    MessagingCenter.Send(message, "BufferingEnd");
                }
            };
        }

        private async void Play(string source)
        {
            Log.Debug(TAG, "StreamingBackgroundService.Play()");
            if (paused && player != null)
            {
                paused = false;
                // We are simply paused so just start again
                player.Start();
                StartForeground();
                return;
            }

            if (player == null)
            {
                IntializePlayer();
            }

            if (player.IsPlaying)
            {
                return;
            }

            try
            {
                await player.SetDataSourceAsync(ApplicationContext, Android.Net.Uri.Parse(source));

                var focusResult = audioManager.RequestAudioFocus(this, Stream.Music, AudioFocus.Gain);
                Log.Debug(TAG, "StreamingBackgroundService.Play() RequestAudioFocus result: " + focusResult);
                if (focusResult != AudioFocusRequest.Granted)
                {
                    // TODO: Revert UI back to "Play", because not granted audio focus
                    return;
                }

                player.PrepareAsync();
                RegisterReceiver(headphonesUnpluggedReceiver, new IntentFilter(AudioManager.ActionAudioBecomingNoisy));
                AquireWifiLock();
                StartForeground();
            }
            catch (Exception ex)
            {
                // Unable to start playback. Log error.
                Log.Debug(TAG, "Unable to start playback: " + ex);
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

        private void Pause()
        {
            if (player == null)
            {
                return;
            }

            if (player.IsPlaying)
            {
                player.Pause();
            }

            StopForeground(true);
            paused = true;
        }

        private void Stop()
        {
            Log.Debug(TAG, "StreamingBackgroundService.Stop()");
            if (player == null)
            {
                return;
            }

            if (player.IsPlaying)
            {
                player.Stop();
                UnregisterReceiver(headphonesUnpluggedReceiver);
            }

            var focusResult = audioManager.AbandonAudioFocus(this);
            Log.Debug(TAG, "StreamingBackgroundService.Stop() AbandonAudioFocus result: " + focusResult);
            if (focusResult != AudioFocusRequest.Granted)
            {
                Log.Debug(TAG, "Could not abandon audio focus");
            }

            player.Reset();
            paused = false;
            StopForeground(true);
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
            if (player != null)
            {
                player.Release();
                player = null;
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
                        IntializePlayer();
                    }

                    if (!player.IsPlaying)
                    {
                        player.Start();
                        paused = false;
                    }

                    player.SetVolume(1.0f, 1.0f);  // Turn it up!
                    break;
                case AudioFocus.Loss:
                    Log.Debug(TAG, "AudioFocus.Loss");
                    var message = new AudioBeginInterruptionMessage();
                    MessagingCenter.Send(message, "AudioBeginInterruption");
                    Stop();
                    break;
                case AudioFocus.LossTransient:
                    Log.Debug(TAG, "AudioFocus.LossTransient");
                    //We have lost focus for a short time, but likely to resume so pause
                    Pause();
                    break;
                case AudioFocus.LossTransientCanDuck:
                    //We have lost focus but should till play at a muted 10% volume
                    Log.Debug(TAG, "AudioFocus.LossTransientCanDuck");
                    if (player.IsPlaying)
                        player.SetVolume(.1f, .1f);//turn it down!
                    break;
            }
        }
    }
}
