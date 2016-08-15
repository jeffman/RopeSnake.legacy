using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public class BlockCollection : IEnumerable<KeyValuePair<string, Block>>
    {
        private Dictionary<string, Block> _blocks;

        public Block this[string key] => _blocks[key];

        public BlockCollection()
        {
            _blocks = new Dictionary<string, Block>();
        }

        public virtual void AddBlock(string key, Block block) => _blocks.Add(key, block);

        public virtual bool RemoveBlock(string key) => _blocks.Remove(key);

        public virtual void AddBlockCollection(BlockCollection collection)
        {
            foreach (var blockPair in collection)
            {
                _blocks.Add(blockPair.Key, blockPair.Value);
            }
        }

        public virtual IEnumerable<string> Keys => _blocks.Keys;

        public virtual IEnumerable<Block> Blocks => _blocks.Values;

        public IEnumerator<KeyValuePair<string, Block>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, Block>>)_blocks).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, Block>>)_blocks).GetEnumerator();
        }
    }
}
