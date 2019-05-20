using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TorrentMonitorLib
{
    static class SerializationHelpers
    {
        /// <summary>
        /// Asynchronously deserializes an object from a file.
        /// </summary>
        /// <typeparam name="T">Type of object being deserialized.</typeparam>
        /// <param name="filePath">File path to read.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to abort the operation.</param>
        public static async Task<T> ReadFromFileAsync<T>(string filePath, CancellationToken cancellationToken = default)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            var json = await AsyncExtensions.ReadTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            var value = JsonConvert.DeserializeObject<T>(json);

            return value;
        }

        /// <summary>
        /// Asynchronously serializes the specified object to a file.
        /// </summary>
        /// <typeparam name="T">Type of object being serialized.</typeparam>
        /// <param name="value">Object to serialize.</param>
        /// <param name="filePath">File path to write.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to abort the operation.</param>
        public static async Task WriteToFile<T>(T value, string filePath, CancellationToken cancellationToken = default)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            else if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            var settings = new JsonSerializerSettings()
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };
            var json = JsonConvert.SerializeObject(value, settings);
            await AsyncExtensions.WriteTextAsync(filePath, json, cancellationToken).ConfigureAwait(false);
        }
    }
}
