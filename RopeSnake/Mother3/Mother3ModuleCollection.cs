using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using RopeSnake.Mother3.Data;
using RopeSnake.Mother3.Text;

namespace RopeSnake.Mother3
{
    public sealed class Mother3ModuleCollection : IEnumerable<Mother3Module>
    {
        public DataModule Data { get; }
        public TextModule Text { get; }

        public Mother3ModuleCollection(Mother3RomConfig romConfig, Mother3ProjectSettings projectSettings)
        {
            Data = new DataModule(romConfig, projectSettings);
            Text = new TextModule(romConfig, projectSettings);
        }

        public IEnumerator<Mother3Module> GetEnumerator()
        {
            yield return Data;
            yield return Text;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
