using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NLog;
using TixatiLib;

namespace TorrentMonitorLib
{
    public class TixatiClientWithStatus : ITorrentClient
    {
        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ITorrentClient client;

        public AsyncManualResetEvent Status { get; }

        public TixatiClientWithStatus(ITorrentClient client, bool setOnline)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.Status = new AsyncManualResetEvent(setOnline);
        }

        public async Task AddFileAsync(string torrentFilePath, bool autoStart, CancellationToken cancellationToken)
        {
            try
            {
                await client.AddFileAsync(torrentFilePath, autoStart, cancellationToken).ConfigureAwait(false);
                UpdateStatus(true);
            }
            catch (Exception ex) when (!(ex is TaskCanceledException))
            {
                UpdateStatus(false);
                throw;
            }
        }

        public async Task AddLinkAsync(string torrentUrl, bool autoStart, CancellationToken cancellationToken)
        {
            try
            {
                await client.AddLinkAsync(torrentUrl, autoStart, cancellationToken).ConfigureAwait(false);
                UpdateStatus(true);
            }
            catch (Exception ex) when (!(ex is TaskCanceledException))
            {
                UpdateStatus(false);
                throw;
            }
        }

        public async Task<List<TorrentFileInfo>> GetTorrentFilesAsync(TorrentInfo torrent, CancellationToken cancellationToken)
        {
            try
            {
                var result = await client.GetTorrentFilesAsync(torrent, cancellationToken).ConfigureAwait(false);
                UpdateStatus(true);

                return result;
            }
            catch (Exception ex) when (!(ex is TaskCanceledException))
            {
                UpdateStatus(false);
                throw;
            }
        }

        public async Task<List<TorrentInfo>> GetTorrentsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var result = await client.GetTorrentsAsync(cancellationToken).ConfigureAwait(false);
                UpdateStatus(true);

                return result;
            }
            catch (Exception ex) when (!(ex is TaskCanceledException))
            {
                UpdateStatus(false);
                throw;
            }
        }

        public async Task<bool> PingAsync(CancellationToken cancellationToken)
        {
            bool result = await client.PingAsync(cancellationToken).ConfigureAwait(false);
            UpdateStatus(result);

            return result;
        }

        private void UpdateStatus(bool isReachable)
        {
            if (isReachable)
            {
                Status.Set();
            }
            else
            {
                Status.Reset();
            }
        }
    }
}
