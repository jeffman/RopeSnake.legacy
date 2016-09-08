using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3.IO
{
    public sealed class SarOffsetTableReader : OffsetTableReader
    {
        public int CurrentSize => _sizes[CurrentIndex];
        private int[] _sizes;

        public SarOffsetTableReader(BinaryStream stream)
            : base(stream)
        {
            int basePosition = stream.Position;

            string header = stream.ReadString(4);
            if (header != "sar ")
                throw new Exception($"Expected sar header but got {header} at 0x{basePosition:X}");

            int count = stream.ReadInt();
            Pointers = new int[count];
            _sizes = new int[count];

            for (int i = 0; i < count; i++)
            {
                int offset = stream.ReadInt();
                _sizes[i] = stream.ReadInt();

                if (offset == 0)
                {
                    Pointers[i] = 0;
                }
                else
                {
                    Pointers[i] = basePosition + offset;
                }
            }
        }
    }
}
