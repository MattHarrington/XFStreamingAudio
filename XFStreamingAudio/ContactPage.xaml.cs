using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace XFStreamingAudio
{
    public partial class ContactPage : ContentPage
    {
        public ContactPage()
        {
            InitializeComponent();

            TapGestureRecognizer callIconTGR = new TapGestureRecognizer();
            callIconTGR.Tapped += OnCallStudio;
            callIcon.GestureRecognizers.Add(callIconTGR);
            callStudioBtn.Clicked += OnCallStudio;

            TapGestureRecognizer emailIconTGR = new TapGestureRecognizer();
            emailIconTGR.Tapped += OnEmailOffice;
            emailIcon.GestureRecognizers.Add(emailIconTGR);
            emailOfficeBtn.Clicked += OnEmailOffice;

            TapGestureRecognizer feedbackIconTGR = new TapGestureRecognizer();
            feedbackIconTGR.Tapped += OnEmailFeedback;
            feedbackIcon.GestureRecognizers.Add(feedbackIconTGR);
            feedbackBtn.Clicked += OnEmailFeedback;
        }
            
        async void OnCallStudio(object sender, System.EventArgs e)
        {
            if (await DisplayAlert(
                    "Call Studio",
                    "Call live broadcaster at\n +1 (530) 265-9555?",
                    "Yes",
                    "No"))
            {
                // Dial the phone
                Device.OpenUri(new Uri("tel:+1 (530) 265-9555"));
            }
        }

        void OnEmailOffice(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("mailto:office@kvmr.org"));
        }

        void OnEmailFeedback(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("mailto:apps@kvmr.org"));
        }
    }
}
