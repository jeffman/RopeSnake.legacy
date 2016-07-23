using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public class Block : IBinarySerializable
    {
        private byte[] _data;

        public virtual byte this[int index]
        {
            get { return _data[index]; }
            set { _data[index] = value; }
        }

        internal byte[] Data => _data;

        public virtual int Size => Data.Length;

        public Block() { }

        public Block(int blockSize)
        {
            if (blockSize < 0)
                throw new ArgumentException(nameof(blockSize));

            _data = new byte[blockSize];
        }

        public Block(byte[] copyFrom)
        {
            if (copyFrom == null)
                throw new ArgumentNullException(nameof(copyFrom));

            _data = new byte[copyFrom.Length];
            Array.Copy(copyFrom, _data, copyFrom.Length);
        }

        public Block(Block copyFrom) : this(copyFrom._data) { }

        public BinaryStream ToBinaryStream()
        {
            return new BinaryStream(new BlockStream(this));
        }

        internal virtual void CopyTo(byte[] destination, int destinationOffset, int sourceOffset, int length)
        {
            Array.Copy(_data, sourceOffset, destination, destinationOffset, length);
        }

        internal void CopyFrom(byte[] source, int sourceOffset, int destinationOffset, int length)
        {
            Array.Copy(source, sourceOffset, _data, destinationOffset, length);
        }

        #region IBinarySerializable implementation

        public void Serialize(Stream stream)
        {
            stream.Write(_data, 0, _data.Length);
        }

        public void Deserialize(Stream stream, int fileSize)
        {
            _data = new byte[fileSize];
            stream.Read(Data, 0, fileSize);
        }

        #endregion
    }
}
