using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFileSystem;
using RopeSnake.Core;
using RopeSnake.Core.Validation;

namespace RopeSnake.Mother3.Data
{
    [Validate]
    public sealed class DataModule : Mother3Module
    {
        #region Static strings

        private static readonly string PsiKey = "Data.Psi";
        private static readonly string ItemsKey = "Data.Items";
        private static readonly string EnemiesKey = "Data.Enemies";

        private static readonly FileSystemPath PsiPath = "/data/psi.json".ToPath();
        private static readonly FileSystemPath ItemsPath = "/data/items.json".ToPath();
        private static readonly FileSystemPath EnemiesPath = "/data/enemies.json".ToPath();

        #endregion

        public override string Name => "Data";

        [Validate(Flags = ValidateFlags.Collection), NotNull(Flags = ValidateFlags.Instance | ValidateFlags.Collection)]
        public List<Psi> Psi { get; set; }

        [Validate(Flags = ValidateFlags.Collection), NotNull(Flags = ValidateFlags.Instance | ValidateFlags.Collection)]
        public List<Item> Items { get; set; }

        [Validate(Flags = ValidateFlags.Collection), NotNull(Flags = ValidateFlags.Instance | ValidateFlags.Collection)]
        public List<Enemy> Enemies { get; set; }

        public DataModule(Mother3RomConfig romConfig, Mother3ProjectSettings projectSettings)
            : base(romConfig, projectSettings)
        {

        }

        public override void ReadFromFiles(IFileSystem fileSystem)
        {
            var jsonManager = new JsonFileManager(fileSystem);
            RegisterFileManagerProgress(jsonManager);

            Psi = jsonManager.ReadJson<List<Psi>>(PsiPath);
            Items = jsonManager.ReadJson<List<Item>>(ItemsPath);
            Enemies = jsonManager.ReadJson<List<Enemy>>(EnemiesPath);
        }

        public override void WriteToFiles(IFileSystem fileSystem, ISet<object> staleObjects)
        {
            var jsonManager = new JsonFileManager(fileSystem);
            RegisterFileManagerProgress(jsonManager);

            jsonManager.WriteJson(PsiPath, Psi);
            jsonManager.WriteJson(ItemsPath, Items);
            jsonManager.WriteJson(EnemiesPath, Enemies);
        }

        public override void ReadFromRom(Block romData)
        {
            Psi = ReadTable(romData, PsiKey, DataExtensions.ReadPsi);
            Items = ReadTable(romData, ItemsKey, DataExtensions.ReadItem);
            Enemies = ReadTable(romData, EnemiesKey, DataExtensions.ReadEnemy);
        }

        public override void WriteToRom(Block romData, AllocatedBlockCollection allocatedBlocks)
        {
            WriteAllocatedBlocks(romData, allocatedBlocks);
            UpdateRomReferences(romData, allocatedBlocks, PsiKey, ItemsKey, EnemiesKey);
        }

        public override ModuleSerializationResult Serialize()
        {
            var blocks = new LazyBlockCollection();

            blocks.Add(PsiKey, () => SerializeTable(Psi, Data.Psi.FieldSize, DataExtensions.WritePsi));
            blocks.Add(ItemsKey, () => SerializeTable(Items, Item.FieldSize, DataExtensions.WriteItem));
            blocks.Add(EnemiesKey, () => SerializeTable(Enemies, Enemy.FieldSize, DataExtensions.WriteEnemy));

            return new ModuleSerializationResult(blocks, null);
        }

        public override void UpdateNameHints(Text.TextModule textModule)
        {
            base.UpdateNameHints(textModule);

            UpdateNameHints(Psi, textModule.PsiNames);
            UpdateNameHints(Items, textModule.ItemNames);
            UpdateNameHints(Enemies, textModule.EnemyNames);
        }
    }
}
