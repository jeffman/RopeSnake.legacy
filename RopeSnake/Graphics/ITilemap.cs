using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Graphics
{
    public interface ITilemap<out T> : IEnumerable<T> where T : TileInfo
    {
        int Width { get; }
        int Height { get; }
        int TileWidth { get; }
        int TileHeight { get; }
        T this[int x, int y] { get; }
    }
}
