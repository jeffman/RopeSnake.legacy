using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3.IO
{
    public sealed class ShortOffsetTableWriter
    {
        private BinaryStream _dataStream;
        private int _baseDataPosition;
        private int[] _offsets;
        private int _currentIndex;

        public ShortOffsetTableWriter(BinaryStream dataStream, int count)
        {
            _dataStream = dataStream;
            _baseDataPosition = dataStream.Position;
            _offsets = new int[count];

            // It is conventional for the data stream to begin with [0xFFFF, count]
            dataStream.WriteUShort(0xFFFF);
            dataStream.WriteUShort((ushort)count);
        }

        public static Block CreateOffsetTable(int count)
            => new Block(count * 2);

        public void AddCurrentOffset()
            => AddPointer(_dataStream.Position);

        public void AddPointer(int pointer)
            => AddOffset(pointer - _baseDataPosition);

        public void AddOffset(int offset)
        {
            if (_currentIndex >= _offsets.Length)
                throw new InvalidOperationException("Exceeded the end of the table");

            _offsets[_currentIndex++] = offset;
        }

        public void UpdateOffsetTable(Block offsetTable, bool divideByTwo)
        {
            if (_currentIndex != _offsets.Length)
                throw new InvalidOperationException("Not all offsets have been added to the table");

            var offsetStream = offsetTable.ToBinaryStream();

            for (int i = 0; i < _offsets.Length; i++)
            {
                int offset = _offsets[i];

                if (divideByTwo)
                {
                    if ((offset & 1) != 0)
                        throw new Exception("Offsets must be even-valued when dividing by two");

                    offset /= 2;
                }

                if (offset > 0xFFFF)
                {
                    throw new Exception($"Offset out of range: 0x{offset:X}");
                }

                offsetStream.WriteUShort((ushort)offset);
            }
        }
    }
}
