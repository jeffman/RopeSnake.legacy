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
            bool wasCreated = false;
            Block[] createdBlocks = null;
            var lockObj = new object();

            for (int i = 0; i < keys.Length; i++)
            {
                int iCopy = i;

                Func<Block> blockCreator = () =>
                {
                    if (!wasCreated)
                    {
                        lock (lockObj)
                        {
                            if (!wasCreated)
                            {
                                createdBlocks = multipleBlockCreator();
                                wasCreated = true;
                            }
                        }
                    }
                    return createdBlocks[iCopy];
                };
                Add(keys[i], blockCreator);
            }
        }
    }
}
