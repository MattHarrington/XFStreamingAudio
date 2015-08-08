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
            callStudioBtn.Clicked += OnCallStudio;
            emailOfficeBtn.Clicked += OnEmailOffice;
        }

        async void OnCallStudio(object sender, System.EventArgs e)
        {
            if (await DisplayAlert(
                    "Call KVMR Studio",
                    "Call +1 (530) 265-9555?",
                    "Yes",
                    "No"))
            {
                // Dial the phone
                Device.OpenUri(new Uri("tel:+1 (530) 555-0000"));
            }
        }

        void OnEmailOffice(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("mailto:office@kvmr.org"));
        }
    }
}
