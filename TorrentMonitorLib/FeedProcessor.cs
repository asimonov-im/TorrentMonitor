using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using Nito.AsyncEx;
using NLog;

namespace TorrentMonitorLib
{
    class FeedProcessor
    {
        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IFeedItemSource feedItemSource;
        private readonly IPatternSource patternSource;
        private readonly AsyncCollection<FeedItemMatch> output;

        /// <summary>
        /// Gets the feed URL.
        /// </summary>
        public Uri FeedUrl => feedItemSource.FeedUrl;

        /// <summary>
        /// Gets the ID of the last item successfully processed from the feed.
        /// </summary>
        public string LastItemId { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedProcessor" /> class.
        /// </summary>
        /// <param name="feedItemSource">Source for feed items.</param>
        /// <param name="patternSource">Source for patterns used for matching.</param>
        /// <param name="output">Output queue for matches.</param>
        /// <param name="lastItemId">ID of last item processed from the feed.</param>
        public FeedProcessor(
            IFeedItemSource feedItemSource,
            IPatternSource patternSource,
            AsyncCollection<FeedItemMatch> output,
            string lastItemId)
        {
            this.feedItemSource = feedItemSource ?? throw new ArgumentNullException(nameof(feedItemSource));
            this.patternSource = patternSource ?? throw new ArgumentNullException(nameof(patternSource));
            this.output = output ?? throw new ArgumentNullException(nameof(output));
            this.LastItemId = lastItemId;
        }

        /// <summary>
        /// Looks for pattern matches in unprocessed feed items and adds the
        /// matches to the output queue.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token that can be used to abort the operation.</param>
        public async Task Process(CancellationToken cancellationToken)
        {
            logger.ConditionalTrace("Processing items");
            var patterns = patternSource.GetPatterns();

            try
            {
                var items = await GetNewItems(cancellationToken).ConfigureAwait(false);
                foreach (var item in items)
                {
                    await ProcessItem(item, patterns, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    throw;
                }
                else
                {
                    logger.Error(ex);
                }
            }
        }

        private async Task ProcessItem(
            FeedItem item,
            IReadOnlyCollection<MatchPattern> patterns,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            logger.ConditionalDebug("Processing \"{0}\"", item.Title);

            if (TitleMatches(item, patterns))
            {
                logger.Info("Match detected: \"{0}\"", item.Title);
                await EnqueueMatchAsync(item, cancellationToken).ConfigureAwait(false);
            }

            LastItemId = item.Id;
        }

        private async Task<IEnumerable<FeedItem>> GetNewItems(CancellationToken cancellationToken)
        {
            var feedItems = await feedItemSource.GetItemsAsync(cancellationToken).ConfigureAwait(false);
            return FilterNewItems(feedItems);
        }

        private IEnumerable<FeedItem> FilterNewItems(IReadOnlyList<FeedItem> items)
        {
            // Find the index of the last processed item
            int i = 0;
            for (; i < items.Count && items[i].Id != LastItemId; ++i);

            // Get the index of the oldest, unprocessed item.
            // The items in the list are in reverse chronological order.
            --i;

            // Return any unprocessed items, in chronological order.
            for (; i >= 0; --i)
            {
                yield return items[i];
            }
        }

        private async Task EnqueueMatchAsync(FeedItem item, CancellationToken cancellationToken)
        {
            logger.ConditionalDebug("Enqueueing matched item: \"{0}\"", item.Title);

            if (Uri.TryCreate(item.Link, UriKind.Absolute, out Uri matchUrl))
            {
                var match = new FeedItemMatch(item.Title, matchUrl);
                await output.AddAsync(match, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                logger.Error("Failed to create Uri from \"{0}\". Ignoring item.", item.Link);
            }
        }

        private static bool TitleMatches(FeedItem item, IReadOnlyCollection<MatchPattern> patterns)
        {
            return patterns.Any(p => p.CompiledPattern.IsMatch(item.Title));
        }
    }
}
