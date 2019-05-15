using System;
using System.Threading;
using System.Threading.Tasks;

namespace TorrentMonitorLib
{
    static class TaskExtensions
    {
        public static async Task PeriodicWithDelay(
            Func<CancellationToken, Task> taskFunc,
            TimeSpan delay,
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await taskFunc(cancellationToken).ConfigureAwait(false);
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
