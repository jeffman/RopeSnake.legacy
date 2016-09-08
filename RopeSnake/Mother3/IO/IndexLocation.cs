using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Mother3.IO
{
    public struct IndexLocation
    {
        public readonly int Index;
        public readonly int Location;

        public bool IsNull => (Location == 0);

        public IndexLocation(int index, int location)
        {
            Index = index;
            Location = location;
        }
    }
}
