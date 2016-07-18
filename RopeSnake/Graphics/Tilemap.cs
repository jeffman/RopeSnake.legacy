using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Graphics
{
    public class Tilemap<T> : ITilemap<T> where T : TileInfo
    {
        private T[,] _tileInfo;

        public virtual int Width => _tileInfo.GetLength(0);
        public virtual int Height => _tileInfo.GetLength(1);

        public virtual T this[int x, int y]
        {
            get { return _tileInfo[x, y]; }
            set { _tileInfo[x, y] = value; }
        }

        public Tilemap(int width, int height)
        {
            if (width < 0)
                throw new ArgumentException(nameof(width));

            if (height < 0)
                throw new ArgumentException(nameof(height));

            _tileInfo = new T[width, height];
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    yield return _tileInfo[x, y];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
