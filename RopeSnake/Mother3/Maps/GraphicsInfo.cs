using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core.Validation;
using Newtonsoft.Json;

namespace RopeSnake.Mother3.Maps
{
    [Validate]
    public sealed class GraphicsInfo : INameHint
    {
        public static readonly int FieldSize = 26;

        [JsonProperty(Order = -1, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string NameHint { get; set; }

        [JsonProperty, NotNull, CountEquals(12)]
        public short[] TileSets { get; private set; } = new short[12];
        public short Palette { get; set; }
    }
}
