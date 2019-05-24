using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace TixatiLib
{
    /// <summary>
    /// Communicates with Tixati using its web interface.
    /// </summary>
    public class TixatiClient : ITorrentClient
    {
        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly Uri ActionEndpoint = new Uri("transfers/action", UriKind.Relative);
        private static readonly Uri TransfersEndpoint = new Uri("transfers", UriKind.Relative);
        private static readonly TimeSpan HttpClientTimeout = TimeSpan.FromSeconds(10);

        private readonly HttpClient client;
        private readonly Uri serverUri;
        private readonly Uri transfersActionUri;
        private readonly Uri transfersUri;

        public TixatiClient(Uri serverUri)
        {
            if (!serverUri.IsAbsoluteUri)
            {
                throw new ArgumentException($"The {nameof(serverUri)} must be absolute.");
            }

            this.client = new HttpClient()
            {
                Timeout = HttpClientTimeout
            };
            this.serverUri = serverUri;
            this.transfersUri = new Uri(serverUri, TransfersEndpoint);
            this.transfersActionUri = new Uri(serverUri, ActionEndpoint);
        }

        // https://stackoverflow.com/questions/20319886/http-multipartformdatacontent
        public async Task AddFileAsync(string torrentFilePath, bool autoStart, CancellationToken cancellationToken)
        {
            if (torrentFilePath == null)
            {
                throw new ArgumentNullException(nameof(torrentFilePath));
            }

            logger.ConditionalDebug("Adding torrent file: \"{0}\"", torrentFilePath);

            var fileStream = File.OpenRead(torrentFilePath);
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"metafile\"",
                FileName = $"\"{Path.GetFileName(torrentFilePath)}\"",
            };
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var formDataContent = CreateMultipartFormDataContent();
            formDataContent.Add(streamContent);
            formDataContent.Add(new StringContent("Add"), "\"addmetafile\"");
            formDataContent.Add(new StringContent(autoStart ? "0" : "1"), "\"noautostart\"");

            await PostAndValidateAsync(transfersActionUri, formDataContent, cancellationToken).ConfigureAwait(false);
        }

        public async Task AddLinkAsync(string torrentUrl, bool autoStart, CancellationToken cancellationToken)
        {
            if (torrentUrl == null)
            {
                throw new ArgumentNullException(nameof(torrentUrl));
            }

            logger.ConditionalDebug("Adding torrent URL: \"{0}\"", torrentUrl);

            var values = new Dictionary<string, string>
            {
               { "addlinktext", torrentUrl },
               { "addlink", "Add" },
               { "noautostart", autoStart ? "0" : "1" }
            };

            var content = new FormUrlEncodedContent(values);

            await PostAndValidateAsync(transfersActionUri, content, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> PingAsync(CancellationToken cancellationToken)
        {
            logger.ConditionalTrace("Pinging server");

            try
            {
                using (var response = await client.GetAsync(serverUri, cancellationToken).ConfigureAwait(false))
                {
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex) when (!(ex is TaskCanceledException))
            {
                return false;
            }

        }

        public async Task<List<TorrentInfo>> GetTorrentsAsync(CancellationToken cancellationToken)
        {
            logger.ConditionalDebug("Getting torrents");

            var content = await GetContentAndValidateAsync(transfersUri, cancellationToken).ConfigureAwait(false);
            var parser = new TorrentInfoParser();

            return parser.ParseTorrents(content);
        }

        public async Task<List<TorrentFileInfo>> GetTorrentFilesAsync(TorrentInfo torrent, CancellationToken cancellationToken)
        {
            if (torrent == null)
            {
                throw new ArgumentNullException(nameof(torrent));
            }

            logger.ConditionalDebug("Getting \"{0}\" torrent files", torrent.Name);

            var torrentFilesUri = new Uri(transfersUri, $"{torrent.Id}/files");
            var content = await GetContentAndValidateAsync(torrentFilesUri, cancellationToken).ConfigureAwait(false);
            var parser = new TorrentFileInfoParser();

            return parser.ParseTorrentFiles(content);
        }

        private async Task<string> GetContentAndValidateAsync(Uri getUri, CancellationToken cancellationToken)
        {
            var getTask = client.GetAsync(getUri, cancellationToken).ConfigureAwait(false);
            using (var response = await getTask)
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to GET with {response.StatusCode} status code.");
                }

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        private async Task PostAndValidateAsync(Uri postUri, HttpContent content, CancellationToken cancellationToken)
        {
            var postTask = client.PostAsync(postUri, content, cancellationToken).ConfigureAwait(false);
            using (var response = await postTask)
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to POST with {response.StatusCode} status code.");
                }
            }
        }

        private static MultipartFormDataContent CreateMultipartFormDataContent()
        {
            // Use a custom boundary, for workaround
            var boundary = Guid.NewGuid().ToString();
            var content = new MultipartFormDataContent(boundary);

            // HttpClient will quote the boundary in the Content-Type header, which is valid
            // per https://tools.ietf.org/html/rfc2046#section-5.1.1
            // Unfortunately, this will result in Tixati failing to parse the request,
            // requiring the following workaround.
            content.Headers.Remove("Content-Type");
            content.Headers.TryAddWithoutValidation("Content-Type", $"multipart/form-data; boundary={boundary}");

            return content;
        }
    }
}
