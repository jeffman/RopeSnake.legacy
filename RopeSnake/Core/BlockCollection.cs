using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public class BlockCollection : OrderedDictionary<string, Block>
    {
        public BlockCollection() : base() { }

        public BlockCollection(IEnumerable<KeyValuePair<string, Block>> copyFrom)
            : base(copyFrom)
        { }
    }
}
