using Xamarin.Forms;
using System.Reflection;
using AI.XamarinSDK.Abstractions;

namespace XFStreamingAudio
{
    public class App : Application
    {
        public App()
        {
            var assembly = typeof(App).GetTypeInfo().Assembly;
            XamSvg.Shared.Config.ResourceAssembly = assembly;

            // The root page of your application
            MainPage = new TabbedMainPage();
        }

        protected override void OnStart()
        {
            ApplicationInsights.Setup("ac4e2cf9-3f15-4cf1-b1dd-26aebb28cf63");
            ApplicationInsights.Start();
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
