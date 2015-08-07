using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace XFStreamingAudio
{
    public partial class PledgePage : ContentPage
    {
        public PledgePage()
        {
            Title = "Support";
            Content = new BoxView
            {
                Color = Color.Green,
                HeightRequest = 100f,
                VerticalOptions = LayoutOptions.Center
            };
            InitializeComponent();
        }
    }
}

