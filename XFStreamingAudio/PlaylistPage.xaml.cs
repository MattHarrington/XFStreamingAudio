using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace XFStreamingAudio
{
    public partial class PlaylistPage : ContentPage
    {
        public PlaylistPage()
        {
            Title = "Playlist";
            Content = new BoxView
            {
                Color = Color.Red,
                HeightRequest = 100f,
                VerticalOptions = LayoutOptions.Center
            };
            InitializeComponent();
        }
    }
}

