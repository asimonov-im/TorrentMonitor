using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NLog;
using TixatiLib;

using static TorrentMonitorLib.AsyncExtensions;
using static TorrentMonitorLib.SerializationHelpers;

namespace TorrentMonitorLib
{
    public class TorrentMonitor : IPatternSource
    {
        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly MonitorConfig config;
        private readonly MonitorState state;

        private bool isStarted;
        private CancellationTokenSource ctSource;
        private Dictionary<Uri, FeedProcessor> feedProcessors;
        private List<Task> tasks;
        private ConcurrentQueue<FeedItemMatch> feedItemQueue;
        private ConcurrentQueue<Torrent> torrentQueue;
        private AsyncCollection<FeedItemMatch> feedItemQueueAsync;
        private AsyncCollection<Torrent> torrentQueueAsync;

        public ImmutableList<MatchPattern> Patterns
        {
            get => config.Patterns;
            set => config.Patterns = value ?? throw new ArgumentNullException(nameof(value));
        }

        private TorrentMonitor(MonitorConfig config, MonitorState state)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.state = state ?? throw new ArgumentNullException(nameof(state));

            feedItemQueue = new ConcurrentQueue<FeedItemMatch>(state.FeedItemMatches);
            feedItemQueueAsync = new AsyncCollection<FeedItemMatch>(feedItemQueue);
            torrentQueue = new ConcurrentQueue<Torrent>(state.Torrents);
            torrentQueueAsync = new AsyncCollection<Torrent>(torrentQueue);
            feedProcessors = new Dictionary<Uri, FeedProcessor>();
            tasks = new List<Task>();
        }

        public void Start()
        {
            if (!isStarted)
            {
                logger.Info("Starting");

                ctSource = new CancellationTokenSource();

                var feedProcessorDelay = TimeSpan.FromSeconds(config.FeedUpdateFrequencySeconds);
                foreach (var feedInfo in config.Feeds)
                {
                    if (!feedInfo.Enabled) continue;

                    var feedItemSource = new FeedItemSource(feedInfo.Url);
                    state.LastProcessedIds.TryGetValue(feedInfo.Url, out var lastItemId);
                    var feedProcessor = new FeedProcessor(feedItemSource, this, feedItemQueueAsync, lastItemId);
                    feedProcessors.Add(feedProcessor.FeedUrl, feedProcessor);

                    var processorTask = PeriodicTaskWithDelay(feedProcessor.Process, feedProcessorDelay, ctSource.Token);
                    tasks.Add(processorTask);
                }

                var feedItemMatchProcessor = new FeedItemMatchProcessor(feedItemQueueAsync, torrentQueueAsync);
                tasks.Add(feedItemMatchProcessor.Process(ctSource.Token));

                var tixatiClient = new TixatiClientWithStatus(new TixatiClient(config.TixatiBaseUrl), true);
                var torrentProcessor = new TorrentProcessor(tixatiClient, tixatiClient.Status, torrentQueueAsync);
                tasks.Add(torrentProcessor.Process(ctSource.Token));

                var tixatiPingDelay = TimeSpan.FromSeconds(config.TixatiPingFrequencySeconds);
                var tixatiPingTask = PeriodicTaskWithDelay(tixatiClient.PingAsync, tixatiPingDelay, ctSource.Token);
                tasks.Add(tixatiPingTask);

                var stateSaveDelay = TimeSpan.FromSeconds(config.StateAutosaveFrequencySeconds);
                var saveStateTask = PeriodicTaskWithDelay(SaveUpdatedState, stateSaveDelay, ctSource.Token);
                tasks.Add(saveStateTask);

                var configSaveDelay = TimeSpan.FromSeconds(config.ConfigAutosaveFrequencySeconds);
                var configStateTask = PeriodicTaskWithDelay(SaveUpdatedConfig, configSaveDelay, ctSource.Token);
                tasks.Add(configStateTask);

                isStarted = true;
            }
        }

        public async Task StopAsync()
        {
            if (isStarted)
            {
                logger.Info("Stopping");

                // Signal cancellation and wait for all tasks to complete.
                // Once this is done, we can safely read out state information
                // out of the data processors and queues
                await CancelTasksAndLogExceptions().ConfigureAwait(false);

                // Save all the things
                var saveConfigTask = SaveUpdatedConfig(CancellationToken.None);
                var saveStateTask = SaveUpdatedState(CancellationToken.None);
                await Task.WhenAll(saveConfigTask, saveStateTask).ConfigureAwait(false);

                // Clear tasks and feed processors so they can be GC-ed
                feedProcessors.Clear();
                tasks.Clear();

                isStarted = false;
            }
        }

        public async Task AddTorrent(
            Uri location,
            string description = null,
            CancellationToken cancellationToken = default)
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
            await torrentQueueAsync.AddAsync(torrent, cancellationToken).ConfigureAwait(false);
        }

        public static async Task<TorrentMonitor> FromDisk(string configFilePath, string stateFilePath)
        {
            var readConfigTask = ReadFromFileAsync<MonitorConfig>(configFilePath);
            var readStateTask = ReadFromFileAsync<MonitorState>(stateFilePath);

            // Failing to load the configuration is a fatal error
            MonitorConfig config = await readConfigTask.ConfigureAwait(false);
            config.FilePath = configFilePath;

            // Failing to load saved state is OK
            MonitorState state;
            try
            {
                state = await readStateTask.ConfigureAwait(false);
            }
            catch (Exception)
            {
                state = new MonitorState();
            }
            state.FilePath = stateFilePath;

            return new TorrentMonitor(config, state);
        }

        public IReadOnlyCollection<MatchPattern> GetPatterns()
        {
            return config.Patterns;
        }

        private async Task CancelTasksAndLogExceptions()
        {
            ctSource.Cancel();

            var allTasks = Task.WhenAll(tasks);
            try
            {
                await allTasks.ConfigureAwait(false);
            }
            catch
            {
                allTasks.Exception?.Handle(ex =>
                {
                    logger.Error(ex);
                    return true;
                });
            }
        }

        private async Task SaveUpdatedState(CancellationToken cancellationToken)
        {
            state.FeedItemMatches = feedItemQueue.ToList();
            state.Torrents = torrentQueue.ToList();
            foreach (var feedProcessor in feedProcessors.Values)
            {
                state.LastProcessedIds[feedProcessor.FeedUrl] = feedProcessor.LastItemId;
            }

            try
            {
                await WriteToFile(state, state.FilePath, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to write state to file.");
            }
        }

        private async Task SaveUpdatedConfig(CancellationToken cancellationToken)
        {
            try
            {
                await WriteToFile(config, config.FilePath, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to write configuration to file.");
            }
        }
    }
}
