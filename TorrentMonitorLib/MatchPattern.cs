using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using TorrentMonitorLib.Settings;

namespace TorrentMonitorLib
{
    [JsonConverter(typeof(MatchPatternConverter))]
    public class MatchPattern
    {
        private static readonly RegexOptions PatternOptions =
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant |
            RegexOptions.IgnoreCase |
            RegexOptions.ExplicitCapture;

        public string Pattern { get; }

        public Regex CompiledPattern { get; }

        public MatchPattern(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw new ArgumentException($"The {nameof(pattern)} must not be null or whitespace."); ;
            }

            Pattern = pattern;
            CompiledPattern = new Regex(pattern, PatternOptions);
        }

        public static MatchPattern TryCreate(string pattern)
        {
            try
            {
                return new MatchPattern(pattern);
            }
            catch
            {
                return null;
            }
        }

        public override string ToString() => Pattern;
    }
}
