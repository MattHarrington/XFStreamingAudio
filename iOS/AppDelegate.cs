using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Foundation;
using UIKit;
using Xamarin;
using XamSvg.XamForms.iOS;
//using AI.XamarinSDK.Abstractions;

namespace XFStreamingAudio.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            SvgImageRenderer.InitializeForms();
            global::Xamarin.Forms.Forms.Init();

            //AI.XamarinSDK.iOS.ApplicationInsights.Init();

            // Code for starting up the Xamarin Test Cloud Agent
            #if ENABLE_TEST_CLOUD
			Xamarin.Calabash.Start();
            #endif

            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

//        public override void WillTerminate(UIApplication application)
//        {
//            if (observer != null)
//            {
//                NSNotificationCenter.DefaultCenter.RemoveObserver(observer);
//                observer = null;
//            }
//        }

        public override void ReceiveMemoryWarning(UIApplication application)
        {
            Debug.WriteLine("ReceiveMemoryWarning()");
            Dictionary<string, string> memoryWarningProperties = new Dictionary<string, string> ();
            memoryWarningProperties.Add ("DateTime.UtcNow", DateTime.UtcNow.ToString());
            //TelemetryManager.TrackEvent("ReceiveMemoryWarning", memoryWarningProperties);
        }
    }
}
