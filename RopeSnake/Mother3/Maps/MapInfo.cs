using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RopeSnake.Core;
using RopeSnake.Core.Validation;

namespace RopeSnake.Mother3.Maps
{
    [Validate]
    public sealed class MapInfo : INameHint
    {
        public static readonly int FieldSize = 28;

        [JsonProperty(Order = -1, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string NameHint { get; set; }

        [JsonProperty, NotNull, CountEquals(2)]
        public byte[] Alpha { get; private set; } = new byte[2];

        [JsonProperty, NotNull(Flags = ValidateFlags.Instance | ValidateFlags.Collection), CountEquals(3), Validate(Flags = ValidateFlags.Collection)]
        public LayerInfo[] Layers { get; private set; } = new LayerInfo[3];
        public uint UnknownFields { get; set; }

        [JsonProperty, NotNull, CountEquals(12)]
        public byte[] Unknown { get; private set; } = new byte[12];
    }
}
