using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RopeSnake.Core.Validation;

namespace RopeSnake.Mother3.Maps
{
    [Validate]
    public sealed class LayerInfo
    {
        public byte Width { get; set; }
        public byte Height { get; set; }
        public byte UnknownFields { get; set; }

        [JsonProperty, NotNull, CountEquals(3)]
        public byte[] Unknown { get; private set; } = new byte[3];
    }
}
