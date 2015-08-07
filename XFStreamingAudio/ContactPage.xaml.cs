using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace XFStreamingAudio
{
    public partial class ContactPage : ContentPage
    {
        public ContactPage()
        {
            Title = "Contact";
            Content = new BoxView
            {
                Color = Color.Purple,
                HeightRequest = 100f,
                VerticalOptions = LayoutOptions.Center
            };
            InitializeComponent();
        }
    }
}

