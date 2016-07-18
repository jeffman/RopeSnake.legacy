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
    public class Tileset<T> : ITileset<T>, IBinarySerializable where T : Tile, new()
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

        protected void ResetTiles(int newCount)
        {
            _tiles = new T[newCount];
        }

        public virtual IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_tiles).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _tiles.GetEnumerator();

        #region IBinarySerializable implementation

        public virtual void Serialize(Stream stream)
        {
            var writer = new BinaryStream(stream);
            writer.WriteInt(Count);

            for (int i = 0; i < Count; i++)
            {
                _tiles[i].Serialize(stream);
            }
        }

        public virtual void Deserialize(Stream stream, int fileSize)
        {
            var reader = new BinaryStream(stream);
            int count = reader.ReadInt();

            ResetTiles(count);

            for (int i = 0; i < Count; i++)
            {
                _tiles[i] = new T();
                _tiles[i].Deserialize(stream, fileSize);
            }
        }

        #endregion
    }
}
