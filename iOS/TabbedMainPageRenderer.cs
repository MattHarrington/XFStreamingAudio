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
        public TabbedMainPageRenderer()
        {
            NSNotificationCenter notificationCenter = NSNotificationCenter.DefaultCenter;
            notificationCenter.AddObserver(this, new ObjCRuntime.Selector("routeChanged:"), 
                AVAudioSession.RouteChangeNotification, null);
        }

        [Export("routeChanged:")]
        public void RouteChanged(NSNotification notification)
        {
            var reason = notification.UserInfo.ValueForKey(new NSString("AVAudioSessionRouteChangeReasonKey"));
            if (reason.Description == "2") // Headphones were unplugged
            {
                // player will be null if user has not hit play
                Page mp = Xamarin.Forms.Application.Current.MainPage;
                var currentPage = ((TabbedPage)mp).CurrentPage;
                MessagingCenter.Send<Page>((Page)currentPage, "HeadphonesUnplugged");
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            Debug.WriteLine("ViewDidAppear()");
            base.ViewDidAppear(animated);
            UIApplication.SharedApplication.BeginReceivingRemoteControlEvents();
            this.BecomeFirstResponder();
        }

        public override void ViewDidDisappear(bool animated)
        {
            Debug.WriteLine("ViewDidDisappear()");
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
