using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RopeSnake.Mother3.Maps
{
    public sealed class LayerInfo
    {
        public byte Width { get; set; }
        public byte Height { get; set; }
        public byte UnknownFields { get; set; }

        [JsonProperty]
        public byte[] Unknown { get; private set; } = new byte[3];
    }
}
