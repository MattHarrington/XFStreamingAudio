using System;

using Xamarin.Forms;
using System.Diagnostics;

namespace XFStreamingAudio
{
    public class App : Application
    {
        Button playStopBtn;

        public App()
        {
            playStopBtn = new Button { Text = "Play", FontSize = 32 };
            // The root page of your application
            MainPage = new ContentPage
            {
                Content = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label
                        {
                            XAlign = TextAlignment.Center,
                            FontSize = 48,
                            Text = "KVMR"
                        },
                        playStopBtn
                    }
                }
            };
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            playStopBtn.Clicked += OnPlayStopBtnClicked;
        }

        void OnPlayStopBtnClicked (object sender, EventArgs e)
        {
            Debug.WriteLine("Clicked!");
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}

