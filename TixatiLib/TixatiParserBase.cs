using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace TixatiLib
{
    abstract class ParserBase
    {
        protected HtmlNodeCollection columns;

        protected static readonly RegexOptions PatternOptions =
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant |
            RegexOptions.ExplicitCapture;

        private static readonly Regex SizeRegex =
            new Regex(@"of (?<size>\d+(\.\d+)?) (?<unit>[KMGT]i?)$", PatternOptions);

        private static readonly Dictionary<string, long> UnitMultipliers =
            new Dictionary<string, long>()
        {
            {"K", 1_000},
            {"Ki", 1_024},
            {"M", 1_000_000},
            {"Mi", 1_048_576},
            {"G", 1_000_000_000},
            {"Gi", 1_073_741_824},
            {"T", 1_000_000_000_000},
            {"Ti", 1_099_511_657_776},
        };

        protected List<T> ParseList<T>(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var table = doc.DocumentNode.SelectSingleNode("//table[@class='xferstable']");
            var tableRows = table.SelectNodes("tr");

            var result = new List<T>(tableRows.Count - 1);
            for (int r = 1; r < tableRows.Count; r++)
            {
                columns = tableRows[r].SelectNodes("td");
                T parseResult = (T)Parse();
                result.Add(parseResult);
            }

            return result;
        }

        protected abstract object Parse();

        protected static float GetSizeInMegabytes(string sizeStr)
        {
            var match = SizeRegex.Match(sizeStr);

            float.TryParse(match.Groups["size"].Value, out float size);
            UnitMultipliers.TryGetValue(match.Groups["unit"].Value, out long multiplier);

            float sizeBytes = size * multiplier;
            return sizeBytes / UnitMultipliers["Mi"];
        }

        protected T ParseColumnValue<T>(int colIdx, Func<string, T> parser)
        {
            if (columns?.Count > colIdx)
            {
                var colValue = columns[colIdx].DecodedInnerText();
                return parser(colValue);
            }

            return default;
        }
    }
}
