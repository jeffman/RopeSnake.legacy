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
        #region Static strings

        private static readonly string ItemsKey = "Data.Items";

        private static readonly FileSystemPath ItemsPath = "/data/items.json".ToPath();

        #endregion

        public override string Name => "Data";

        public List<Item> Items { get; set; }

        public DataModule(Mother3RomConfig romConfig, Mother3ProjectSettings projectSettings)
            : base(romConfig, projectSettings)
        {

        }

        public override void ReadFromFiles(IFileSystem fileSystem)
        {
            var jsonManager = new JsonFileManager(fileSystem);

            Items = jsonManager.ReadJson<List<Item>>(ItemsPath);
        }

        public override void WriteToFiles(IFileSystem fileSystem, ISet<object> staleObjects)
        {
            var jsonManager = new JsonFileManager(fileSystem);

            jsonManager.WriteJson(ItemsPath, Items);
        }

        public override void ReadFromRom(Block romData)
        {
            Items = ReadTable(romData, ItemsKey, s => s.ReadItem());
        }

        public override void WriteToRom(Block romData, AllocatedBlockCollection allocatedBlocks)
        {
            WriteAllocatedBlocks(romData, allocatedBlocks);
            UpdateRomReferences(romData, allocatedBlocks, ItemsKey);
        }

        public override ModuleSerializationResult Serialize()
        {
            var blocks = new LazyBlockCollection();

            blocks.Add(ItemsKey, () => SerializeTable(Items, Item.FieldSize, DataStreamExtensions.WriteItem));

            return new ModuleSerializationResult(blocks, null);
        }
    }
}
