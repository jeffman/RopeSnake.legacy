using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RopeSnake.Mother3.Text
{
    [JsonConverter(typeof(ContextStringConverter))]
    public sealed class ContextString : IEquatable<ContextString>
    {
        [JsonProperty]
        public string String { get; private set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Context Context { get; private set; }

        internal ContextString() { }

        public ContextString(string str)
        {
            String = str;
            Context = Context.None;
        }

        public ContextString(string str, Context context)
        {
            String = str;
            Context = context;
        }

        public override string ToString()
        {
            if (Context == Context.None)
                return String;
            else
                return "{ \"" + String + "\", " + Context.ToString() + "}";
        }

        public bool Equals(ContextString other)
        {
            return Context == other.Context && String == other.String;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(ContextString))
                return false;

            ContextString other = (ContextString)obj;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Context.GetHashCode() ^ String.GetHashCode();
        }
    }

    public class ContextStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ContextString);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                return new ContextString((string)reader.Value);
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                JObject jObject = JObject.Load(reader);
                ContextString str = new ContextString();
                serializer.Populate(jObject.CreateReader(), str);
                return str;
            }

            throw new JsonReaderException("Could not convert to ContextString");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            ContextString str = value as ContextString;
            if (str.Context == Context.None)
            {
                serializer.Serialize(writer, str.String);
            }
            else
            {
                serializer.Serialize(writer, str);
            }
        }
    }

    public enum Context
    {
        None = 0,
        Normal,
        Saturn
    }
}
