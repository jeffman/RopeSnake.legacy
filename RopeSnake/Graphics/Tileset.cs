using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Graphics
{
    public class Tileset<T> : ITileset<T> where T : Tile
    {
        private T[] _tiles;

        public int Count => _tiles.Length;

        public virtual T this[int index]
        {
            get { return _tiles[index]; }
            set { _tiles[index] = value; }
        }

        public Tileset(int count)
        {
            if (count < 0)
                throw new ArgumentException(nameof(count));

            _tiles = new T[count];
        }

        public virtual IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_tiles).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _tiles.GetEnumerator();
    }
}
