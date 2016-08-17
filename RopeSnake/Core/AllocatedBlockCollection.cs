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

        public override bool IsReadOnly => true;

        public AllocatedBlockCollection(BlockCollection blocks, Dictionary<string, int> allocatedPointers)
        {
            if (blocks == null)
                throw new ArgumentNullException(nameof(blocks));

            if (allocatedPointers == null)
                throw new ArgumentNullException(nameof(allocatedPointers));

            // Verify that blocks and allocatedPointers contain the same keys
            if (blocks.Keys.Any(k => !allocatedPointers.ContainsKey(k)) || allocatedPointers.Keys.Any(k => !blocks.ContainsKey(k)))
                throw new Exception($"{nameof(blocks)} and {nameof(allocatedPointers)} must have matching keys");

            _allocatedPointers = new Dictionary<string, int>();
            foreach (string key in blocks.Keys)
            {
                base.ForceAdd(key, blocks[key]);
                _allocatedPointers.Add(key, allocatedPointers[key]);
            }
        }

        public virtual int GetAllocatedPointer(string key) => _allocatedPointers[key];
    }
}
