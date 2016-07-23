using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using RopeSnake.Gba;
using Newtonsoft.Json;

namespace RopeSnake.Mother3
{
    public sealed class Mother3RomConfig
    {
        [JsonProperty]
        public string Version { get; private set; }

        [JsonProperty]
        public Dictionary<string, HashSet<int>> References { get; private set; }

        [JsonProperty]
        public List<Range> FreeRanges { get; private set; }

        [JsonProperty]
        public Dictionary<string, object> Parameters { get; private set; }

        public IEnumerable<int> GetReferences(string key)
        {
            return References[key];
        }

        public int GetOffset(string key, Block romData)
        {
            var references = GetReferences(key);
            var offsets = new HashSet<int>();
            var stream = romData.ToBinaryStream();

            foreach (int reference in references)
            {
                stream.Position = reference;
                int offset = stream.ReadGbaPointer();
                offsets.Add(offset);
            }

            if (offsets.Count != 1)
                throw new Exception("Inconsistent offsets");

            return offsets.First();
        }

        public T GetParameter<T>(string key)
        {
            return (T)Convert.ChangeType(Parameters[key], typeof(T));
        }

        public void SetParameter(string key, object value)
        {
            Parameters[key] = value;
        }
    }
}
