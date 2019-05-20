using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TorrentMonitorLib
{
    /// <summary>
    /// Describes the configuration and saved state for the <see cref="TorrentMonitor"/>.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    class MonitorConfig
    {
        /// <summary>
        /// Gets or sets the path from which the object was deserialized.
        /// </summary>
        [JsonIgnore]
        public string FilePath { get; set; }

        [JsonProperty(Required = Required.Always)]
        public Uri TixatiBaseUrl { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int FeedUpdateFrequencySeconds { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int StateAutosaveFrequencySeconds { get; set; }

        [JsonProperty(Required = Required.Always)]
        public List<FeedInfo> Feeds { get; set; }

        [JsonProperty(Required = Required.Always)]
        public List<MatchPattern> Patterns { get; set; }
    }
}
