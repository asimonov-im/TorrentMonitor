using System.Collections.Generic;

namespace TorrentMonitorLib
{
    /// <summary>
    /// Interface for class used to return patterns used to match feed items.
    /// </summary>
    interface IPatternSource
    {
        IReadOnlyCollection<MatchPattern> GetPatterns();
    }
}
