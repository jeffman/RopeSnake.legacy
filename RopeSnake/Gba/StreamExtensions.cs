using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Gba
{
    public static class StreamExtensions
    {
        public static int ReadGbaPointer(this BinaryStream stream)
        {
            return stream.ReadInt() & 0x1FFFFFF;
        }

        public static void WriteGbaPointer(this BinaryStream stream, int pointer)
        {
            stream.WriteInt(pointer | 0x8000000);
        }
    }
}
