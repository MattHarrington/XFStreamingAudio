using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin;

namespace XFStreamingAudio.Droid
{
    [Activity(Label = "XFStreamingAudio.Droid", Icon = "@drawable/icon", MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            #if DEBUG
            Insights.Initialize(Insights.DebugModeKey, this);
            #else
            Insights.Initialize("6177fa9abb40234e5363a7143c5ee7b9cf7a70a9", this);
            #endif

            global::Xamarin.Forms.Forms.Init(this, bundle);

            LoadApplication(new App());
        }
    }
}

