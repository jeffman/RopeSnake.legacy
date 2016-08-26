using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFileSystem;
using Newtonsoft.Json;

namespace RopeSnake.Core
{
    internal sealed class FileSystemPathConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(FileSystemPath);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
                throw new JsonException($"Expected a string: {reader.Path}");

            string path = (string)reader.Value;

            try
            {
                return FileSystemPath.Parse(path);
            }
            catch (Exception ex)
            {
                throw new JsonException($"Could not parse \"{path}\" as valid path: {reader.Path}", ex);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, ((FileSystemPath)value).Path);
        }
    }
}
