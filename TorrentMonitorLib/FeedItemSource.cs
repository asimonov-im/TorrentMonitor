using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CodeHollow.FeedReader;

namespace TorrentMonitorLib
{
    /// <summary>
    /// Fetches items from the specified RSS feed.
    /// </summary>
    class FeedItemSource : IFeedItemSource
    {
        /// <summary>
        /// Gets the feed URL.
        /// </summary>
        public Uri FeedUrl { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedItemSource" /> class.
        /// </summary>
        /// <param name="feedUrl">Feed URL.</param>
        public FeedItemSource(Uri feedUrl)
        {
            FeedUrl = feedUrl ?? throw new ArgumentNullException(nameof(feedUrl));
        }

        /// <summary>
        /// Fetches all available items from the feed.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token that can be used to abort the operation.</param>
        public async Task<IReadOnlyList<FeedItem>> GetItemsAsync(CancellationToken cancellationToken)
        {
            var feed = await FeedReader.ReadAsync(FeedUrl.AbsoluteUri, cancellationToken).ConfigureAwait(false);
            return feed.Items;
        }
    }
}
