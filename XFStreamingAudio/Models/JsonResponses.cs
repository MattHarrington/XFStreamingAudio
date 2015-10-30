using System;
using System.Collections.Generic;

namespace XFStreamingAudio
{
    public class Track
    {
        public IList<string> location { get; set; }
        public string title { get; set; }
    }

    public class Playlist
    {
        public string title { get; set; }
        public string creator { get; set; }
        public IList<Track> track { get; set; }
    }
}

