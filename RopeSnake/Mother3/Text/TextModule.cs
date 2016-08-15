using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using RopeSnake.Mother3.IO;

namespace RopeSnake.Mother3.Text
{
    public sealed class TextModule : Mother3Module
    {
        #region Keys

        private static readonly string TextBankKey = "Text.Bank";
        private static readonly string RoomDescriptionsKey = "Text.RoomDescriptions";
        private static readonly string ItemNamesKey = "Text.ItemNames";
        private static readonly string ItemDescriptionsKey = "Text.ItemDescriptions";
        private static readonly string CharNamesKey = "Text.CharNames";
        private static readonly string PartyCharNamesKey = "Text.PartyCharNames";
        private static readonly string EnemyNamesKey = "Text.EnemyNames";
        private static readonly string PsiNamesKey = "Text.PsiNames";
        private static readonly string PsiDescriptionsKey = "Text.PsiDescriptions";
        private static readonly string StatusesKey = "Text.Statuses";
        private static readonly string DefaultCharNamesKey = "Text.DefaultCharNames";
        private static readonly string SkillsKey = "Text.Skills";
        private static readonly string SkillDescriptionsKey = "Text.SkillDescriptions";
        private static readonly string MainScriptKey = "Text.MainScript";

        #endregion

        public override string Name => "Text";

        public List<string> RoomDescriptions { get; set; }
        public StringTable ItemNames { get; set; }
        public List<string> ItemDescriptions { get; set; }
        public StringTable CharNames { get; set; }
        public StringTable PartyCharNames { get; set; }
        public StringTable EnemyNames { get; set; }
        public StringTable PsiNames { get; set; }
        public List<string> PsiDescriptions { get; set; }
        public StringTable Statuses { get; set; }
        public StringTable DefaultCharNames { get; set; }
        public StringTable Skills { get; set; }
        public List<string> SkillDescriptions { get; set; }
        public List<List<string>> MainScript { get; set; }

        private string[] _textKeys;
        private string[] _mainScriptKeys;

        public TextModule(Mother3RomConfig romConfig, Mother3ProjectSettings projectSettings)
            : base(romConfig, projectSettings)
        {

        }

        public override void ReadFromFiles(IFileSystem fileSystem)
        {
            var jsonManager = new JsonFileManager(fileSystem);

            MainScript = jsonManager.ReadJson<List<List<string>>>(@"text\main-script.json");
        }

        public override void WriteToFiles(IFileSystem fileSystem)
        {
            var jsonManager = new JsonFileManager(fileSystem);

            jsonManager.WriteJson(@"text\room-descriptions.json", RoomDescriptions);
            jsonManager.WriteJson(@"text\item-names.json", ItemNames);
            jsonManager.WriteJson(@"text\item-descriptions.json", ItemDescriptions);
            jsonManager.WriteJson(@"text\char-names.json", CharNames);
            jsonManager.WriteJson(@"text\party-char-names.json", PartyCharNames);
            jsonManager.WriteJson(@"text\enemy-names.json", EnemyNames);
            jsonManager.WriteJson(@"text\psi-names.json", PsiNames);
            jsonManager.WriteJson(@"text\psi-descriptions.json", PsiDescriptions);
            jsonManager.WriteJson(@"text\statuses.json", Statuses);
            jsonManager.WriteJson(@"text\default-char-names.json", DefaultCharNames);
            jsonManager.WriteJson(@"text\skills.json", Skills);
            jsonManager.WriteJson(@"text\skill-descriptions.json", SkillDescriptions);
            jsonManager.WriteJson(@"text\main-script.json", MainScript);
        }

        public override void ReadFromRom(Block romData)
        {
            var codec = StringCodec.Create(RomConfig);

            ReadTextBank(romData, codec);
            ReadMainScript(romData, codec);
        }

        private void ReadTextBank(Block romData, StringCodec codec)
        {
            var stream = romData.ToBinaryStream(RomConfig.GetOffset(TextBankKey, romData));
            var offsetTableReader = new WideOffsetTableReader(stream);

            RoomDescriptions = offsetTableReader.ReadStringOffsetTable(codec, false, false);
            ItemNames = offsetTableReader.ReadStringTable(codec);
            ItemDescriptions = offsetTableReader.ReadStringOffsetTable(codec, false, false);
            CharNames = offsetTableReader.ReadStringTable(codec);
            PartyCharNames = offsetTableReader.ReadStringTable(codec);
            EnemyNames = offsetTableReader.ReadStringTable(codec);
            PsiNames = offsetTableReader.ReadStringTable(codec);
            PsiDescriptions = offsetTableReader.ReadStringOffsetTable(codec, false, false);
            Statuses = offsetTableReader.ReadStringTable(codec);
            DefaultCharNames = offsetTableReader.ReadStringTable(codec);
            Skills = offsetTableReader.ReadStringTable(codec);
            SkillDescriptions = offsetTableReader.ReadStringOffsetTable(codec, false, false);
        }

        private BlockCollection SerializeTextBank(StringCodec codec)
        {
            var blockCollection = new BlockCollection();

            blockCollection.AddStringOffsetTableBlocks(RoomDescriptionsKey, codec, RoomDescriptions, false, false);
            blockCollection.AddBlock(ItemNamesKey, TextStreamExtensions.SerializeStringTable(codec, ItemNames));
            blockCollection.AddStringOffsetTableBlocks(ItemDescriptionsKey, codec, ItemDescriptions, false, false);
            blockCollection.AddBlock(CharNamesKey, TextStreamExtensions.SerializeStringTable(codec, CharNames));
            blockCollection.AddBlock(PartyCharNamesKey, TextStreamExtensions.SerializeStringTable(codec, PartyCharNames));
            blockCollection.AddBlock(EnemyNamesKey, TextStreamExtensions.SerializeStringTable(codec, EnemyNames));
            blockCollection.AddBlock(PsiNamesKey, TextStreamExtensions.SerializeStringTable(codec, PsiNames));
            blockCollection.AddStringOffsetTableBlocks(PsiDescriptionsKey, codec, PsiDescriptions, false, false);
            blockCollection.AddBlock(StatusesKey, TextStreamExtensions.SerializeStringTable(codec, Statuses));
            blockCollection.AddBlock(DefaultCharNamesKey, TextStreamExtensions.SerializeStringTable(codec, DefaultCharNames));
            blockCollection.AddBlock(SkillsKey, TextStreamExtensions.SerializeStringTable(codec, Skills));
            blockCollection.AddStringOffsetTableBlocks(SkillDescriptionsKey, codec, SkillDescriptions, false, false);

            _textKeys = blockCollection.Keys.ToArray();
            blockCollection.AddBlock(TextBankKey, WideOffsetTableWriter.CreateOffsetTable(16));

            if (ProjectSettings.OffsetTableMode == OffsetTableMode.Contiguous)
            {
                var contiguousBlock = WideOffsetTableWriter.ToContiguous(blockCollection, TextBankKey, _textKeys, 4);
                blockCollection = new BlockCollection();
                blockCollection.AddBlock(TextBankKey, contiguousBlock);
                _textKeys = null;
            }

            return blockCollection;
        }

        private void ReadMainScript(Block romData, StringCodec codec)
        {
            var stream = romData.ToBinaryStream(RomConfig.GetOffset(MainScriptKey, romData));
            MainScript = new List<List<string>>();

            var offsetTableReader = new WideOffsetTableReader(stream);
            while (!offsetTableReader.EndOfTable)
            {
                MainScript.Add(offsetTableReader.ReadStringOffsetTable(codec, true, false));
            }
        }

        private BlockCollection SerializeMainScript(StringCodec codec)
        {
            var blockCollection = new BlockCollection();

            for (int i = 0; i < MainScript.Count; i++)
            {
                blockCollection.AddStringOffsetTableBlocks($"{MainScriptKey}.{i}", codec, MainScript[i], true, false);
            }

            _mainScriptKeys = blockCollection.Keys.ToArray();
            blockCollection.AddBlock(MainScriptKey, WideOffsetTableWriter.CreateOffsetTable(MainScript.Count * 2));

            if (ProjectSettings.OffsetTableMode == OffsetTableMode.Contiguous)
            {
                var contiguousBlock = WideOffsetTableWriter.ToContiguous(blockCollection, MainScriptKey, _mainScriptKeys, 4);
                blockCollection = new BlockCollection();
                blockCollection.AddBlock(MainScriptKey, contiguousBlock);
                _mainScriptKeys = null;
            }

            return blockCollection;
        }

        public override void WriteToRom(Block romData, AllocatedBlockCollection allocatedBlocks)
        {
            if (ProjectSettings.OffsetTableMode == OffsetTableMode.Fragmented)
            {
                WideOffsetTableWriter.UpdateOffsetTable(allocatedBlocks, TextBankKey, _textKeys, 0);
                WideOffsetTableWriter.UpdateOffsetTable(allocatedBlocks, MainScriptKey, _mainScriptKeys, 0);
            }

            WriteAllocatedBlocks(romData, allocatedBlocks);
            UpdateRomReferences(romData, allocatedBlocks, TextBankKey, MainScriptKey);

            _textKeys = null;
            _mainScriptKeys = null;
        }

        public override BlockCollection Serialize()
        {
            var codec = StringCodec.Create(RomConfig);
            var blockCollection = new BlockCollection();

            blockCollection.AddBlockCollection(SerializeTextBank(codec));
            blockCollection.AddBlockCollection(SerializeMainScript(codec));

            return blockCollection;
        }
    }
}
