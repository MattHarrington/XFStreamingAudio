using System;
using System.Diagnostics;
using AVFoundation;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XFStreamingAudio;
using XFStreamingAudio.iOS;

[assembly:ExportRenderer(typeof(TabbedMainPage), typeof(TabbedMainPageRenderer))]
namespace XFStreamingAudio.iOS
{
    public class TabbedMainPageRenderer : TabbedRenderer
    {
        NSObject observer;
        bool bandwidthSwitchState;

        public TabbedMainPageRenderer()
        {
            bandwidthSwitchState = Helpers.Settings.BandwidthSwitchState;
            NSNotificationCenter notificationCenter = NSNotificationCenter.DefaultCenter;
            notificationCenter.AddObserver(this, new ObjCRuntime.Selector("routeChanged:"), 
                AVAudioSession.RouteChangeNotification, null);
            observer = NSNotificationCenter.DefaultCenter
                .AddObserver((NSString)"NSUserDefaultsDidChangeNotification", DefaultsChanged);
        }

        public void DefaultsChanged(NSNotification obj)
        {   
            Debug.WriteLine("DefaultsChanged()");
            if (bandwidthSwitchState == Helpers.Settings.BandwidthSwitchState)
            {
                Debug.WriteLine("DefaultsUnchanged");
                return;
            }
            bandwidthSwitchState = Helpers.Settings.BandwidthSwitchState;
            Page mp = Xamarin.Forms.Application.Current.MainPage;
            var currentPage = ((TabbedPage)mp).CurrentPage;
            MessagingCenter.Send<Page>((Page)currentPage, "BandwidthSwitchToggled");
        }

        [Export("routeChanged:")]
        public void RouteChanged(NSNotification notification)
        {
            var reason = notification.UserInfo.ValueForKey(new NSString("AVAudioSessionRouteChangeReasonKey"));
            if (reason.Description == "2") // Headphones were unplugged
            {
                // player will be null if user has not hit play
                MessagingCenter.Send<HeadphonesUnpluggedMessage>(new HeadphonesUnpluggedMessage(), 
                    "HeadphonesUnplugged");
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            UIApplication.SharedApplication.BeginReceivingRemoteControlEvents();
            this.BecomeFirstResponder();
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            UIApplication.SharedApplication.EndReceivingRemoteControlEvents();
            this.ResignFirstResponder();
        }

        public override void RemoteControlReceived(UIEvent theEvent)
        {
            if (theEvent.Type != UIEventType.RemoteControl)
            {
                return;
            }

            Debug.WriteLine("Received remote control event: " + theEvent.Subtype);

            Page mp = Xamarin.Forms.Application.Current.MainPage;
            var currentPage = ((TabbedPage)mp).CurrentPage;

            switch (theEvent.Subtype)
            {
                case UIEventSubtype.RemoteControlTogglePlayPause:
                    MessagingCenter.Send<Page>((Page)currentPage, "RemoteControlTogglePlayPause");
                    break;
                case UIEventSubtype.RemoteControlPause:
                case UIEventSubtype.RemoteControlStop:
                    MessagingCenter.Send<Page>((Page)currentPage, "RemoteControlPauseOrStop");
                    break;
                case UIEventSubtype.RemoteControlPlay:
                case UIEventSubtype.RemoteControlPreviousTrack:
                case UIEventSubtype.RemoteControlNextTrack:
                    MessagingCenter.Send<Page>((Page)currentPage, "RemoteControlPlayOrPreviousTrackOrNextTrack");
                    break;
            }
        }
    }
}
