using System;

namespace TixatiLib
{
    public class TorrentFileInfo
    {
        public string Name { get; }
        public Uri DownloadUri { get; }
        public TorrentPriority Priority { get; }
        public float SizeMegabytes { get; }
        public int PercentDone { get; }

        public bool IsDone => PercentDone == 100;

        public TorrentFileInfo(
            string name,
            Uri downloadUri,
            TorrentPriority priority,
            float size,
            int percentDone)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DownloadUri = downloadUri;
            Priority = priority;
            SizeMegabytes = size;
            PercentDone = percentDone;
        }
    }
}
