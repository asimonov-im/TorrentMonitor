using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace TorrentMonitorLib
{
    /// <summary>
    /// Communicates with Tixati using its web interface.
    /// </summary>
    class TixatiHttpClient : ITorrentClient
    {
        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly Uri ActionEndpoint = new Uri("transfers/action", UriKind.Relative);
        private static readonly TimeSpan HttpClientTimeout = TimeSpan.FromSeconds(10);

        private readonly HttpClient client;
        private readonly Uri postUri;

        public TixatiHttpClient(Uri serverUri)
        {
            if (!serverUri.IsAbsoluteUri)
            {
                throw new ArgumentException($"The {nameof(serverUri)} must be absolute.");
            }

            this.client = new HttpClient()
            {
                Timeout = HttpClientTimeout
            };
            this.postUri = new Uri(serverUri, ActionEndpoint);
        }

        // https://stackoverflow.com/questions/20319886/http-multipartformdatacontent
        public async Task AddFileAsync(string torrentFilePath, bool autoStart, CancellationToken cancellationToken)
        {
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

            await PostAndValidateAsync(formDataContent, cancellationToken).ConfigureAwait(false);
        }

        public async Task AddLinkAsync(string torrentUrl, bool autoStart, CancellationToken cancellationToken)
        {
            logger.ConditionalDebug("Adding torrent URL: \"{0}\"", torrentUrl);

            var values = new Dictionary<string, string>
            {
               { "addlinktext", torrentUrl },
               { "addlink", "Add" },
               { "noautostart", autoStart ? "0" : "1" }
            };

            var content = new FormUrlEncodedContent(values);

            await PostAndValidateAsync(content, cancellationToken).ConfigureAwait(false);
        }

        private async Task PostAndValidateAsync(HttpContent content, CancellationToken cancellationToken)
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
