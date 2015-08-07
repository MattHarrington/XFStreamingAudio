using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace XFStreamingAudio
{
    public partial class SchedulePage : ContentPage
    {
        public SchedulePage()
        {
            Title = "Schedule";
            Content = new BoxView
            {
                Color = Color.Blue,
                HeightRequest = 100f,
                VerticalOptions = LayoutOptions.Center
            };
            InitializeComponent();
        }
    }
}

