using System;

namespace XFStreamingAudio
{
    public interface IAudioPlayer
    {
        void Play(Uri source);
        void Stop();
    }
}

