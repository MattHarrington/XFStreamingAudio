using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Foundation;
using UIKit;
using AI.XamarinSDK.Abstractions;

namespace XFStreamingAudio.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        NSObject observer;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

            ApplicationInsights.Setup("6bc59a82-3838-4b99-806e-d6929b5b5651");
            ApplicationInsights.Start();
            AI.XamarinSDK.iOS.ApplicationInsights.Init();

            // Code for starting up the Xamarin Test Cloud Agent
            #if ENABLE_TEST_CLOUD
			Xamarin.Calabash.Start();
            #endif

            LoadApplication(new App());

            observer = NSNotificationCenter.DefaultCenter
                .AddObserver((NSString)"NSUserDefaultsDidChangeNotification", DefaultsChanged);

            return base.FinishedLaunching(app, options);
        }

        public void DefaultsChanged(NSNotification obj)
        {   
            Debug.WriteLine("DefaultsChanged()");
//            Page mp = Xamarin.Forms.Application.Current.MainPage;
//            var currentPage = ((TabbedPage)mp).CurrentPage;
//            MessagingCenter.Send<Page>((Page)currentPage, "BandwidthSwitchToggled");
        }

        public override void WillTerminate (UIApplication application)
        {
            if (observer != null) {
                NSNotificationCenter.DefaultCenter.RemoveObserver (observer);
                observer = null;
            }
        }

        public override void ReceiveMemoryWarning(UIApplication application)
        {
            Debug.WriteLine("ReceiveMemoryWarning()");
            TelemetryManager.TrackEvent("ReceiveMemoryWarning()");
        }
    }
}
