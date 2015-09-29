using System;
using Xamarin.Forms;

namespace XFStreamingAudio
{
    public class TabbedMainPage : TabbedPage
    {
        public TabbedMainPage()
        {
            Title = "KVMR Streaming";
            BackgroundColor = Color.White;
            Children.Add(new ListenPage());
            Children.Add(new SchedulePage());
            //Children.Add(new PlaylistPage());
            Children.Add(new PledgePage());
            Children.Add(new ConnectPage());
        }
    }
}
