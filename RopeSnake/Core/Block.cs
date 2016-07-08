using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public class Block
    {
        private byte[] _data;

        public virtual byte this[int index]
        {
            get { return _data[index]; }
            set { _data[index] = value; }
        }

        protected byte[] Data => _data;

        public virtual int Size => Data.Length;

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
    }
}
