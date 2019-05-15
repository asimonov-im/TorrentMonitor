using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NLog;

namespace TorrentMonitorLib
{
    public class TorrentMonitor : IPatternSource
    {
        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly MonitorConfig config;

        private bool isStarted;
        private CancellationTokenSource ctSource;
        private List<FeedProcessor> feedProcessors;
        private Task[] tasks;
        private AsyncProducerConsumerQueue<FeedItemMatch> feedItemQueue;
        private AsyncProducerConsumerQueue<Torrent> torrentQueue;

        private TorrentMonitor(MonitorConfig config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public void Start()
        {
            if (!isStarted)
            {
                logger.Info("Starting");

                var allTasks = new List<Task>();
                ctSource = new CancellationTokenSource();
                feedProcessors = new List<FeedProcessor>();
                feedItemQueue = new AsyncProducerConsumerQueue<FeedItemMatch>(config.FeedItemMatches);
                torrentQueue = new AsyncProducerConsumerQueue<Torrent>(config.Torrents);

                var feedProcessorDelay = TimeSpan.FromSeconds(config.FeedUpdateFrequency);
                foreach (var feedInfo in config.Feeds)
                {
                    var feedItemSource = new FeedItemSource(feedInfo.Url);
                    var feedProcessor = new FeedProcessor(feedItemSource, this, feedItemQueue, feedInfo.LastItemId);
                    feedProcessors.Add(feedProcessor);

                    var processorTask = TaskExtensions.PeriodicWithDelay(feedProcessor.Process, feedProcessorDelay, ctSource.Token);
                    allTasks.Add(processorTask);
                }

                var feedItemMatchProcessor = new FeedItemMatchProcessor(feedItemQueue, torrentQueue);
                allTasks.Add(feedItemMatchProcessor.Process(ctSource.Token));

                var tixatiClient = new TixatiHttpClient(config.TixatiBaseUrl);
                var torrentProcessor = new TorrentProcessor(tixatiClient, torrentQueue);
                allTasks.Add(torrentProcessor.Process(ctSource.Token));

                tasks = allTasks.ToArray();

                isStarted = true;
            }
        }

        public void Stop()
        {
            if (isStarted)
            {
                logger.Info("Stopping");

                // Signal cancellation and wait for all tasks to complete.
                // Once this is done, we can safely read out state information
                // out of the data processors and queues
                CancelTasksAndLogExceptions();

                // Prevent any further additions to the queues
                feedItemQueue.CompleteAdding();
                torrentQueue.CompleteAdding();

                UpdateSettings();
                MonitorConfig.WriteToFile(config);

                isStarted = false;
            }
        }

        private void CancelTasksAndLogExceptions()
        {
            ctSource.Cancel();
            try
            {
                Task.WaitAll(tasks);
            }
            catch (AggregateException ae)
            {
                ae.Handle(ex =>
                {
                    if (!(ex is TaskCanceledException))
                    {
                        logger.Error(ex);
                    }

                    return true;
                });
            }
        }

        public async Task AddTorrent(
            Uri location,
            string description = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }
            else if (!isStarted)
            {
                throw new InvalidOperationException("The monitor is not running.");
            }

            logger.Debug("Adding torrent: \"{0}\"", location);
            var torrent = new Torrent(location, true, description);
            await torrentQueue.EnqueueAsync(torrent, cancellationToken).ConfigureAwait(false);
        }

        public static TorrentMonitor FromSettingsFile(string filePath)
        {
            var settings = MonitorConfig.ReadFromFile(filePath);
            return new TorrentMonitor(settings);
        }

        public IReadOnlyCollection<MatchPattern> GetPatterns()
        {
            return config.Patterns;
        }

        private void UpdateSettings()
        {
            config.FeedItemMatches = feedItemQueue.GetConsumingEnumerable().ToList();
            config.Torrents = torrentQueue.GetConsumingEnumerable().ToList();

            for (int i = 0; i < feedProcessors.Count; ++i)
            {
                config.Feeds[i].LastItemId = feedProcessors[i].LastItemId;
            }
        }
    }
}
