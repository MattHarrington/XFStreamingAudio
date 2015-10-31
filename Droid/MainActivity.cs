﻿using System;

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

namespace XFStreamingAudio.Droid
{
    [Activity(Label = "KVMR", Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
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
            base.OnCreate(bundle);

            #if DEBUG
            Insights.Initialize(Insights.DebugModeKey, this);
            #else
            Insights.Initialize("6177fa9abb40234e5363a7143c5ee7b9cf7a70a9", this);
            #endif

            SvgImageRenderer.InitializeForms();
            global::Xamarin.Forms.Forms.Init(this, bundle);

            LoadApplication(new App());
        }

        protected override void OnStart()
        {
            base.OnStart();

            var intent = new Intent(this, typeof(StreamingService));
            streamingBackgroundServiceConnection = new StreamingBackgroundServiceConnection(this);
            BindService(intent, streamingBackgroundServiceConnection, Bind.None);
        }

        protected override void OnDestroy ()
        {
            base.OnDestroy ();

            if (!isConfigurationChange) {
                if (isBound) {
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
                var streamingBackgroundServiceBinder = service as StreamingServiceBinder;

                if (streamingBackgroundServiceBinder != null)
                {
                    activity.binder = streamingBackgroundServiceBinder;
                    activity.isBound = true;

                    // keep instance for preservation across configuration changes
                    this.binder = streamingBackgroundServiceBinder;
                }
            }

            public void OnServiceDisconnected(ComponentName name)
            {
                activity.isBound = false;
            }
        }
    }
}
