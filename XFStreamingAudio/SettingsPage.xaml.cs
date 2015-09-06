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

        void OnBandwidthSwitchToggled(object sender, ToggledEventArgs e)
        {
            Application.Current.Properties["bandwidthSwitchState"] = bandwidthSwitch.On;
            Page mp = Xamarin.Forms.Application.Current.MainPage;
            var currentPage = ((TabbedPage)mp).CurrentPage;
            MessagingCenter.Send<Page>((Page)currentPage, "BandwidthSwitchToggled");
        }
    }
}
