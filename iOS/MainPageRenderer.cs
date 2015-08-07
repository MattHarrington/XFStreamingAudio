using System;
using Xamarin.Forms.Platform.iOS;
using System.Diagnostics;
using UIKit;
using Xamarin.Forms;
using Foundation;
using AVFoundation;

[assembly:ExportRenderer(typeof(XFStreamingAudio.MainPage), typeof(XFStreamingAudio.iOS.MainPageRenderer))]
namespace XFStreamingAudio.iOS
{
    public class MainPageRenderer : PageRenderer
    {
        public MainPageRenderer()
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
                MessagingCenter.Send<XFStreamingAudio.MainPage>((XFStreamingAudio.MainPage)mp, 
                    "HeadphonesUnplugged");
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

            switch (theEvent.Subtype)
            {
                case UIEventSubtype.RemoteControlTogglePlayPause:
                    MessagingCenter.Send<XFStreamingAudio.MainPage>((XFStreamingAudio.MainPage)mp, 
                        "RemoteControlTogglePlayPause");
                    break;
                case UIEventSubtype.RemoteControlPause:
                case UIEventSubtype.RemoteControlStop:
                    MessagingCenter.Send<XFStreamingAudio.MainPage>((XFStreamingAudio.MainPage)mp, 
                        "RemoteControlPauseOrStop");
                    break;
                case UIEventSubtype.RemoteControlPlay:
                case UIEventSubtype.RemoteControlPreviousTrack:
                case UIEventSubtype.RemoteControlNextTrack:
                    MessagingCenter.Send<XFStreamingAudio.MainPage>((XFStreamingAudio.MainPage)mp, 
                        "RemoteControlPlayOrPreviousTrackOrNextTrack");
                    break;
            }
        }
    }
}
