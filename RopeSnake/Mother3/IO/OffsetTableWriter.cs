using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Mother3.IO
{
    public abstract class OffsetTableWriter
    {
        public abstract void AddNull();
        public abstract void AddPointer(int pointer);
    }
}
