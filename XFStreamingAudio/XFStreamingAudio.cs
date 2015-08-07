using System;

using Xamarin.Forms;
using System.Diagnostics;

namespace XFStreamingAudio
{
    public class App : Application
    {

        public App()
        {
            // The root page of your application
            MainPage = new XFStreamingAudio.MainPage();
        }

        protected override void OnStart()
        {

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

