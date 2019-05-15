using System;
using System.Collections.Generic;
using System.IO;
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
        [JsonIgnore]
        public string FilePath { get; set; }

        [JsonProperty(Required = Required.Always)]
        public Uri TixatiBaseUrl { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int FeedUpdateFrequency { get; set; }

        [JsonProperty(Required = Required.Always)]
        public List<FeedInfo> Feeds { get; set; }

        [JsonProperty(Required = Required.Always)]
        public List<MatchPattern> Patterns { get; set; }

        [JsonProperty(Required = Required.Always)]
        public List<FeedItemMatch> FeedItemMatches { get; set; }

        [JsonProperty(Required = Required.Always)]
        public List<Torrent> Torrents { get; set; }

        public static MonitorConfig ReadFromFile(string filePath)
        {
            using (StreamReader file = File.OpenText(filePath))
            {
                var serializer = new JsonSerializer();
                var reader = new JsonTextReader(file);
                var config = serializer.Deserialize<MonitorConfig>(reader);
                config.FilePath = filePath;

                return config;
            }
        }

        public static void WriteToFile(MonitorConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            using (StreamWriter file = File.CreateText(config.FilePath))
            {
                var settings = new JsonSerializerSettings()
                {
                    Formatting = Newtonsoft.Json.Formatting.Indented
                };
                JsonSerializer serializer = JsonSerializer.Create(settings);
                serializer.Serialize(file, config);
            }
        }
    }
}
