using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public class BlockCollection
    {
        private Dictionary<string, Block> _blocks;

        public Block this[string key] => _blocks[key];

        public BlockCollection()
        {
            _blocks = new Dictionary<string, Block>();
        }

        public virtual void AddBlock(string key, Block block) => _blocks.Add(key, block);

        public virtual bool RemoveBlock(string key) => _blocks.Remove(key);

        public virtual IEnumerable<string> Keys => _blocks.Keys;

        public virtual IEnumerable<Block> Blocks => _blocks.Values;
    }
}
