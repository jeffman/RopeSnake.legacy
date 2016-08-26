using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RopeSnake.Core
{
    public sealed class ByteArrayJsonConverter : JsonConverter
    {
        public bool OnSameLine { get; set; } = true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(byte[]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                return Convert.FromBase64String((string)reader.Value);
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                var byteList = new List<byte>();

                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonToken.Integer:
                            byteList.Add(Convert.ToByte(reader.Value));
                            break;

                        case JsonToken.EndArray:
                            return byteList.ToArray();

                        case JsonToken.Comment:
                            break;

                        default:
                            throw new Exception($"Unexpected token: {reader.Path}");
                    }
                }
            }

            throw new JsonException($"Expected byte array or base64 string: {reader.Path}");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var array = value as byte[];

            if (array == null)
            {
                writer.WriteNull();
            }
            else
            {
                if (OnSameLine)
                {
                    writer.WriteRawValue($"[{string.Join(", ", array)}]");
                }
                else
                {
                    writer.WriteStartArray();
                    foreach (byte b in array)
                    {
                        writer.WriteToken(JsonToken.Integer, b);
                    }
                    writer.WriteEndArray();
                }
            }
        }
    }
}
