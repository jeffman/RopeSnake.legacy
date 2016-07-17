using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Graphics
{
    public interface ITileset<out T> : IEnumerable<T> where T : Tile
    {
        int Count { get; }
        T this[int index] { get; }
    }
}
