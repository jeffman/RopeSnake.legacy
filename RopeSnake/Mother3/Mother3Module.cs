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
        protected Mother3RomConfig RomConfig { get; }

        protected Mother3Module(Mother3RomConfig romConfig)
        {
            RomConfig = romConfig;
        }

        protected void UpdateRomReferences(Block romData, string key, int value)
        {
            var stream = romData.ToBinaryStream();
            var references = RomConfig.GetReferences(key);

            foreach (int reference in references)
            {
                stream.Position = reference;
                stream.WriteGbaPointer(value);
            }
        }

        protected void WriteAllocatedBlocksAndUpdateReferences(Block romData,
            AllocatedBlockCollection allocatedBlocks, params string[] keys)
        {
            foreach (string key in keys)
            {
                int pointer = allocatedBlocks.GetAllocatedPointer(key);
                var block = allocatedBlocks[key];
                block.CopyTo(romData.Data, pointer, 0, block.Size);
                UpdateRomReferences(romData, key, pointer);
            }
        }

        #region Helpers

        protected List<T> ReadTable<T>(Block romData, string key, Func<BinaryStream, T> elementReader)
        {
            int offset = RomConfig.GetOffset(key, romData);
            int count = RomConfig.GetParameter<int>(key + ".Count");
            var stream = romData.ToBinaryStream();
            stream.Position = offset;

            var list = new List<T>();
            for (int i = 0; i < count; i++)
                list.Add(elementReader(stream));

            return list;
        }

        protected Block SerializeTable<T>(List<T> list, int fieldSize, Action<BinaryStream, T> elementWriter)
        {
            var block = new Block(list.Count * fieldSize);
            var stream = block.ToBinaryStream();

            foreach (T element in list)
                elementWriter(stream, element);

            return block;
        }

        #endregion

        #region IModule implementation

        public abstract string Name { get; }
        public abstract void ReadFromRom(Block romData);
        public abstract void WriteToRom(Block romData, AllocatedBlockCollection allocatedBlocks);
        public abstract void ReadFromFiles(IFileSystem fileSystem);
        public abstract void WriteToFiles(IFileSystem fileSystem);
        public abstract BlockCollection Serialize();

        #endregion
    }
}
