﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3.IO
{
    public sealed class ShortOffsetTableReader : OffsetTableReader
    {
        public ShortOffsetTableReader(BinaryStream stream, int dataPointer)
            : base(stream)
        {
            ushort offset;
            var pointers = new List<int>();
            while ((offset = stream.ReadUShort()) != 0xFFFF)
            {
                pointers.Add(dataPointer + offset);
            }
            Pointers = pointers.ToArray();
        }
    }
}
