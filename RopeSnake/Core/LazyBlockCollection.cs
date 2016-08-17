using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public class LazyBlockCollection : OrderedDictionary<string, Func<Block>>
    {
        public LazyBlockCollection() : base() { }

        public LazyBlockCollection(IEnumerable<KeyValuePair<string, Func<Block>>> copyFrom)
            : base(copyFrom)
        { }

        public virtual void AddRange(string[] keys, Func<Block[]> multipleBlockCreator)
        {
            // There are multiple keys and blocks, but the blocks are all created at once
            // We only want multipleBlockCreator to run once
            int wasCreated = 0;
            Block[] createdBlocks = null;

            for (int i = 0; i < keys.Length; i++)
            {
                int iCopy = i;

                Func<Block> blockCreator = () =>
                {
                    if (Interlocked.CompareExchange(ref wasCreated, 1, 0) == 0)
                    {
                        createdBlocks = multipleBlockCreator();
                    }
                    return createdBlocks[iCopy];
                };
                Add(keys[i], blockCreator);
            }
        }
    }
}
