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
        public int BitDepth { get; }

        public GbaTileset(int count, int bitDepth)
            : base(count)
        {
            BitDepth = bitDepth;
        }

        #region IBinarySerializable implementation

        public override void Serialize(Stream stream)
        {
            var writer = new BinaryStream(stream);
            for (int i = 0; i < Count; i++)
            {
                this[i].Write(writer, BitDepth);
            }
        }

        public override void Deserialize(Stream stream, int fileSize)
        {
            int count = fileSize / (BitDepth * 8);
            ResetTiles(count);

            var reader = new BinaryStream(stream);

            for (int i = 0; i < count; i++)
            {
                this[i] = new GbaTile();
                this[i].Read(reader, BitDepth);
            }
        }

        #endregion
    }
}
