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
        private int _width;
        private int _height;

        public virtual int Width => _width;
        public virtual int Height => _height;

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
            _width = width;
            _height = height;
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    yield return _tileInfo[x, y];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
