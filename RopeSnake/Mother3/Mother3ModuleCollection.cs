using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using RopeSnake.Mother3.Data;

namespace RopeSnake.Mother3
{
    public sealed class Mother3ModuleCollection : IEnumerable<Mother3Module>
    {
        public DataModule Data { get; }

        public Mother3ModuleCollection(Mother3RomConfig romConfig)
        {
            Data = new DataModule(romConfig);
        }

        public IEnumerator<Mother3Module> GetEnumerator()
        {
            yield return Data;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
