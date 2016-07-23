using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3.Data
{
    public sealed class DataModule : Mother3Module
    {
        public override string Name => "Data";

        public List<Item> Items { get; set; }

        public DataModule(Mother3RomConfig romConfig)
            : base(romConfig)
        {

        }

        public override void ReadFromFiles(IFileSystem fileSystem)
        {
            var jsonManager = new JsonFileManager(fileSystem);

            Items = jsonManager.ReadJson<List<Item>>(@"data\items.json");
        }

        public override void WriteToFiles(IFileSystem fileSystem)
        {
            var jsonManager = new JsonFileManager(fileSystem);

            jsonManager.WriteJson(@"data\items.json", Items);
        }

        public override void ReadFromRom(Block romData)
        {
            Items = ReadTable(romData, "Data.Items", s => s.ReadItem());
        }

        public override void WriteToRom(Block romData, AllocatedBlockCollection allocatedBlocks)
        {
            WriteAllocatedBlocksAndUpdateReferences(romData, allocatedBlocks, "Data.Items");
        }

        public override BlockCollection Serialize()
        {
            var blocks = new BlockCollection();

            blocks.AddBlock("Data.Items", SerializeTable(Items, Item.FieldSize, DataStreamExtensions.WriteItem));

            return blocks;
        }
    }
}
