using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TorrentMonitorLib
{
    static class AsyncExtensions
    {
        public static async Task PeriodicTaskWithDelay(
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

        /// <summary>
        /// Asynchronously writes the specified text to a file, using the UTF8 encoding.
        /// Overwrites all existing file contents.
        /// </summary>
        /// <param name="filePath">File path to write to.</param>
        /// <param name="text">Text to write.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to abort the operation.</param>
        public static async Task WriteTextAsync(string filePath, string text, CancellationToken cancellationToken = default)
        {
            var mode = FileMode.OpenOrCreate;
            var access = FileAccess.Write;
            var share = FileShare.None;
            int bufferSize = 4096;
            var useAsync = true;
            using (var file = new FileStream(filePath, mode, access, share, bufferSize, useAsync))
            {
                byte[] encodedText = Encoding.UTF8.GetBytes(text);
                await file.WriteAsync(encodedText, 0, encodedText.Length, cancellationToken).ConfigureAwait(false);
            };
        }

        /// <summary>
        /// Asynchronously reads the contents of the specified UTF8-encoded file.
        /// </summary>
        /// <param name="filePath">File to read.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to abort the operation.</param>
        public static async Task<string> ReadTextAsync(string filePath, CancellationToken cancellationToken = default)
        {
            var mode = FileMode.Open;
            var access = FileAccess.Read;
            var share = FileShare.Read;
            int bufferSize = 4096;
            var useAsync = true;
            using (var file = new FileStream(filePath, mode, access, share, bufferSize, useAsync))
            using (var reader = new StreamReader(file, Encoding.UTF8))
            {
                // Use a StreamReader to correctly handle multibyte encoding.
                // Unfortunately, StreamReader async operations are not cancellable,
                // but we might want to try emulating the behavior via a ReadAsync loop that checks
                // the cancellation token on every iteration.
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }
    }
}
