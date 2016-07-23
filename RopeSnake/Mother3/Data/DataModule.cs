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
            ReadItems(romData);
        }

        public override void WriteToRom(Block romData, AllocatedBlockCollection allocatedBlocks)
        {
            WriteAllocatedBlocksAndUpdateReferences(romData, allocatedBlocks, "Data.Items");
        }

        private void ReadItems(Block romData)
        {
            int offset = RomConfig.GetOffset("Data.Items", romData);
            int count = RomConfig.GetParameter<int>("Data.Items.Count");
            var stream = romData.ToBinaryStream();
            stream.Position = offset;

            Items = new List<Item>();
            for (int i = 0; i < count; i++)
                Items.Add(stream.ReadItem());
        }

        private Block SerializeItems()
        {
            var block = new Block(Items.Count * Item.FieldSize);
            var stream = block.ToBinaryStream();

            foreach (Item item in Items)
                stream.WriteItem(item);

            return block;
        }

        public override BlockCollection Serialize()
        {
            var blocks = new BlockCollection();

            blocks.AddBlock("Data.Items", SerializeItems());

            return blocks;
        }
    }
}
