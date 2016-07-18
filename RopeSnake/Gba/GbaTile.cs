using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using RopeSnake.Graphics;

namespace RopeSnake.Gba
{
    public sealed class GbaTile : Tile
    {
        public override int Width => 8;
        public override int Height => 8;

        public GbaTile() : base(8, 8) { }

        #region Serialization

        internal void Read(BinaryStream stream, int bitDepth)
        {
            switch (bitDepth)
            {
                case 4:
                    Read4Bpp(stream);
                    break;
                case 8:
                    Read8Bpp(stream);
                    break;
                default:
                    throw new NotSupportedException(nameof(bitDepth));
            }
        }

        private void Read4Bpp(BinaryStream stream)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x += 2)
                {
                    byte tmp = stream.ReadByte();
                    this[x, y] = (byte)(tmp & 0xF);
                    this[x + 1, y] = (byte)((tmp >> 4) & 0xF);
                }
            }
        }

        private void Read8Bpp(BinaryStream stream)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    this[x, y] = stream.ReadByte();
                }
            }
        }

        internal void Write(BinaryStream stream, int bitDepth)
        {
            switch (bitDepth)
            {
                case 4:
                    Write4Bpp(stream);
                    break;
                case 8:
                    Write8Bpp(stream);
                    break;
                default:
                    throw new NotSupportedException(nameof(bitDepth));
            }
        }

        private void Write4Bpp(BinaryStream stream)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x += 2)
                {
                    byte tmp = (byte)((this[x, y] & 0xF) | ((this[x + 1, y] & 0xF) << 4));
                    stream.WriteByte(tmp);
                }
            }
        }

        private void Write8Bpp(BinaryStream stream)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    stream.WriteByte(this[x, y]);
                }
            }
        }

        #endregion
    }
}
