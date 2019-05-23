using System;

namespace TixatiLib
{
    public class TorrentInfo
    {
        public string Id { get; }
        public string Name { get; }
        public float SizeMegabytes { get; }
        public int PercentDone { get; }
        public TorrentStatus Status { get; }
        public TorrentPriority Priority { get; }
        public TimeSpan TimeRemaining { get; }

        public bool IsDone => PercentDone == 100;

        public TorrentInfo(
            string id,
            string name,
            float size,
            int percentDone,
            TorrentStatus status,
            TorrentPriority priority,
            TimeSpan timeRemaining)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            SizeMegabytes = size;
            PercentDone = percentDone;
            Status = status;
            Priority = priority;
            TimeRemaining = timeRemaining;
        }
    }
}
