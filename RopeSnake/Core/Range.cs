using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RopeSnake.Core
{
    /// <summary>
    /// Represents a range of memory.
    /// </summary>
    [JsonConverter(typeof(RangeConverter))]
    public struct Range
    {
        /// <summary>
        /// Start location of this range. Inclusive.
        /// </summary>
        public readonly int Start;

        /// <summary>
        /// End location of this range. Inclusive.
        /// </summary>
        public readonly int End;

        /// <summary>
        /// Gets the size of the range.
        /// </summary>
        public int Size { get { return End - Start + 1; } }

        private Range(int start, int end)
        {
            if (start < 0 || end < 0)
                throw new Exception("Locations may not be negative");

            if (end < start)
                throw new Exception($"{nameof(end)} must not be less than {nameof(start)}");
            Start = start;
            End = end;
        }

        /// <summary>
        /// Creates a <see cref="Range"/> using the specified start and end locations.
        /// </summary>
        /// <param name="start">start location, inclusive</param>
        /// <param name="end">end location, inclusive</param>
        /// <returns></returns>
        public static Range StartEnd(int start, int end) => new Range(start, end);

        /// <summary>
        /// Creates a <see cref="Range"/> using the specified start location and size.
        /// </summary>
        /// <param name="start">start location, inclusive</param>
        /// <param name="size">desired size of range</param>
        /// <returns></returns>
        public static Range StartSize(int start, int size) => new Range(start, start + size - 1);

        /// <summary>
        /// Combines two <see cref="Range"/>s.
        /// </summary>
        /// <param name="first">the first range to combine</param>
        /// <param name="second">the second range to combine</param>
        /// <returns>the combined <see cref="Range"/></returns>
        public static Range Combine(Range first, Range second)
        {
            if (!first.CanCombineWith(second))
                throw new Exception("Cannot combine first range with second. Perhaps they don't overlap?"
                    + Environment.NewLine
                    + $"First: {first.ToString()}, second: {second.ToString()}");

            Range lower;
            Range upper;
            SortRanges(first, second, out lower, out upper);

            return StartEnd(lower.Start, Math.Max(lower.End, upper.End));
        }

        /// <summary>
        /// Combines the current <see cref="Range"/> with another one.
        /// </summary>
        /// <param name="other">the range to combine with</param>
        /// <returns>the combined range</returns>
        public Range CombineWith(Range other)
            => Combine(this, other);

        /// <summary>
        /// Checks whether two <see cref="Range"/>s may be combined into one.
        /// </summary>
        /// <param name="other">the other <see cref="Range"/> to compare with</param>
        /// <returns><c>true</c> if this <see cref="Range"/> may be combined with <paramref name="other"/>,
        /// <c>false</c> otherwise</returns>
        public bool CanCombineWith(Range other)
        {
            Range lower;
            Range upper;
            SortRanges(this, other, out lower, out upper);

            return upper.Start <= (lower.End + 1);
        }

        public static Range Parse(string rangeString)
        {
            // Format: [start,end]
            // Where start and end may be hex (prefixed with 0x), and there may be
            // whitespace in between any two tokens
            string trimmed = rangeString.Trim();

            // First and last chars must be []
            if (rangeString[0] != '[')
            {
                throw new Exception("Expected opening square bracket");
            }

            if (rangeString[rangeString.Length - 1] != ']')
            {
                throw new Exception("Expected closing square bracket");
            }

            // Get the insides of the brackets and split by comma
            string[] insideSplit = trimmed.Substring(1, trimmed.Length - 2)
                .Split(',')
                .Select(s => s.Trim())
                .ToArray();

            if (insideSplit.Length != 2)
            {
                throw new Exception("Expected exactly two comma-separated values inside square brackets");
            }

            int[] numbers = insideSplit.Select(s => ParseNumber(s)).ToArray();

            return StartEnd(numbers[0], numbers[1]);
        }

        private static int ParseNumber(string number)
        {
            bool hexMode = number.StartsWith("0x") || number.StartsWith("0X");
            int numberBase = hexMode ? 16 : 10;
            int numberStart = hexMode ? 2 : 0;

            return Convert.ToInt32(number.Substring(numberStart), numberBase);
        }

        /// <summary>
        /// Gets the aligned size of this range.
        /// </summary>
        /// <param name="align">alignment to use</param>
        /// <returns>aligned size</returns>
        public int GetAlignedSize(int align)
            => End - Start.Align(align) + 1;

        internal static void SortRanges(Range first, Range second, out Range lower, out Range upper)
        {
            if (first.Start <= second.Start)
            {
                lower = first;
                upper = second;
            }
            else
            {
                lower = second;
                upper = first;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[{Start}, {End}]";
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Start.GetHashCode() ^ End.GetHashCode();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is Range && this == (Range)obj;
        }

        /// <inheritdoc/>
        public static bool operator ==(Range first, Range second)
            => first.Start == second.Start && first.End == second.End;

        /// <inheritdoc/>
        public static bool operator !=(Range first, Range second)
            => !(first == second);
    }

    class RangeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Range);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);

            if (token.Type != JTokenType.String)
            {
                throw new JsonSerializationException("Expected string type");
            }

            string rangeString = token.Value<string>();
            return Range.Parse(rangeString);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType() != typeof(Range))
            {
                throw new InvalidOperationException("Value must be a Range");
            }

            Range range = (Range)value;
            string rangeString = $"[0x{range.Start:X}, 0x{range.End:X}]";
            writer.WriteValue(rangeString);
        }
    }
}
