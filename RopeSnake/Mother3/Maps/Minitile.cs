using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core.Validation;

namespace RopeSnake.Mother3.Maps
{
    [Validate]
    public sealed class Minitile
    {
        [InRange(0, 0x3F)]
        public byte TileIndex { get; set; }

        public bool FlipX { get; set; }
        public bool FlipY { get; set; }
        public bool Visible { get; set; }
    }
}
