using System;
using Newtonsoft.Json;

namespace TorrentMonitorLib.Settings
{
    /// <summary>
    /// Custom converter for serializing a <see cref="MatchPattern"/>.
    /// </summary>
    class MatchPatternConverter : JsonConverter<MatchPattern>
    {
        public override void WriteJson(JsonWriter writer, MatchPattern value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override MatchPattern ReadJson(JsonReader reader, Type objectType, MatchPattern existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string s = (string)reader.Value;
            return new MatchPattern(s);
        }
    }
}
