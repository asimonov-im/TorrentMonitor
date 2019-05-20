using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TorrentMonitorLib
{
    class MonitorState
    {
        /// <summary>
        /// Gets or sets the path from which the object was deserialized.
        /// </summary>
        [JsonIgnore]
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the dictionary containing the last item processed for each feed.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Dictionary<Uri, string> LastProcessedIds { get; set; }

        [JsonProperty(Required = Required.Always)]
        public List<FeedItemMatch> FeedItemMatches { get; set; }

        [JsonProperty(Required = Required.Always)]
        public List<Torrent> Torrents { get; set; }

        public MonitorState()
        {
            LastProcessedIds = new Dictionary<Uri, string>();
            FeedItemMatches = new List<FeedItemMatch>();
            Torrents = new List<Torrent>();
        }
    }
}
