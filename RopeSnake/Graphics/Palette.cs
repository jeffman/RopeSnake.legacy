using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RopeSnake.Graphics
{
    [JsonConverter(typeof(PaletteConverter))]
    public class Palette : IEnumerable<Color>
    {
        private Color[,] _colors;
        private int _subPaletteCount;
        private int _colorsPerSubPalette;

        public virtual int SubPaletteCount => _subPaletteCount;
        public virtual int ColorsPerSubPalette => _colorsPerSubPalette;

        public virtual Color this[int subPaletteIndex, int colorIndex]
        {
            get { return _colors[subPaletteIndex, colorIndex]; }
            set { _colors[subPaletteIndex, colorIndex] = value; }
        }

        public Palette(int subPaletteCount, int colorsPerSubPalette)
        {
            if (subPaletteCount < 0)
                throw new ArgumentException(nameof(subPaletteCount));

            if (colorsPerSubPalette < 0)
                throw new ArgumentException(nameof(colorsPerSubPalette));

            _colors = new Color[subPaletteCount, colorsPerSubPalette];
            _subPaletteCount = subPaletteCount;
            _colorsPerSubPalette = colorsPerSubPalette;
        }

        public virtual Color GetColor(int subPaletteIndex, int colorIndex)
            => _colors[subPaletteIndex, colorIndex];

        public virtual void SetColor(int subPaletteIndex, int colorIndex, Color color)
            => _colors[subPaletteIndex, colorIndex] = color;

        public virtual IEnumerator<Color> GetEnumerator()
        {
            for (int subPalette = 0; subPalette < _subPaletteCount; subPalette++)
            {
                for (int color = 0; color < _colorsPerSubPalette; color++)
                {
                    yield return _colors[subPalette, color];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public sealed class PaletteConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Palette);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartArray)
                throw new JsonException($"Expected start of array: {reader.Path}");

            var subPaletteList = new List<List<Color>>();
            int previousColorCount = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndArray)
                    break;

                if (reader.TokenType != JsonToken.StartArray)
                    throw new JsonException($"Expected start of array: {reader.Path}");

                var colorList = new List<Color>();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.EndArray)
                        break;

                    var color = serializer.Deserialize<Color>(reader);
                    colorList.Add(color);
                }

                if (subPaletteList.Count > 0 && colorList.Count != previousColorCount)
                    throw new JsonException($"Inconsistent subpalette counts. Previous subpalette had {previousColorCount} colors, but the current one has {colorList.Count}: {reader.Path}");

                previousColorCount = colorList.Count;
                subPaletteList.Add(colorList);
            }

            var palette = new Palette(subPaletteList.Count, previousColorCount);

            for (int subIndex = 0; subIndex < subPaletteList.Count; subIndex++)
            {
                for (int colorIndex = 0; colorIndex < previousColorCount; colorIndex++)
                {
                    palette[subIndex, colorIndex] = subPaletteList[subIndex][colorIndex];
                }
            }

            return palette;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var palette = value as Palette;

            writer.WriteStartArray();

            for (int subIndex = 0; subIndex < palette.SubPaletteCount; subIndex++)
            {
                writer.WriteStartArray();

                for (int colorIndex = 0; colorIndex < palette.ColorsPerSubPalette; colorIndex++)
                {
                    serializer.Serialize(writer, palette[subIndex, colorIndex]);
                }

                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }
    }
}
