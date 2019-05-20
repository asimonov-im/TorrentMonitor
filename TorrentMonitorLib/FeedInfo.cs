using System;
using Newtonsoft.Json;

namespace TorrentMonitorLib
{
    /// <summary>
    /// Contains information about an RSS feed to be monitored.
    /// </summary>
    class FeedInfo
    {
        private Uri url;

        /// <summary>
        /// Gets or sets the feed description.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the feed should be monitored.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the feed URL.
        /// </summary>
        public Uri Url
        {
            get => url;
            set => url = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedInfo" /> class.
        /// </summary>
        /// <param name="url">Feed URL.</param>
        [JsonConstructor]
        public FeedInfo(Uri url)
        {
            Url = url;
        }
    }
}
