using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TixatiLib
{
    public interface ITorrentClient
    {
        Task AddFileAsync(string torrentFilePath, bool autoStart, CancellationToken cancellationToken);

        Task AddLinkAsync(string torrentUrl, bool autoStart, CancellationToken cancellationToken);

        Task<bool> PingAsync(CancellationToken cancellationToken);

        Task<List<TorrentInfo>> GetTorrentsAsync(CancellationToken cancellationToken);

        Task<List<TorrentFileInfo>> GetTorrentFilesAsync(TorrentInfo torrent, CancellationToken cancellationToken);
    }
}
