using System.Threading;
using System.Threading.Tasks;

namespace TorrentMonitorLib
{
    interface ITorrentClient
    {
        Task AddFileAsync(string torrentFilePath, bool autoStart, CancellationToken cancellationToken);

        Task AddLinkAsync(string torrentUrl, bool autoStart, CancellationToken cancellationToken);
    }
}
