using System;

namespace XFStreamingAudio
{
    public interface IAudioPlayer
    {
        bool IsPlaying{ get; }

        void Play(Uri source);

        void Stop();
    }
}

