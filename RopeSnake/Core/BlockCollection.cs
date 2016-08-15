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
        private List<string> _orderedKeys;

        public Block this[string key] => _blocks[key];

        public BlockCollection()
        {
            _blocks = new Dictionary<string, Block>();
            _orderedKeys = new List<string>();
        }

        public virtual void AddBlock(string key, Block block)
        {
            _blocks.Add(key, block);
            _orderedKeys.Add(key);
        }

        public virtual bool RemoveBlock(string key)
        {
            bool result = _blocks.Remove(key);
            _orderedKeys.Remove(key);
            return result;
        }

        public virtual void AddBlockCollection(BlockCollection collection)
        {
            foreach (var key in collection.Keys)
            {
                AddBlock(key, collection[key]);
            }
        }

        public virtual IEnumerable<string> Keys => _orderedKeys;

        public virtual IEnumerable<Block> Blocks => _orderedKeys.Select(k => _blocks[k]);

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
