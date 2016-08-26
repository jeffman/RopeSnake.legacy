using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFileSystem;
using Newtonsoft.Json;
using RopeSnake.Core;

namespace RopeSnake.Mother3.Maps
{
    public sealed class MapModule : Mother3Module
    {
        public override string Name => "Maps";

        public MapModule(Mother3RomConfig romConfig, Mother3ProjectSettings projectSettings)
            : base(romConfig, projectSettings)
        {

        }

        public override void ReadFromFiles(IFileSystem fileSystem)
        {
            throw new NotImplementedException();
        }

        public override void ReadFromRom(Block romData)
        {
            throw new NotImplementedException();
        }

        public override ModuleSerializationResult Serialize()
        {
            throw new NotImplementedException();
        }

        public override void WriteToFiles(IFileSystem fileSystem, ISet<object> staleObjects)
        {
            throw new NotImplementedException();
        }

        public override void WriteToRom(Block romData, AllocatedBlockCollection allocatedBlocks)
        {
            throw new NotImplementedException();
        }
    }
}
