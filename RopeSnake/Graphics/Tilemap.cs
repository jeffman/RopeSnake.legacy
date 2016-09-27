using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Graphics
{
    public class Tilemap<T> : ITilemap<T>, IBinarySerializable where T : TileInfo, new()
    {
        private T[,] _tileInfo;
        private int _tileWidth;
        private int _tileHeight;

        public virtual int Width => _tileInfo.GetLength(0);
        public virtual int Height => _tileInfo.GetLength(1);
        public virtual int TileWidth => _tileWidth;
        public virtual int TileHeight => _tileHeight;

        public virtual T this[int x, int y]
        {
            get { return _tileInfo[x, y]; }
            set { _tileInfo[x, y] = value; }
        }

        public Tilemap(int width, int height, int tileWidth, int tileHeight)
        {
            ResetTileInfo(width, height);
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
        }

        protected void ResetTileInfo(int newWidth, int newHeight)
        {
            if (newWidth < 0)
                throw new ArgumentException(nameof(newWidth));

            if (newHeight < 0)
                throw new ArgumentException(nameof(newHeight));

            _tileInfo = new T[newWidth, newHeight];

            for (int y = 0; y < newHeight; y++)
                for (int x = 0; x < newWidth; x++)
                    _tileInfo[x, y] = new T();
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

        #region IBinarySerializable implementation

        public virtual void Serialize(Stream stream)
        {
            var writer = new BinaryStream(stream);

            writer.WriteInt(Width);
            writer.WriteInt(Height);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    this[x, y].Serialize(stream);
                }
            }
        }

        public virtual void Deserialize(Stream stream, int fileSize)
        {
            var reader = new BinaryStream(stream);

            int width = reader.ReadInt();
            int height = reader.ReadInt();

            ResetTileInfo(width, height);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    this[x, y].Deserialize(stream, fileSize);
                }
            }
        }

        #endregion
    }
}
