using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3.IO
{
    public static class WideOffsetTableWriter
    {
        public static Block CreateOffsetTable(int count)
            => new Block((count + 1) * 4 + 4);

        public static Block CreateDummyTable(int fieldSize, int count)
        {
            int tableSize = fieldSize * count;
            var block = new Block(tableSize + 12); // dummy table has 1 offset, plus header, plus footer offset

            // Since the dummy table will always be contiguous, and the header is always 12 bytes,
            // then the header is going to be static: [01 00 00 00 0C 00 00 00 <tableSize + 12>]
            UpdateTableCount(block, 1);
            UpdateTableOffsets(block, new[] { new IndexLocation(0, 12) }, 0);

            return block;
        }

        //public static void UpdateOffsetTable(AllocatedBlockCollection allocatedBlocks,
        //    string offsetTableKey, IEnumerable<string> dataKeys)
        //{
        //    var table = allocatedBlocks[offsetTableKey];
        //    int basePointer = allocatedBlocks.GetAllocatedPointer(offsetTableKey);
        //    var stream = table.ToBinaryStream();

        //    dataKeys = dataKeys.Where(k => k != offsetTableKey);

        //    int count = dataKeys.Count();
        //    stream.WriteInt(count);

        //    int endPointer = 0;
        //    foreach (string key in dataKeys)
        //    {
        //        int pointer = allocatedBlocks.GetAllocatedPointer(key);
        //        if (pointer == 0)
        //        {
        //            stream.WriteInt(0);
        //        }
        //        else
        //        {
        //            stream.WriteInt(pointer - basePointer);
        //            endPointer = pointer + allocatedBlocks[key].Size;
        //        }
        //    }

        //    if (endPointer == 0)
        //    {
        //        stream.WriteInt(0);
        //    }
        //    else
        //    {
        //        stream.WriteInt(endPointer - basePointer);
        //    }
        //}

        public static void UpdateTableCount(Block offsetTable, int newCount, int physicalOffsetTableLocation = 0)
        {
            var tableStream = offsetTable.ToBinaryStream(physicalOffsetTableLocation);
            tableStream.WriteInt(newCount);
        }

        public static void UpdateTableOffsets(Block offsetTable, IEnumerable<IndexLocation> newLocations,
            int offsetTableBase, int physicalOffsetTableLocation = 0)
        {
            var tableStream = offsetTable.ToBinaryStream();

            foreach (var newLocation in newLocations)
            {
                tableStream.Position = (physicalOffsetTableLocation + 4) + (newLocation.Index * 4);

                if (newLocation.IsNull)
                {
                    tableStream.WriteInt(0);
                }
                else
                {
                    tableStream.WriteInt(newLocation.Location - offsetTableBase);
                }
            }
        }
    }
}
