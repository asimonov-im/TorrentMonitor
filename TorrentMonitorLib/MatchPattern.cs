using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using TorrentMonitorLib.Settings;

namespace TorrentMonitorLib
{
    [JsonConverter(typeof(MatchPatternConverter))]
    public readonly struct MatchPattern
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
            Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
            CompiledPattern = new Regex(pattern, PatternOptions);
        }

        public override string ToString() => Pattern;
    }
}
