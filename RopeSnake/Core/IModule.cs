using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public interface IModule
    {
        string Name { get; }

        void ReadFromRom(Block romData);
        void WriteToRom(Block romData, AllocatedBlockCollection allocatedBlocks);
        void ReadFromFiles(IFileManager manager);
        void WriteToFiles(IFileManager manager);

        BlockCollection Serialize();
        void UpdateReferences(AllocatedBlockCollection allocatedBlocks);
    }
}
