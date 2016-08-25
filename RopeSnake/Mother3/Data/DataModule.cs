using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFileSystem;
using RopeSnake.Core;

namespace RopeSnake.Mother3.Data
{
    public sealed class DataModule : Mother3Module
    {
        public override string Name => "Data";

        public List<Item> Items { get; set; }

        public DataModule(Mother3RomConfig romConfig, Mother3ProjectSettings projectSettings)
            : base(romConfig, projectSettings)
        {

        }

        public override void ReadFromFiles(IFileSystem fileSystem)
        {
            var jsonManager = new JsonFileManager(fileSystem);

            Items = jsonManager.ReadJson<List<Item>>(@"data\items.json");
        }

        public override void WriteToFiles(IFileSystem fileSystem, ISet<object> staleObjects)
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
            WriteAllocatedBlocks(romData, allocatedBlocks);
            UpdateRomReferences(romData, allocatedBlocks, "Data.Items");
        }

        public override ModuleSerializationResult Serialize()
        {
            var blocks = new LazyBlockCollection();

            blocks.Add("Data.Items", () => SerializeTable(Items, Item.FieldSize, DataStreamExtensions.WriteItem));

            return new ModuleSerializationResult(blocks, null);
        }
    }
}
