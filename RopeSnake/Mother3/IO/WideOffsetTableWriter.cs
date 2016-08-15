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

        public static void UpdateOffsetTable(AllocatedBlockCollection allocatedBlocks,
            string offsetTableKey, IEnumerable<string> dataKeys, int endPointer)
        {
            var table = allocatedBlocks[offsetTableKey];
            int basePointer = allocatedBlocks.GetAllocatedPointer(offsetTableKey);
            var stream = table.ToBinaryStream();

            int count = dataKeys.Count();
            stream.WriteInt(count);

            foreach (string key in dataKeys)
            {
                int pointer = allocatedBlocks.GetAllocatedPointer(key);
                if (pointer == 0)
                {
                    stream.WriteInt(0);
                }
                else
                {
                    stream.WriteInt(pointer - basePointer);
                }
            }

            if (endPointer == 0)
            {
                stream.WriteInt(0);
            }
            else
            {
                stream.WriteInt(endPointer - basePointer);
            }
        }
    }
}
