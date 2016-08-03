using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3.IO
{
    public sealed class WideOffsetTableReader : OffsetTableReader
    {
        public override int Count => Pointers.Length - 1;
        public int EndPointer => Pointers[Count];

        public WideOffsetTableReader(BinaryStream stream)
            : base(stream)
        {
            int basePosition = stream.Position;

            int count = stream.ReadInt();
            Pointers = new int[count + 1];

            for (int i = 0; i <= count; i++)
            {
                int offset = stream.ReadInt();
                if (offset != 0)
                    Pointers[i] = offset + basePosition;
            }
        }
    }
}
