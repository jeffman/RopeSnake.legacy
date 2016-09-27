using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using RopeSnake.Core.Validation;

namespace RopeSnake.Mother3.Maps
{
    [Validate]
    public class BigtileSet : IBinarySerializable
    {
        [NotNull(Flags = ValidateFlags.Instance | ValidateFlags.Collection), CountEquals(0x300)]
        public Bigtile[] Tiles { get; private set; } = new Bigtile[0x300];

        public BigtileSet()
        {
            for (int i = 0; i < 0x300; i++)
                Tiles[i] = new Bigtile();
        }

        public void Serialize(Stream stream)
        {
            var writer = new BinaryStream(stream);
            foreach (var tile in Tiles)
            {
                writer.WriteBigtile(tile);
            }
        }

        public void Deserialize(Stream stream, int fileSize)
        {
            int expectedSize = 0x300 * 8;

            if (fileSize != expectedSize)
                throw new Exception($"File size was {fileSize} bytes, but expected {expectedSize} byte");

            Tiles = new Bigtile[0x300];
            var reader = new BinaryStream(stream);
            for (int i = 0; i < 0x300; i++)
            {
                Tiles[i] = reader.ReadBigtile();
            }
        }
    }
}
