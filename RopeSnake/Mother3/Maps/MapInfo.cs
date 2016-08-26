using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RopeSnake.Core;

namespace RopeSnake.Mother3.Maps
{
    public sealed class MapInfo
    {
        public static readonly int FieldSize = 28;

        [JsonProperty(Order = -1, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string NameHint { get; set; }

        [JsonProperty]
        public byte[] Alpha { get; private set; } = new byte[2];
        [JsonProperty]
        public LayerInfo[] Layers { get; private set; } = new LayerInfo[3];
        public uint UnknownFields { get; set; }

        [JsonProperty]
        public byte[] Unknown { get; private set; } = new byte[12];
    }
}
