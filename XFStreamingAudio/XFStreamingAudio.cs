using Xamarin.Forms;
using System.Reflection;

namespace XFStreamingAudio
{
    public class App : Application
    {
        public App()
        {
            var assembly = typeof (App).GetTypeInfo().Assembly;
            XamSvg.Shared.Config.ResourceAssembly = assembly;

            // The root page of your application
            MainPage = new TabbedMainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
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
