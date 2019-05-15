using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NLog;

namespace TorrentMonitorLib
{
    /// <summary>
    /// Processes <see cref="FeedItemMatch"/> items.
    /// </summary>
    class FeedItemMatchProcessor
        {
        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly AsyncProducerConsumerQueue<FeedItemMatch> input;
        private readonly AsyncProducerConsumerQueue<Torrent> output;

        public FeedItemMatchProcessor(
            AsyncProducerConsumerQueue<FeedItemMatch> input,
            AsyncProducerConsumerQueue<Torrent> output)
        {
            this.input = input ?? throw new ArgumentNullException(nameof(input));
            this.output = output ?? throw new ArgumentNullException(nameof(output));
        }

        public async Task Process(CancellationToken cancellationToken)
        {
            while (await input.OutputAvailableAsync(cancellationToken).ConfigureAwait(false))
            {
                var item = await input.DequeueAsync(cancellationToken).ConfigureAwait(false);
                logger.ConditionalDebug("Processing \"{0}\"", item.Title);

                // Do not pass a CancellationToken, so that the operation cannot be interrupted.
                var torrentItem = new Torrent(item.Url, true, item.Title);
                await output.EnqueueAsync(torrentItem).ConfigureAwait(false);
            }
        }
    }
}
