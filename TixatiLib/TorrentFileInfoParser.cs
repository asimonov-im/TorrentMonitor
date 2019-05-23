using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TixatiLib
{
    class TorrentFileInfoParser : ParserBase
    {
        private const int NameColIdx = 1;
        private const int PriorityColIdx = 2;
        private const int BytesColIdx = 3;
        private const int StatusColIdx = 4;

        private static readonly Regex StatusRegex =
            new Regex(@"^(?<percent>\d+)%", PatternOptions);

        private static readonly Dictionary<string, TorrentPriority> PrioritiesMap =
            new Dictionary<string, TorrentPriority>()
        {
            {"Off", TorrentPriority.Off},
            {"Ultra Low", TorrentPriority.UltraLow},
            {"Very Low", TorrentPriority.VeryLow},
            {"Low", TorrentPriority.Low},
            {"Below Normal", TorrentPriority.BelowNormal},
            {"Normal", TorrentPriority.Normal},
            {"Above Normal", TorrentPriority.AboveNormal},
            {"High", TorrentPriority.High},
            {"Very High", TorrentPriority.VeryHigh},
            {"Ultra High", TorrentPriority.UltraHigh},
        };

        public List<TorrentFileInfo> ParseTorrentFiles(string html)
        {
            return base.ParseList<TorrentFileInfo>(html);
        }

        protected override object Parse()
        {
            return new TorrentFileInfo(
                ParseName(),
                ParseDownloadUri(),
                ParsePriority(),
                ParseSizeInMegabytes(),
                ParsePercentDone());
        }

        private string ParseName()
        {
            return ParseColumnValue(NameColIdx, value => value);
        }

        private Uri ParseDownloadUri()
        {
            if (columns?.Count > NameColIdx)
            {
                var link = columns[NameColIdx].SelectSingleNode("a");
                var hrefStr = link.Attributes["href"].Value;
                Uri.TryCreate(hrefStr, UriKind.Relative, out Uri downloadUri);
                return downloadUri;
            }

            return default;
        }

        private TorrentPriority ParsePriority()
        {
            return ParseColumnValue(PriorityColIdx, value =>
            {
                PrioritiesMap.TryGetValue(value, out TorrentPriority priority);
                return priority;
            });
        }

        private float ParseSizeInMegabytes()
        {
            return ParseColumnValue(BytesColIdx, value => GetSizeInMegabytes(value));
        }

        private int ParsePercentDone()
        {
            return ParseColumnValue(StatusColIdx, value =>
            {
                var match = StatusRegex.Match(value);
                int.TryParse(match.Groups["percent"].Value, out int percent);
                return percent;
            });
        }
    }
}
