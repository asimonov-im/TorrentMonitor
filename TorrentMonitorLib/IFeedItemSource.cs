﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CodeHollow.FeedReader;

namespace TorrentMonitorLib
{
    /// <summary>
    /// Interface for class used to fetch items from an RSS feed.
    /// </summary>
    interface IFeedItemSource
    {
        Task<IReadOnlyList<FeedItem>> GetItemsAsync(CancellationToken cancellationToken);
    }
}