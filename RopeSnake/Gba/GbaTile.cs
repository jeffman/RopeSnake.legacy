using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Graphics;

namespace RopeSnake.Gba
{
    public sealed class GbaTile : Tile
    {
        public override int Width => 8;
        public override int Height => 8;

        public GbaTile() : base(8, 8) { }
    }
}
