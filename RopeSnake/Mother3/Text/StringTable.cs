using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RopeSnake.Mother3.Text
{
    public sealed class StringTable
    {
        public ushort MaxLength { get; set; }

        [JsonProperty]
        public List<string> Strings { get; private set; } = new List<string>();
    }
}
