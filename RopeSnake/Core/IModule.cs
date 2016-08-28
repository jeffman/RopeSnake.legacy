using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFileSystem;

namespace RopeSnake.Core
{
    public interface IModule
    {
        string Name { get; }
        IProgress<ProgressPercent> Progress { set; }

        void ReadFromRom(Block romData);
        void WriteToRom(Block romData, AllocatedBlockCollection allocatedBlocks);
        void ReadFromFiles(IFileSystem fileSystem);
        void WriteToFiles(IFileSystem fileSystem, ISet<object> staleObjects);
        ModuleSerializationResult Serialize();
        IEnumerable<string> GetBlockKeysForFile(FileSystemPath path);
    }

    public sealed class ModuleSerializationResult
    {
        public LazyBlockCollection Blocks { get; }
        public IEnumerable<IList<string>> ContiguousKeys { get; }

        public ModuleSerializationResult(LazyBlockCollection blocks, IEnumerable<IList<string>> contiguousKeys)
        {
            Blocks = blocks;
            ContiguousKeys = contiguousKeys;
        }
    }
}
