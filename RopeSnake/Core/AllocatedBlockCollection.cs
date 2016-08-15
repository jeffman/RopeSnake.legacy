using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public class AllocatedBlockCollection : BlockCollection
    {
        private Dictionary<string, int> _allocatedPointers;

        public AllocatedBlockCollection(BlockCollection blocks, Dictionary<string, int> allocatedPointers)
        {
            if (blocks == null)
                throw new ArgumentNullException(nameof(blocks));

            if (allocatedPointers == null)
                throw new ArgumentNullException(nameof(allocatedPointers));

            _allocatedPointers = new Dictionary<string, int>();
            foreach (string key in blocks.Keys)
            {
                base.AddBlock(key, blocks[key]);
                _allocatedPointers.Add(key, allocatedPointers[key]);
            }
        }

        public int GetAllocatedPointer(string key) => _allocatedPointers[key];

        public override bool AddBlock(string key, Block block)
        {
            throw new InvalidOperationException("Cannot modify an AllocatedBlockCollection");
        }

        public override bool RemoveBlock(string key)
        {
            throw new InvalidOperationException("Cannot modify an AllocatedBlockCollection");
        }

        public override void AddBlockCollection(BlockCollection collection)
        {
            throw new InvalidOperationException("Cannot modify an AllocatedBlockCollection");
        }
    }
}
