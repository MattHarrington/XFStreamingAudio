using System;

namespace XFStreamingAudio
{
    public interface IDeviceExtension
    {
        bool CanOpenUrl(Uri uri);
    }
}
