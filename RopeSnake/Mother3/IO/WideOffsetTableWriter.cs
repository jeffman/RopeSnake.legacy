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

        public static BlockCollection ToContiguous(BlockCollection blockCollection, string offsetTableKey,
            IEnumerable<string> dataKeys, int alignment = 1)
        {
            var tempCollection = new BlockCollection();
            tempCollection.AddBlockCollection(blockCollection);

            var allocatedPointers = new Dictionary<string, int>();
            var table = tempCollection[offsetTableKey];

            allocatedPointers.Add(offsetTableKey, 0);

            int currentPointer = table.Size;
            foreach (string key in dataKeys)
            {
                currentPointer = currentPointer.Align(alignment);
                var dataBlock = tempCollection[key];
                if (dataBlock == null)
                {
                    allocatedPointers.Add(key, 0);
                }
                else
                {
                    allocatedPointers.Add(key, currentPointer);
                    currentPointer += dataBlock.Size;
                }
            }

            var contiguousBlock = new Block(currentPointer);
            var contiguousStream = contiguousBlock.ToBinaryStream(table.Size);

            foreach (string key in dataKeys)
            {
                var dataBlock = tempCollection[key];
                if (dataBlock != null)
                {
                    contiguousStream.Position = allocatedPointers[key];
                    contiguousStream.WriteBytes(dataBlock.Data, 0, dataBlock.Size);
                }
            }

            tempCollection.AddBlock(offsetTableKey, contiguousBlock);
            UpdateOffsetTable(new AllocatedBlockCollection(tempCollection, allocatedPointers), offsetTableKey, dataKeys, currentPointer);
            tempCollection = new BlockCollection();
            tempCollection.AddBlock(offsetTableKey, contiguousBlock);
            return tempCollection;
        }
    }
}
