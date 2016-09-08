using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3.IO
{
    public static class SarOffsetTableWriter
    {
        public static Block CreateOffsetTable(int count)
            => new Block((count * 8) + 8);

        public static void UpdateTableCount(Block offsetTable, int newCount, int physicalOffsetTableLocation = 0)
        {
            var tableStream = offsetTable.ToBinaryStream(physicalOffsetTableLocation + 4);
            tableStream.WriteInt(newCount);
        }

        public static void UpdateTableOffsets(Block offsetTable, IEnumerable<IndexLocationSize> newLocations,
            int offsetTableBase, int physicalOffsetTableLocation = 0)
        {
            var tableStream = offsetTable.ToBinaryStream();

            foreach (var newLocation in newLocations)
            {
                tableStream.Position = (physicalOffsetTableLocation + 8) + (newLocation.Index * 8);

                if (newLocation.IsNull)
                {
                    tableStream.WriteInt(0);
                }
                else
                {
                    tableStream.WriteInt(newLocation.Location - offsetTableBase);
                }

                tableStream.WriteInt(newLocation.Size);
            }
        }
    }
}
