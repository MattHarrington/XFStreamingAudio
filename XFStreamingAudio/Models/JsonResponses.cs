using System;
using System.Collections.Generic;

namespace XFStreamingAudio
{
    public class Track
    {
        public List<string> location { get; set; }

        public string title { get; set; }
    }

    public class Playlist
    {
        public string title { get; set; }

        public string creator { get; set; }

        public List<Track> track { get; set; }
    }

    public class JSPF
    {
        public Playlist playlist { get; set; }
    }
}
