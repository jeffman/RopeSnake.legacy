using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Graphics
{
    public class TileInfo : IBinarySerializable
    {
        public virtual int TileIndex { get; set; }
        public virtual int PaletteIndex { get; set; }
        public virtual bool FlipX { get; set; }
        public virtual bool FlipY { get; set; }

        #region IBinarySerializable implementation

        public virtual void Serialize(Stream stream)
        {
            var writer = new BinaryStream(stream);

            writer.WriteInt(TileIndex);
            writer.WriteInt(PaletteIndex);
            writer.WriteBool(FlipX);
            writer.WriteBool(FlipY);
        }

        public virtual void Deserialize(Stream stream, int fileSize)
        {
            var reader = new BinaryStream(stream);

            TileIndex = reader.ReadInt();
            PaletteIndex = reader.ReadInt();
            FlipX = reader.ReadBool();
            FlipY = reader.ReadBool();
        }

        #endregion
    }
}
