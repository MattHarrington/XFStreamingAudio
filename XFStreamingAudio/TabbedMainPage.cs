using System;
using Xamarin.Forms;

namespace XFStreamingAudio
{
    public class TabbedMainPage : TabbedPage
    {
        public TabbedMainPage()
        {
            Title = "KALX Streaming";
            Children.Add(new ListenPage());
            //Children.Add(new SchedulePage());
            Children.Add(new PledgePage());
            Children.Add(new ConnectPage());

            MessagingCenter.Subscribe<LostStreamMessage>(this, "LostStream", OnLostStream);
        }

        void OnLostStream(LostStreamMessage obj)
        {
            Device.BeginInvokeOnMainThread(async () => await DisplayAlert("Lost Stream", "Check network.", "OK"));
        }
    }
}
