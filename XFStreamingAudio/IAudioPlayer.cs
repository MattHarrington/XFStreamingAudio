using System;
using System.Collections.Generic;

namespace XFStreamingAudio
{
    public interface IAudioPlayer
    {
        bool IsPlaying { get; }

        bool PlaybackLikelyToKeepUp { get; }

        double DurationLoaded { get; }

        bool PlaybackBufferFull { get; }

        void Play(Uri source);

        void Stop();
    }
}
