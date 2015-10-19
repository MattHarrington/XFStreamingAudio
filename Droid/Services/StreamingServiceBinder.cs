using System;
using Android.OS;
using XFStreamingAudio.Droid.Services;

namespace XFStreamingAudio.Droid
{
    public class StreamingServiceBinder : Binder
    {
        StreamingService _service;

        public StreamingServiceBinder(StreamingService service)
        {
            _service = service;
        }

        public StreamingService GetStreamingBackgroundService()
        {
            return _service;
        }
    }
}
