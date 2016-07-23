using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using RopeSnake.Gba;

namespace RopeSnake.Mother3
{
    public abstract class Mother3Module : IModule
    {
        protected Mother3RomConfig RomConfiguration { get; }

        protected Mother3Module(Mother3RomConfig romConfig)
        {
            RomConfiguration = romConfig;
        }

        protected void UpdateRomReferences(Block romData, string key, int value)
        {
            var stream = romData.ToBinaryStream();
            var references = RomConfiguration.GetReferences(key);

            foreach (int reference in references)
            {
                stream.Position = reference;
                stream.WriteGbaPointer(value);
            }
        }

        #region IModule implementation

        public abstract string Name { get; }
        public abstract void ReadFromRom(Block romData);
        public abstract void WriteToRom(Block romData, AllocatedBlockCollection allocatedBlocks);
        public abstract void ReadFromFiles(IFileSystem fileSystem);
        public abstract void WriteToFiles(IFileSystem fileSystem);
        public abstract BlockCollection Serialize();
        public abstract void UpdateReferences(AllocatedBlockCollection allocatedBlocks);

        #endregion
    }
}
