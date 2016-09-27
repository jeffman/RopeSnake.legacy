using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RopeSnake.Graphics
{
    [JsonConverter(typeof(ColorConverter))]
    public struct Color : IEquatable<Color>
    {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;

        [JsonIgnore]
        public readonly uint Argb;
        
        public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
            Argb = (uint)(b | (g << 8) | (r << 16)) | 0xFF000000;
        }

        public bool Equals(Color other)
        {
            return
                R == other.R &&
                G == other.G &&
                B == other.B;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Color))
                return false;

            return Equals((Color)obj);
        }

        public override int GetHashCode()
        {
            return (int)Argb;
        }

        public static bool operator ==(Color first, Color second) => first.Equals(second);

        public static bool operator !=(Color first, Color second) => !(first == second);

        public override string ToString()
        {
            return $"R: {R}, G: {G}, B: {B}";
        }
    }

    public sealed class ColorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Color);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                byte[] channels = new byte[3];
                for (int i = 0; i < 3; i++)
                {
                    if (!reader.Read())
                        throw new JsonException($"Reader ended unexpectedly: {reader.Path}");

                    if (reader.TokenType != JsonToken.Integer)
                        throw new JsonException($"Expected integer value: {reader.Path}");

                    channels[i] = Convert.ToByte(reader.Value);
                }

                if (!reader.Read())
                    throw new JsonException($"Reader ended unexpectedly: {reader.Path}");

                if (reader.TokenType != JsonToken.EndArray)
                    throw new JsonException($"Expected end of array after 3 elements: {reader.Path}");

                return new Color(channels[0], channels[1], channels[2]);
            }
            else
            {
                throw new JsonException($"Could not parse color: {reader.Path}");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var color = (Color)value;
            writer.WriteRawValue($"[{color.R}, {color.G}, {color.B}]");
        }
    }
}
