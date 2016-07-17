using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Graphics
{
    public class TileInfo
    {
        public virtual int TileIndex { get; set; }
        public virtual int PaletteIndex { get; set; }
        public virtual bool FlipX { get; set; }
        public virtual bool FlipY { get; set; }
    }
}
