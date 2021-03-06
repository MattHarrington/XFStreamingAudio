﻿using System;
using System.Diagnostics;
using Xamarin.Forms;
using Connectivity.Plugin;

namespace XFStreamingAudio
{
    public partial class SchedulePage : ContentPage
    {
        const string height = "500";
        const string url = 
            "https://calendar.google.com/calendar/embed" +
            "?showNav=0&showTitle=0&showPrint=0&showTabs=0&showCalendars=0" +
            "&mode=AGENDA&height=" + height + "&wkst=1&bgcolor=%23FFFFFF" +
            "&src=p8smcuo4jo50sthdad6cfq3cko%40group.calendar.google.com" +
            "&color=%235229A3&ctz=America%2FLos_Angeles";
        readonly Uri calendarUri;

        public SchedulePage()
        {
            InitializeComponent();
            browser.Navigated += OnBrowserNavigated;
            backBtn.Clicked += backClicked;
            calendarUri = new Uri(url);
            browser.Source = calendarUri.AbsoluteUri;

            TapGestureRecognizer refreshIconTGR = new TapGestureRecognizer();
            refreshIconTGR.Tapped += OnRefresh;
            refreshIcon.GestureRecognizers.Add(refreshIconTGR);
        }

        async void OnRefresh(object sender, EventArgs e)
        {
            var sourceReachable = await CrossConnectivity.Current.IsRemoteReachable(calendarUri.Host);
            if (!sourceReachable)
            {
                await DisplayAlert("Server Unreachable", "Check your network connection", "OK");
                return;
            }
            browser.Source = calendarUri.AbsoluteUri;
        }

        void OnBrowserNavigated(object sender, WebNavigatedEventArgs e)
        {
            // Hide and display the Back button by comparing current location vs. initial URL because
            // using CanGoBack was not working as expected.
            var currentSource = browser.Source as UrlWebViewSource;
            if (currentSource.Url != calendarUri.AbsoluteUri)
            {
                backBtn.IsEnabled = true;
            }
            else
            {
                backBtn.IsEnabled = false;
            }
        }

        private void backClicked(object sender, EventArgs e)
        {
            if (browser.CanGoBack)
            {
                browser.GoBack();
            }
        }
    }
}
