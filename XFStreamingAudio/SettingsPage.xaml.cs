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
            if (Application.Current.Properties.ContainsKey("bandwidthSwitchState"))
            {
                bandwidthSwitch.On = (bool)Application.Current.Properties["bandwidthSwitchState"];
            }
            bandwidthSwitch.OnChanged += OnBandwidthSwitchToggled;
        }

        async void OnBandwidthSwitchToggled(object sender, ToggledEventArgs e)
        {
            if (bandwidthSwitch.On)
            {
                if (await DisplayAlert(
                        "Warning",
                        "High quality stream uses 43.2 Megabytes/hour. " +
                        "Low quality stream uses 14.4 Megabytes/hour.\n\n" +
                        "Are you sure you want to switch?",
                        "Yes",
                        "No"))
                {
                    Application.Current.Properties["bandwidthSwitchState"] = bandwidthSwitch.On;
                    Page mp = Xamarin.Forms.Application.Current.MainPage;
                    var currentPage = ((TabbedPage)mp).CurrentPage;
                    MessagingCenter.Send<Page>((Page)currentPage, "BandwidthSwitchToggled");
                }
                else
                {
                    if (Application.Current.Properties.ContainsKey("bandwidthSwitchState"))
                    {
                        bandwidthSwitch.On = (bool)Application.Current.Properties["bandwidthSwitchState"];
                    }
                    else
                    {
                        bandwidthSwitch.On = false;
                    }
                }
            }
            else
            {
                Application.Current.Properties["bandwidthSwitchState"] = bandwidthSwitch.On;
                Page mp = Xamarin.Forms.Application.Current.MainPage;
                var currentPage = ((TabbedPage)mp).CurrentPage;
                MessagingCenter.Send<Page>((Page)currentPage, "BandwidthSwitchToggled");
            }
        }
    }
}
