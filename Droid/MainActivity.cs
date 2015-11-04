using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin;
using XFStreamingAudio.Droid.Services;
using XamSvg.XamForms.Droid;
using Android.Util;
using Xamarin.Forms;

namespace XFStreamingAudio.Droid
{
    [Activity(Label = "KVMR", Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        const string TAG = "KVMR";
        bool isBound = false;
        bool isConfigurationChange = false;
        StreamingServiceBinder binder;
        StreamingBackgroundServiceConnection streamingBackgroundServiceConnection;

        public bool IsPlaying
        {
            get
            {
                if (isBound)
                {
                    return binder.GetStreamingBackgroundService().IsPlaying;
                }
                else
                {
                    return false;
                }
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            Log.Debug(TAG, "MainActivity.OnCreate()");
            base.OnCreate(bundle);

            SvgImageRenderer.InitializeForms();
            global::Xamarin.Forms.Forms.Init(this, bundle);

            LoadApplication(new App());
        }

        protected override void OnStart()
        {
            Log.Debug(TAG, "MainActivity.OnStart()");
            base.OnStart();

            var intent = new Intent(this, typeof(StreamingService));
            streamingBackgroundServiceConnection = new StreamingBackgroundServiceConnection(this);
            BindService(intent, streamingBackgroundServiceConnection, Bind.None);
        }

        protected override void OnDestroy()
        {
            Log.Debug(TAG, "MainActivity.OnDestroy()");
            base.OnDestroy();

            if (!isConfigurationChange)
            {
                if (isBound)
                {
                    UnbindService(streamingBackgroundServiceConnection);
                    isBound = false;
                }
            }
        }

        class StreamingBackgroundServiceConnection : Java.Lang.Object, IServiceConnection
        {
            MainActivity activity;
            StreamingServiceBinder binder;

            public StreamingServiceBinder Binder
            {
                get
                {
                    return binder;
                }
            }

            public StreamingBackgroundServiceConnection(MainActivity activity)
            {
                this.activity = activity;
            }

            public void OnServiceConnected(ComponentName name, IBinder service)
            {
                Log.Debug(TAG, "OnServiceConnected()");
                var streamingBackgroundServiceBinder = service as StreamingServiceBinder;

                if (streamingBackgroundServiceBinder != null)
                {
                    activity.binder = streamingBackgroundServiceBinder;
                    activity.isBound = true;

                    // keep instance for preservation across configuration changes
                    this.binder = streamingBackgroundServiceBinder;
                    if (binder.GetStreamingBackgroundService().IsPlaying)
                    {
                        MessagingCenter.Send<PlayerStartedMessage>(new PlayerStartedMessage(), "PlayerStarted");
                    }
                }
            }

            public void OnServiceDisconnected(ComponentName name)
            {
                activity.isBound = false;
            }
        }
    }
}
