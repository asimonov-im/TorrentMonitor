using System;

namespace TorrentMonitorLib
{
    public class Torrent
    {
        public Uri Location { get; }

        public bool AutoStart { get; }

        public string Description { get; }

        public Torrent(Uri location, bool autoStart, string description = null)
        {
            Location = location ?? throw new ArgumentNullException(nameof(location));
            AutoStart = autoStart;
            Description = description;
        }
    }
}
