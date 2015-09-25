using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace XFStreamingAudio
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();

            bandwidthSwitch.On = Helpers.Settings.BandwidthSwitchState;
            bandwidthSwitch.OnChanged += OnBandwidthSwitchToggled;
        }

        async void OnBandwidthSwitchToggled(object sender, ToggledEventArgs e)
        {
            if (bandwidthSwitch.On)
            {
                if (await DisplayAlert(
                        "Warning",
                        "High quality uses 43.2 Megabytes/hour. " +
                        "Low quality uses 14.4 Megabytes/hour.\n\n" +
                        "Are you sure you want to switch?",
                        "Yes",
                        "No"))
                {
                    Helpers.Settings.BandwidthSwitchState = bandwidthSwitch.On;
                    Page mp = Xamarin.Forms.Application.Current.MainPage;
                    var currentPage = ((TabbedPage)mp).CurrentPage;
                    MessagingCenter.Send<Page>((Page)currentPage, "BandwidthSwitchToggled");
                }
                else
                {
                    bandwidthSwitch.On = false;
                }
            }
            else
            {
                Helpers.Settings.BandwidthSwitchState = bandwidthSwitch.On;
                Page mp = Xamarin.Forms.Application.Current.MainPage;
                var currentPage = ((TabbedPage)mp).CurrentPage;
                MessagingCenter.Send<Page>((Page)currentPage, "BandwidthSwitchToggled");
            }
        }
    }
}
