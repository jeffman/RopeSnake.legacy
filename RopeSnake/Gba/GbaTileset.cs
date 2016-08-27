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
    public sealed class GbaTileset : Tileset<GbaTile>
    {
        public int BitDepth { get; private set; }

        public GbaTileset() : this(0, 0)
        {

        }

        public GbaTileset(int count, int bitDepth)
            : base(count)
        {
            BitDepth = bitDepth;
        }

        #region IBinarySerializable implementation

        public override void Serialize(Stream stream)
        {
            var writer = new BinaryStream(stream);

            // Write a "header tile" containing the bit depth
            writer.WriteInt(BitDepth);
            var zeroes = Enumerable.Repeat<byte>(0, BitDepth * 8 - 4).ToArray();
            writer.WriteBytes(zeroes, 0, zeroes.Length);

            for (int i = 0; i < Count; i++)
            {
                this[i].Write(writer, BitDepth);
            }
        }

        public override void Deserialize(Stream stream, int fileSize)
        {
            var reader = new BinaryStream(stream);

            // Read the "header tile"
            BitDepth = reader.ReadInt();
            reader.Position += (BitDepth * 8 - 4);

            int count = fileSize / (BitDepth * 8) - 1;
            ResetTiles(count);

            for (int i = 0; i < count; i++)
            {
                this[i] = new GbaTile();
                this[i].Read(reader, BitDepth);
            }
        }

        #endregion
    }
}
