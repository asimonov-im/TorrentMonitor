﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NLog;

namespace TorrentMonitorLib
{
    class TorrentProcessor
    {
        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ITorrentClient torrentClient;
        private readonly AsyncProducerConsumerQueue<Torrent> input;

        public TorrentProcessor(ITorrentClient torrentClient, AsyncProducerConsumerQueue<Torrent> input)
        {
            if (torrentClient == null)
            {
                throw new ArgumentNullException(nameof(torrentClient));
            }
            else if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            this.torrentClient = torrentClient;
            this.input = input;
        }

        public async Task Process(CancellationToken cancellationToken)
        {
            while (true)
            {
                // The AsyncProducerConsumerQueue should never be in a completed state,
                // so DequeueAsync should only throw OperationCanceledException,
                // in which case the item should still be in the queue.
                var item = await input.DequeueAsync(cancellationToken).ConfigureAwait(false);
                logger.ConditionalDebug("Processing \"{0}\"", item.Description);

                try
                {
                    await AddItemAsync(item, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // Put the item back in the queue, so it is not lost.
                    // Do not provide a CancellationToken to try to ensure that the operation succeeds.
                    logger.Warn("Adding \"{0}\" back into the queue due to failure", item.Description);
                    await input.EnqueueAsync(item).ConfigureAwait(false);

                    if (ex is OperationCanceledException)
                    {
                        throw;
                    }
                    else
                    {
                        logger.Error(ex, "Adding torrent failed");
                    }
                }
            }
        }

        private async Task AddItemAsync(Torrent item, CancellationToken cancellationToken)
        {
            Task addTask;
            if (item.Location.IsFile)
            {
                addTask = torrentClient.AddFileAsync(item.Location.LocalPath, item.AutoStart, cancellationToken);
            }
            else
            {
                addTask = torrentClient.AddLinkAsync(item.Location.AbsoluteUri, item.AutoStart, cancellationToken);
            }

            await addTask.ConfigureAwait(false);
        }
    }
}
