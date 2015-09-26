using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Foundation;
using UIKit;
using Xamarin;

namespace XFStreamingAudio.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

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
            Insights.Track("ReceiveMemoryWarning", new Dictionary <string, string> { 
                {"track-local-time", DateTime.Now.ToString()}
            });
        }
    }
}
