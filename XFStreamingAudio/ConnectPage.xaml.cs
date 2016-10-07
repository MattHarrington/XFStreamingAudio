using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace XFStreamingAudio
{
    public partial class ConnectPage : ContentPage
    {
        public ConnectPage()
        {
            InitializeComponent();

            if (Device.OS == TargetPlatform.Android)
            {
                callStudioBtn.TextColor = callOfficeBtn.TextColor = emailOfficeBtn.TextColor = 
                    /*feedbackBtn.TextColor =*/ websiteBtn.TextColor = twitterBtn.TextColor = 
                        facebookBtn.TextColor = infoBtn.TextColor = Color.White;
            }

            TapGestureRecognizer callStudioIconTGR = new TapGestureRecognizer();
            callStudioIconTGR.Tapped += OnCallStudio;
            callStudioIcon.GestureRecognizers.Add(callStudioIconTGR);
            callStudioBtn.Clicked += OnCallStudio;

            TapGestureRecognizer callOfficeIconTGR = new TapGestureRecognizer();
            callOfficeIconTGR.Tapped += OnCallOffice;
            callOfficeIcon.GestureRecognizers.Add(callOfficeIconTGR);
            callOfficeBtn.Clicked += OnCallOffice;

            TapGestureRecognizer emailIconTGR = new TapGestureRecognizer();
            emailIconTGR.Tapped += OnEmailOffice;
            emailIcon.GestureRecognizers.Add(emailIconTGR);
            emailOfficeBtn.Clicked += OnEmailOffice;

            //TapGestureRecognizer feedbackIconTGR = new TapGestureRecognizer();
            //feedbackIconTGR.Tapped += OnEmailFeedback;
            //feedbackIcon.GestureRecognizers.Add(feedbackIconTGR);
            //feedbackBtn.Clicked += OnEmailFeedback;

            TapGestureRecognizer websiteIconTGR = new TapGestureRecognizer();
            websiteIconTGR.Tapped += OnVisitWebsite;
            websiteIcon.GestureRecognizers.Add(websiteIconTGR);
            websiteBtn.Clicked += OnVisitWebsite;

            TapGestureRecognizer facebookIconTGR = new TapGestureRecognizer();
            facebookIconTGR.Tapped += OnLaunchFacebook;
            facebookIcon.GestureRecognizers.Add(facebookIconTGR);
            facebookBtn.Clicked += OnLaunchFacebook;

            TapGestureRecognizer twitterIconTGR = new TapGestureRecognizer();
            twitterIconTGR.Tapped += OnLaunchTwitter;
            twitterIcon.GestureRecognizers.Add(twitterIconTGR);
            twitterBtn.Clicked += OnLaunchTwitter;

            //TapGestureRecognizer archiveIconTGR = new TapGestureRecognizer();
            //archiveIconTGR.Tapped += OnLaunchArchive;
            //archiveIcon.GestureRecognizers.Add(archiveIconTGR);
            //archiveBtn.Clicked += OnLaunchArchive;

            TapGestureRecognizer infoIconTGR = new TapGestureRecognizer();
            infoIconTGR.Tapped += OnDisplayInfo;
            infoIcon.GestureRecognizers.Add(infoIconTGR);
            infoBtn.Clicked += OnDisplayInfo;
        }

        async void OnDisplayInfo(object sender, EventArgs e)
        {
            await DisplayAlert("About", 
                #if DEBUG
                "Debug Build\n" +
                #endif
                "Version: 1.0.0.11\n" +
                "Built with: Xamarin\n\n",
                "OK");
        }

        void OnLaunchTwitter(object sender, EventArgs e)
        {
            Uri twitterUrlScheme = new Uri("twitter://user?id=38070896");
            IDeviceExtension deviceExtension = DependencyService.Get<IDeviceExtension>();
            bool canOpenUrl = deviceExtension.CanOpenUrl(twitterUrlScheme);
            if (canOpenUrl)
            {
                Device.OpenUri(twitterUrlScheme);
            }
            else
            {
                Device.OpenUri(new Uri("https://twitter.com/KALXradio"));
            }
        }

        void OnLaunchFacebook(object sender, EventArgs e)
        {
            Uri facebookUrlScheme = new Uri("fb://profile/8422996124");
            IDeviceExtension deviceExtension = DependencyService.Get<IDeviceExtension>();
            bool canOpenUrl = deviceExtension.CanOpenUrl(facebookUrlScheme);
            if (canOpenUrl)
            {
                Device.OpenUri(facebookUrlScheme);
            }
            else
            {
                Device.OpenUri(new Uri("https://www.facebook.com/90.7fm"));
            }
        }

        void OnVisitWebsite(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("https://kalx.berkeley.edu/"));
        }

        void OnLaunchArchive(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("http://archive.kvmr.org/"));
        }

        async void OnCallStudio(object sender, System.EventArgs e)
        {
            if (await DisplayAlert(
                    "Call Studio",
                    "Call live broadcaster at\n+1 (510) 642-KALX?",
                    "Yes",
                    "No"))
            {
                // Dial the phone
                Device.OpenUri(new Uri("tel:+1 (510) 642-5259"));
            }
        }

        async void OnCallOffice(object sender, EventArgs e)
        {
            if (await DisplayAlert(
                    "Call Office",
                    "Call KALX office at\n+1 (510) 642-1111?",
                    "Yes",
                    "No"))
            {
                // Dial the phone
                Device.OpenUri(new Uri("tel:+1 (510) 642-1111"));
            }
        }

        void OnEmailOffice(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("mailto:mail@kalx.berkeley.edu"));
        }

        void OnEmailFeedback(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("mailto:apps@kvmr.org"));
        }
    }
}
