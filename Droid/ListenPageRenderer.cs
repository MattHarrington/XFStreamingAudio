using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XFStreamingAudio;
using XFStreamingAudio.Droid;
using Android.Content;

//[assembly:ExportRenderer(typeof(ListenPage), typeof(ListenPageRenderer))]
namespace XFStreamingAudio.Droid
{
    public class ListenPageRenderer : PageRenderer
    {
        public ListenPageRenderer()
        {
        }

//        private void SendAudioCommand(string action)
//        {
//            var intent = new Intent(action);
//            StartService(intent);
//        }

    }
}
