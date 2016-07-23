using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RopeSnake.Mother3
{
    public sealed class Mother3RomConfig
    {
        [JsonProperty]
        public string Version { get; private set; }

        [JsonProperty]
        public Dictionary<string, HashSet<int>> References { get; private set; }

        public IEnumerable<int> GetReferences(string key)
        {
            return References[key];
        }
    }
}
