using System;
using Android.OS;
using XFStreamingAudio.Droid.Services;

namespace XFStreamingAudio.Droid
{
    public class StreamingBackgroundServiceBinder : Binder
    {
        StreamingBackgroundService _service;

        public StreamingBackgroundServiceBinder(StreamingBackgroundService service)
        {
            _service = service;
        }

        public StreamingBackgroundService GetStreamingBackgroundService()
        {
            return _service;
        }
    }
}
