using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Graphics
{
    public abstract class Tile
    {
        private byte[,] _pixels;

        protected virtual byte[,] Pixels
        {
            get { return _pixels; }
            set { _pixels = value; }
        }

        public virtual byte this[int x, int y]
        {
            get { return _pixels[x, y]; }
            set { _pixels[x, y] = value; }
        }

        public abstract int Width { get; }
        public abstract int Height { get; }

        protected Tile(int width, int height)
        {
            _pixels = new byte[width, height];
        }
    }
}
