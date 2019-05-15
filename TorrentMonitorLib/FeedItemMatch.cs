using System;

namespace TorrentMonitorLib
{
    class FeedItemMatch
    {
        /// <summary>
        /// Gets the item title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the item URL.
        /// </summary>
        public Uri Url { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedItemMatch" /> class.
        /// </summary>
        /// <param name="title">Item title.</param>
        /// <param name="url">Item URL.</param>
        public FeedItemMatch(string title, Uri url)
        {
            Title = title;
            Url = url;
        }
    }
}
