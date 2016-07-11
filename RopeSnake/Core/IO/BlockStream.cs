using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public sealed class BlockStream : Stream
    {
        private Block _block;

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => _block.Size;

        public override long Position { get; set; }

        public BlockStream(Block block)
        {
            _block = block;
        }

        public override void Flush() { }

        public override int ReadByte() => _block[(int)(Position++)];

        public override int Read(byte[] buffer, int offset, int count)
        {
            _block.CopyTo(buffer, offset, (int)Position, count);
            Position += count;
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;

                case SeekOrigin.Current:
                    Position += offset;
                    break;

                case SeekOrigin.End:
                    throw new NotSupportedException();
            }

            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void WriteByte(byte value) => _block[(int)(Position++)] = value;

        public override void Write(byte[] buffer, int offset, int count)
            => _block.CopyFrom(buffer, offset, (int)Position, count);
    }
}
