using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TixatiLib
{
    class TorrentInfoParser : ParserBase
    {
        private const int NameColIdx = 1;
        private const int BytesColIdx = 2;
        private const int PercentColIdx = 3;
        private const int StatusColIdx = 4;
        private const int PriorityColIdx = 7;
        private const int TimeLeftColIdx = 8;

        private static readonly Regex StatusRegex =
            new Regex(@"^(?<status>[A-Za-z]+)", PatternOptions);

        private static readonly Regex IdRegex =
            new Regex(@"\/transfers\/(?<id>.*)\/details$", PatternOptions);

        private static readonly Dictionary<string, TorrentPriority> PrioritiesMap =
            new Dictionary<string, TorrentPriority>()
        {
            {"UL", TorrentPriority.UltraLow},
            {"VL", TorrentPriority.VeryLow},
            {"L", TorrentPriority.Low},
            {"BN", TorrentPriority.BelowNormal},
            {"Normal", TorrentPriority.Normal},
            {"AN", TorrentPriority.AboveNormal},
            {"H", TorrentPriority.High},
            {"VH", TorrentPriority.VeryHigh},
            {"UH", TorrentPriority.UltraHigh},
        };

        public List<TorrentInfo> ParseTorrents(string html)
        {
            return base.ParseList<TorrentInfo>(html);
        }

        protected override object Parse()
        {
            return new TorrentInfo(
                ParseId(),
                ParseName(),
                ParseSizeInMegabytes(),
                ParsePercentDone(),
                ParseStatus(),
                ParsePriority(),
                ParseTimeLeft());
        }

        private string ParseName()
        {
            return ParseColumnValue(NameColIdx, value => value);
        }

        private string ParseId()
        {
            if (columns?.Count > NameColIdx)
            {
                var link = columns[NameColIdx].SelectSingleNode("a");
                var hrefStr = link.Attributes["href"].Value;
                var match = IdRegex.Match(hrefStr);
                return match.Groups["id"].Value;
            }

            return default;
        }

        private float ParseSizeInMegabytes()
        {
            return ParseColumnValue(BytesColIdx, value => GetSizeInMegabytes(value));
        }

        private int ParsePercentDone()
        {
            return ParseColumnValue(PercentColIdx, value =>
            {
                int.TryParse(value, out int percent);
                return percent;
            });
        }

        private TorrentStatus ParseStatus()
        {
            return ParseColumnValue(StatusColIdx, value =>
            {
                var match = StatusRegex.Match(value);
                Enum.TryParse(match.Groups["status"].Value, out TorrentStatus status);
                return status;
            });
        }

        private TorrentPriority ParsePriority()
        {
            return ParseColumnValue(PriorityColIdx, value =>
            {
                PrioritiesMap.TryGetValue(value, out TorrentPriority priority);
                return priority;
            });
        }

        private TimeSpan ParseTimeLeft()
        {
            return ParseColumnValue(TimeLeftColIdx, value =>
            {
                TimeSpan.TryParse(value, out TimeSpan timeLeft);
                return timeLeft;
            });
        }
    }
}
