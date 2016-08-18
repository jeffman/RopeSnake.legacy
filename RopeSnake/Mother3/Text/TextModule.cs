using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using RopeSnake.Mother3.IO;

namespace RopeSnake.Mother3.Text
{
    public sealed class TextModule : Mother3Module
    {
        #region Static strings

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

        private static readonly string RoomDescriptionsFile = Path.Combine("text", "room-descriptions.json");
        private static readonly string ItemNamesFile = Path.Combine("text", "item-names.json");
        private static readonly string ItemDescriptionsFile = Path.Combine("text", "item-descriptions.json");
        private static readonly string CharNamesFile = Path.Combine("text", "char-names.json");
        private static readonly string PartyCharNamesFile = Path.Combine("text", "party-char-names.json");
        private static readonly string EnemyNamesFile = Path.Combine("text", "enemy-names.json");
        private static readonly string PsiNamesFile = Path.Combine("text", "psi-names.json");
        private static readonly string PsiDescriptionsFile = Path.Combine("text", "psi-descriptions.json");
        private static readonly string StatusesFile = Path.Combine("text", "statuses.json");
        private static readonly string DefaultCharNamesFile = Path.Combine("text", "default-char-names.json");
        private static readonly string SkillsFile = Path.Combine("text", "skills.json");
        private static readonly string SkillDescriptionsFile = Path.Combine("text", "skill-descriptions.json");
        private static readonly string MainScriptFile = Path.Combine("text", "main-script.json");

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

            RoomDescriptions = jsonManager.ReadJson<List<string>>(RoomDescriptionsFile);
            ItemNames = jsonManager.ReadJson<StringTable>(ItemNamesFile);
            ItemDescriptions = jsonManager.ReadJson<List<string>>(ItemDescriptionsFile);
            CharNames = jsonManager.ReadJson<StringTable>(CharNamesFile);
            PartyCharNames = jsonManager.ReadJson<StringTable>(PartyCharNamesFile);
            EnemyNames = jsonManager.ReadJson<StringTable>(EnemyNamesFile);
            PsiNames = jsonManager.ReadJson<StringTable>(PsiNamesFile);
            PsiDescriptions = jsonManager.ReadJson<List<string>>(PsiDescriptionsFile);
            Statuses = jsonManager.ReadJson<StringTable>(StatusesFile);
            DefaultCharNames = jsonManager.ReadJson<StringTable>(DefaultCharNamesFile);
            Skills = jsonManager.ReadJson<StringTable>(SkillsFile);
            SkillDescriptions = jsonManager.ReadJson<List<string>>(SkillDescriptionsFile);
            MainScript = jsonManager.ReadJson<List<List<string>>>(MainScriptFile);

            AddBlockKeysForFile(RoomDescriptionsFile, TextBankKey, GetOffsetAndDataKeys(RoomDescriptionsKey));
            AddBlockKeysForFile(ItemNamesFile, TextBankKey, ItemNamesKey);
            AddBlockKeysForFile(ItemDescriptionsFile, TextBankKey, GetOffsetAndDataKeys(ItemDescriptionsKey));
            AddBlockKeysForFile(CharNamesFile, TextBankKey, CharNamesKey);
            AddBlockKeysForFile(PartyCharNamesFile, TextBankKey, PartyCharNamesKey);
            AddBlockKeysForFile(EnemyNamesFile, TextBankKey, EnemyNamesKey);
            AddBlockKeysForFile(PsiNamesFile, TextBankKey, PsiNamesKey);
            AddBlockKeysForFile(PsiDescriptionsFile, TextBankKey, GetOffsetAndDataKeys(PsiDescriptionsKey));
            AddBlockKeysForFile(StatusesFile, TextBankKey, StatusesKey);
            AddBlockKeysForFile(DefaultCharNamesFile, TextBankKey, DefaultCharNamesKey);
            AddBlockKeysForFile(SkillsFile, TextBankKey, SkillsKey);
            AddBlockKeysForFile(SkillDescriptionsFile, TextBankKey, GetOffsetAndDataKeys(SkillDescriptionsKey));
            AddBlockKeysForFile(MainScriptFile, MainScriptKey, MainScript.SelectMany((s, i) => GetOffsetAndDataKeys($"{MainScriptKey}.{i}")));
        }

        public override void WriteToFiles(IFileSystem fileSystem, ISet<object> staleObjects)
        {
            var jsonManager = new JsonFileManager(fileSystem);
            jsonManager.StaleObjects = staleObjects;

            jsonManager.WriteJson(RoomDescriptionsFile, RoomDescriptions);
            jsonManager.WriteJson(ItemNamesFile, ItemNames);
            jsonManager.WriteJson(ItemDescriptionsFile, ItemDescriptions);
            jsonManager.WriteJson(CharNamesFile, CharNames);
            jsonManager.WriteJson(PartyCharNamesFile, PartyCharNames);
            jsonManager.WriteJson(EnemyNamesFile, EnemyNames);
            jsonManager.WriteJson(PsiNamesFile, PsiNames);
            jsonManager.WriteJson(PsiDescriptionsFile, PsiDescriptions);
            jsonManager.WriteJson(StatusesFile, Statuses);
            jsonManager.WriteJson(DefaultCharNamesFile, DefaultCharNames);
            jsonManager.WriteJson(SkillsFile, Skills);
            jsonManager.WriteJson(SkillDescriptionsFile, SkillDescriptions);
            jsonManager.WriteJson(MainScriptFile, MainScript);
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

        private LazyBlockCollection SerializeTextBank(StringCodec codec, List<List<string>> contiguousBlocks)
        {
            var blockCollection = new LazyBlockCollection();

            blockCollection.Add(TextBankKey, () => WideOffsetTableWriter.CreateOffsetTable(16));
            blockCollection.AddStringOffsetTableBlocks(RoomDescriptionsKey, codec, RoomDescriptions, false, false);
            blockCollection.Add(ItemNamesKey, () => TextStreamExtensions.SerializeStringTable(codec, ItemNames));
            blockCollection.AddStringOffsetTableBlocks(ItemDescriptionsKey, codec, ItemDescriptions, false, false);
            blockCollection.Add(CharNamesKey, () => TextStreamExtensions.SerializeStringTable(codec, CharNames));
            blockCollection.Add(PartyCharNamesKey, () => TextStreamExtensions.SerializeStringTable(codec, PartyCharNames));
            blockCollection.Add(EnemyNamesKey, () => TextStreamExtensions.SerializeStringTable(codec, EnemyNames));
            blockCollection.Add(PsiNamesKey, () => TextStreamExtensions.SerializeStringTable(codec, PsiNames));
            blockCollection.AddStringOffsetTableBlocks(PsiDescriptionsKey, codec, PsiDescriptions, false, false);
            blockCollection.Add(StatusesKey, () => TextStreamExtensions.SerializeStringTable(codec, Statuses));
            blockCollection.Add(DefaultCharNamesKey, () => TextStreamExtensions.SerializeStringTable(codec, DefaultCharNames));
            blockCollection.Add(SkillsKey, () => TextStreamExtensions.SerializeStringTable(codec, Skills));
            blockCollection.AddStringOffsetTableBlocks(SkillDescriptionsKey, codec, SkillDescriptions, false, false);

            _textKeys = blockCollection.Keys.ToArray();

            if (ProjectSettings.OffsetTableMode == OffsetTableMode.Contiguous)
            {
                contiguousBlocks.Add(new List<string>(_textKeys));
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

        private LazyBlockCollection SerializeMainScript(StringCodec codec, List<List<string>> contiguousBlocks)
        {
            var blockCollection = new LazyBlockCollection();

            blockCollection.Add(MainScriptKey, () => WideOffsetTableWriter.CreateOffsetTable(MainScript.Count * 2));
            for (int i = 0; i < MainScript.Count; i++)
            {
                blockCollection.AddStringOffsetTableBlocks($"{MainScriptKey}.{i}", codec, MainScript[i], true, false);
            }

            _mainScriptKeys = blockCollection.Keys.ToArray();
    
            if (ProjectSettings.OffsetTableMode == OffsetTableMode.Contiguous)
            {
                contiguousBlocks.Add(new List<string>(_mainScriptKeys));
            }

            return blockCollection;
        }

        public override void WriteToRom(Block romData, AllocatedBlockCollection allocatedBlocks)
        {
            WideOffsetTableWriter.UpdateOffsetTable(allocatedBlocks, TextBankKey, _textKeys);
            WideOffsetTableWriter.UpdateOffsetTable(allocatedBlocks, MainScriptKey, _mainScriptKeys);

            WriteAllocatedBlocks(romData, allocatedBlocks);
            UpdateRomReferences(romData, allocatedBlocks, TextBankKey, MainScriptKey);

            _textKeys = null;
            _mainScriptKeys = null;
        }

        public override ModuleSerializationResult Serialize()
        {
            var codec = StringCodec.Create(RomConfig);
            var blocks = new LazyBlockCollection();
            var contiguousKeys = new List<List<string>>();

            blocks.AddRange(SerializeTextBank(codec, contiguousKeys));
            blocks.AddRange(SerializeMainScript(codec, contiguousKeys));

            return new ModuleSerializationResult(blocks, contiguousKeys);
        }
    }
}
