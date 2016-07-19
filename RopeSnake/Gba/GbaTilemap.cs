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
    public sealed class GbaTilemap : Tilemap<TileInfo>
    {
        public GbaTilemap(int width, int height) : base(width, height, 8, 8) { }

        #region IBinarySerializable implementation

        public override void Serialize(Stream stream)
        {
            var writer = new BinaryStream(stream);

            writer.WriteInt(Width);
            writer.WriteInt(Height);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    writer.WriteTileInfo(this[x, y]);
                }
            }
        }

        public override void Deserialize(Stream stream, int fileSize)
        {
            var reader = new BinaryStream(stream);

            int width = reader.ReadInt();
            int height = reader.ReadInt();

            ResetTileInfo(width, height);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    this[x, y] = reader.ReadTileInfo();
                }
            }
        }

        #endregion
    }
}
