using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Mother3.IO
{
    public struct IndexLocationSize
    {
        public readonly int Index;
        public readonly int Location;
        public readonly int Size;

        public bool IsNull => (Location == 0);

        public IndexLocationSize(int index, int location, int size)
        {
            Index = index;
            Location = location;
            Size = size;
        }
    }
}
