using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace XFStreamingAudio
{
    public partial class SchedulePage : ContentPage
    {
        public SchedulePage()
        {
            InitializeComponent();
            scheduleListView.ItemsSource = new string[]
            {"Monday", "Tuesday", "Wednesday", "Thursday",
                "Friday", "Saturday", "Sunday"
            };
        }
    }
}
