using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace BrianClient
{
    public static class StreamExtensions
    {
        public static T ReadAndDeserializeFromJson<T>(this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new NotSupportedException("Can't read from this stream.");
            }

            using (var streamReader = new StreamReader(stream))
            {
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    var jsonSerializer = new JsonSerializer();
                    return jsonSerializer.Deserialize<T>(jsonTextReader);
                }
            }
        }

        public static void SerializeToJsonAndWrite<T>(this Stream stream, T objectToWrite)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanWrite)
            {
                throw new NotSupportedException("Can't write to this stream.");
            }

            using (var sw = new StreamWriter(stream, new UTF8Encoding(), 1024, true))
            using (var jsonTxtWriter = new JsonTextWriter(sw))
            {
                var jsonSerializer = new JsonSerializer();
                jsonSerializer.Serialize(jsonTxtWriter, objectToWrite);
                jsonTxtWriter.Flush();
            }
        }
    }
}
