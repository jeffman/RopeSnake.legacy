using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFileSystem;
using RopeSnake.Core;
using RopeSnake.Core.Validation;
using RopeSnake.Mother3.IO;

namespace RopeSnake.Mother3.Text
{
    [Validate]
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
        private static readonly string EnemyNamesShortKey = "Text.EnemyNamesShort";
        private static readonly string ItemDescriptionsSpecialKey = "Text.ItemDescriptionsSpecial";
        private static readonly string OutsideKey = "Text.Outside";
        private static readonly string MenusKey = "Text.Menus";
        private static readonly string MemosKey = "Text.Memos";
        private static readonly string EnemyDescriptionsKey = "Text.EnemyDescriptions";
        private static readonly string MusicTitlesKey = "Text.MusicTitles";

        private static readonly string MenusBankAKey = "Menus.BankA";
        private static readonly string MenusBankBKey = "Menus.BankB";

        private static readonly FileSystemPath RoomDescriptionsPath = "/text/room-descriptions.json".ToPath();
        private static readonly FileSystemPath ItemNamesPath = "/text/item-names.json".ToPath();
        private static readonly FileSystemPath ItemDescriptionsPath = "/text/item-descriptions.json".ToPath();
        private static readonly FileSystemPath CharNamesPath = "/text/char-names.json".ToPath();
        private static readonly FileSystemPath PartyCharNamesPath = "/text/party-char-names.json".ToPath();
        private static readonly FileSystemPath EnemyNamesPath = "/text/enemy-names.json".ToPath();
        private static readonly FileSystemPath PsiNamesPath = "/text/psi-names.json".ToPath();
        private static readonly FileSystemPath PsiDescriptionsPath = "/text/psi-descriptions.json".ToPath();
        private static readonly FileSystemPath StatusesPath = "/text/statuses.json".ToPath();
        private static readonly FileSystemPath DefaultCharNamesPath = "/text/default-char-names.json".ToPath();
        private static readonly FileSystemPath SkillsPath = "/text/skills.json".ToPath();
        private static readonly FileSystemPath SkillDescriptionsPath = "/text/skill-descriptions.json".ToPath();
        private static readonly FileSystemPath MainScriptPath = "/text/main-script.json".ToPath();
        private static readonly FileSystemPath EnemyNamesShortPath = "/text/enemy-names-short.json".ToPath();
        private static readonly FileSystemPath ItemDescriptionsSpecialPath = "/text/item-descriptions-special.json".ToPath();
        private static readonly FileSystemPath OutsidePath = "/text/outside.json".ToPath();
        private static readonly FileSystemPath MenusPath = "/text/menus.json".ToPath();
        private static readonly FileSystemPath MemosPath = "/text/memos.json".ToPath();
        private static readonly FileSystemPath EnemyDescriptionsPath = "/text/enemy-descriptions.json".ToPath();
        private static readonly FileSystemPath MusicTitlesPath = "/text/music-titles.json".ToPath();

        #endregion

        public override string Name => "Text";

        [NotNull] public List<string> RoomDescriptions { get; set; }
        [NotNull] public StringTable ItemNames { get; set; }
        [NotNull] public List<string> ItemDescriptions { get; set; }
        [NotNull] public StringTable CharNames { get; set; }
        [NotNull] public StringTable PartyCharNames { get; set; }
        [NotNull] public StringTable EnemyNames { get; set; }
        [NotNull] public StringTable PsiNames { get; set; }
        [NotNull] public List<string> PsiDescriptions { get; set; }
        [NotNull] public StringTable Statuses { get; set; }
        [NotNull] public StringTable DefaultCharNames { get; set; }
        [NotNull] public StringTable Skills { get; set; }
        [NotNull] public List<string> SkillDescriptions { get; set; }
        [NotNull] public List<List<string>> MainScript { get; set; }
        [NotNull] public List<string> Outside { get; set; }
        [NotNull] public List<string> Menus { get; set; }
        [NotNull] public List<string> Memos { get; set; }
        [NotNull] public List<string> EnemyDescriptions { get; set; }
        [NotNull] public Bxt MusicTitles { get; set; }

        public StringTable EnemyNamesShort { get; set; }
        public List<string> ItemDescriptionsSpecial { get; set; }

        private string[] _textKeys;
        private string[] _mainScriptKeys;

        public TextModule(Mother3RomConfig romConfig, Mother3ProjectSettings projectSettings)
            : base(romConfig, projectSettings)
        {

        }

        public override void ReadFromFiles(IFileSystem fileSystem)
        {
            var jsonManager = new JsonFileManager(fileSystem);
            RegisterFileManagerProgress(jsonManager);

            RoomDescriptions = jsonManager.ReadJson<List<string>>(RoomDescriptionsPath);
            ItemNames = jsonManager.ReadJson<StringTable>(ItemNamesPath);
            ItemDescriptions = jsonManager.ReadJson<List<string>>(ItemDescriptionsPath);
            CharNames = jsonManager.ReadJson<StringTable>(CharNamesPath);
            PartyCharNames = jsonManager.ReadJson<StringTable>(PartyCharNamesPath);
            EnemyNames = jsonManager.ReadJson<StringTable>(EnemyNamesPath);
            PsiNames = jsonManager.ReadJson<StringTable>(PsiNamesPath);
            PsiDescriptions = jsonManager.ReadJson<List<string>>(PsiDescriptionsPath);
            Statuses = jsonManager.ReadJson<StringTable>(StatusesPath);
            DefaultCharNames = jsonManager.ReadJson<StringTable>(DefaultCharNamesPath);
            Skills = jsonManager.ReadJson<StringTable>(SkillsPath);
            SkillDescriptions = jsonManager.ReadJson<List<string>>(SkillDescriptionsPath);
            MainScript = jsonManager.ReadJson<List<List<string>>>(MainScriptPath);
            Outside = jsonManager.ReadJson<List<string>>(OutsidePath);
            Menus = jsonManager.ReadJson<List<string>>(MenusPath);
            Memos = jsonManager.ReadJson<List<string>>(MemosPath);
            EnemyDescriptions = jsonManager.ReadJson<List<string>>(EnemyDescriptionsPath);
            MusicTitles = jsonManager.ReadJson<Bxt>(MusicTitlesPath);

            AddBlockKeysForFile(RoomDescriptionsPath, TextBankKey, GetOffsetAndDataKeys(RoomDescriptionsKey));
            AddBlockKeysForFile(ItemNamesPath, TextBankKey, ItemNamesKey);
            AddBlockKeysForFile(ItemDescriptionsPath, TextBankKey, GetOffsetAndDataKeys(ItemDescriptionsKey));
            AddBlockKeysForFile(CharNamesPath, TextBankKey, CharNamesKey);
            AddBlockKeysForFile(PartyCharNamesPath, TextBankKey, PartyCharNamesKey);
            AddBlockKeysForFile(EnemyNamesPath, TextBankKey, EnemyNamesKey);
            AddBlockKeysForFile(PsiNamesPath, TextBankKey, PsiNamesKey);
            AddBlockKeysForFile(PsiDescriptionsPath, TextBankKey, GetOffsetAndDataKeys(PsiDescriptionsKey));
            AddBlockKeysForFile(StatusesPath, TextBankKey, StatusesKey);
            AddBlockKeysForFile(DefaultCharNamesPath, TextBankKey, DefaultCharNamesKey);
            AddBlockKeysForFile(SkillsPath, TextBankKey, SkillsKey);
            AddBlockKeysForFile(SkillDescriptionsPath, TextBankKey, GetOffsetAndDataKeys(SkillDescriptionsKey));
            AddBlockKeysForFile(MainScriptPath, MainScriptKey, MainScript.SelectMany((s, i) => GetOffsetAndDataKeys($"{MainScriptKey}.{i}")));
            AddBlockKeysForFile(OutsidePath, GetOffsetAndDataKeys(OutsideKey));
            AddBlockKeysForFile(MenusPath, GetOffsetAndDataKeys(MenusKey));
            AddBlockKeysForFile(MemosPath, GetOffsetAndDataKeys(MemosKey));
            AddBlockKeysForFile(EnemyDescriptionsPath, GetOffsetAndDataKeys(EnemyDescriptionsKey));
            AddBlockKeysForFile(MusicTitlesPath, MusicTitlesKey);

            if (RomConfig.IsEnglish)
            {
                EnemyNamesShort = jsonManager.ReadJson<StringTable>(EnemyNamesShortPath);
                ItemDescriptionsSpecial = jsonManager.ReadJson<List<string>>(ItemDescriptionsSpecialPath);

                AddBlockKeysForFile(EnemyNamesShortPath, EnemyNamesShortKey);
                AddBlockKeysForFile(ItemDescriptionsSpecialPath, GetOffsetAndDataKeys(ItemDescriptionsSpecialKey));
            }
        }

        public override void WriteToFiles(IFileSystem fileSystem, ISet<object> staleObjects)
        {
            var jsonManager = new JsonFileManager(fileSystem);
            RegisterFileManagerProgress(jsonManager);
            jsonManager.StaleObjects = staleObjects;

            jsonManager.WriteJson(RoomDescriptionsPath, RoomDescriptions);
            jsonManager.WriteJson(ItemNamesPath, ItemNames);
            jsonManager.WriteJson(ItemDescriptionsPath, ItemDescriptions);
            jsonManager.WriteJson(CharNamesPath, CharNames);
            jsonManager.WriteJson(PartyCharNamesPath, PartyCharNames);
            jsonManager.WriteJson(EnemyNamesPath, EnemyNames);
            jsonManager.WriteJson(PsiNamesPath, PsiNames);
            jsonManager.WriteJson(PsiDescriptionsPath, PsiDescriptions);
            jsonManager.WriteJson(StatusesPath, Statuses);
            jsonManager.WriteJson(DefaultCharNamesPath, DefaultCharNames);
            jsonManager.WriteJson(SkillsPath, Skills);
            jsonManager.WriteJson(SkillDescriptionsPath, SkillDescriptions);
            jsonManager.WriteJson(MainScriptPath, MainScript);
            jsonManager.WriteJson(OutsidePath, Outside);
            jsonManager.WriteJson(MenusPath, Menus);
            jsonManager.WriteJson(MemosPath, Memos);
            jsonManager.WriteJson(EnemyDescriptionsPath, EnemyDescriptions);
            jsonManager.WriteJson(MusicTitlesPath, MusicTitles);

            if (RomConfig.IsEnglish)
            {
                jsonManager.WriteJson(EnemyNamesShortPath, EnemyNamesShort);
                jsonManager.WriteJson(ItemDescriptionsSpecialPath, ItemDescriptionsSpecial);
            }
        }

        public override void ReadFromRom(Block romData)
        {
            var codec = StringCodec.Create(RomConfig);

            ReadTextBank(romData, codec);
            ReadMainScript(romData, codec);
            ReadMiscText(romData, codec);
        }

        private void ReadTextBank(Block romData, StringCodec codec)
        {
            var stream = romData.ToBinaryStream(RomConfig.GetOffset(TextBankKey, romData));
            var offsetTableReader = new WideOffsetTableReader(stream);

            RoomDescriptions = offsetTableReader.ReadStringOffsetTable(codec, false);
            ItemNames = offsetTableReader.ReadStringTable(codec);
            ItemDescriptions = offsetTableReader.ReadStringOffsetTable(codec, false);
            CharNames = offsetTableReader.ReadStringTable(codec);
            PartyCharNames = offsetTableReader.ReadStringTable(codec);
            EnemyNames = offsetTableReader.ReadStringTable(codec);
            PsiNames = offsetTableReader.ReadStringTable(codec);
            PsiDescriptions = offsetTableReader.ReadStringOffsetTable(codec, false);
            Statuses = offsetTableReader.ReadStringTable(codec);
            DefaultCharNames = offsetTableReader.ReadStringTable(codec);
            Skills = offsetTableReader.ReadStringTable(codec);
            SkillDescriptions = offsetTableReader.ReadStringOffsetTable(codec, false);

            if (RomConfig.IsEnglish)
            {
                stream.Position = RomConfig.GetOffset(EnemyNamesShortKey, romData);
                EnemyNamesShort = stream.ReadStringTable(codec);

                var itemDescKeys = GetOffsetAndDataKeys(ItemDescriptionsSpecialKey);
                var itemDescPositions = itemDescKeys.Select(k => RomConfig.GetOffset(k, romData)).ToArray();
                stream.Position = itemDescPositions[0];
                ItemDescriptionsSpecial = stream.ReadStringOffsetTable(codec, false, itemDescPositions[1]);
            }
        }

        private LazyBlockCollection SerializeTextBank(StringCodec codec, List<List<string>> contiguousBlocks)
        {
            var blockCollection = new LazyBlockCollection();

            blockCollection.Add(TextBankKey, () => WideOffsetTableWriter.CreateOffsetTable(16));
            blockCollection.AddStringOffsetTableBlocks(RoomDescriptionsKey, codec, RoomDescriptions, false);
            blockCollection.Add(ItemNamesKey, () => TextExtensions.SerializeStringTable(codec, ItemNames));
            blockCollection.AddStringOffsetTableBlocks(ItemDescriptionsKey, codec, ItemDescriptions, false);
            blockCollection.Add(CharNamesKey, () => TextExtensions.SerializeStringTable(codec, CharNames));
            blockCollection.Add(PartyCharNamesKey, () => TextExtensions.SerializeStringTable(codec, PartyCharNames));
            blockCollection.Add(EnemyNamesKey, () => TextExtensions.SerializeStringTable(codec, EnemyNames));
            blockCollection.Add(PsiNamesKey, () => TextExtensions.SerializeStringTable(codec, PsiNames));
            blockCollection.AddStringOffsetTableBlocks(PsiDescriptionsKey, codec, PsiDescriptions, false);
            blockCollection.Add(StatusesKey, () => TextExtensions.SerializeStringTable(codec, Statuses));
            blockCollection.Add(DefaultCharNamesKey, () => TextExtensions.SerializeStringTable(codec, DefaultCharNames));
            blockCollection.Add(SkillsKey, () => TextExtensions.SerializeStringTable(codec, Skills));
            blockCollection.AddStringOffsetTableBlocks(SkillDescriptionsKey, codec, SkillDescriptions, false);

            _textKeys = blockCollection.Keys.Skip(1).ToArray();

            if (RomConfig.IsEnglish)
            {
                blockCollection.Add(EnemyNamesShortKey, () => TextExtensions.SerializeStringTable(codec, EnemyNamesShort));
                blockCollection.AddStringOffsetTableBlocks(ItemDescriptionsSpecialKey, codec, ItemDescriptionsSpecial, false);
                contiguousBlocks.Add(new List<string>(GetOffsetAndDataKeys(ItemDescriptionsSpecialKey)));
            }

            if (ProjectSettings.OffsetTableMode == OffsetTableMode.Contiguous)
            {
                contiguousBlocks.Add(new List<string>(_textKeys));
            }
            else
            {
                // String offset tables should always be contiguous regardless
                contiguousBlocks.Add(new List<string> { _textKeys[0], _textKeys[1] }); // Room descriptions
                contiguousBlocks.Add(new List<string> { _textKeys[3], _textKeys[4] }); // Item descriptions
                contiguousBlocks.Add(new List<string> { _textKeys[9], _textKeys[10] }); // PSI descriptions
                contiguousBlocks.Add(new List<string> { _textKeys[14], _textKeys[15] }); // Skill descriptions
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
                MainScript.Add(offsetTableReader.ReadStringOffsetTable(codec, true));
            }
        }

        private LazyBlockCollection SerializeMainScript(StringCodec codec, List<List<string>> contiguousBlocks)
        {
            var blockCollection = new LazyBlockCollection();

            blockCollection.Add(MainScriptKey, () => WideOffsetTableWriter.CreateOffsetTable(MainScript.Count * 2));
            for (int i = 0; i < MainScript.Count; i++)
            {
                blockCollection.AddStringOffsetTableBlocks($"{MainScriptKey}.{i}", codec, MainScript[i], true);
            }

            _mainScriptKeys = blockCollection.Keys.Skip(1).ToArray();

            if (ProjectSettings.OffsetTableMode == OffsetTableMode.Contiguous)
            {
                contiguousBlocks.Add(new List<string>(_mainScriptKeys));
            }
            else
            {
                // Offset table and data should always be contiguous regardless
                for (int i = 0; i < MainScript.Count; i++)
                {
                    var keys = GetOffsetAndDataKeys($"{MainScriptKey}.{i}");
                    contiguousBlocks.Add(new List<string>(keys));
                }
            }

            return blockCollection;
        }

        private void EncodeMainScript(Block romData, AllocatedBlockCollection allocatedBlocks)
        {
            if (RomConfig.ScriptEncodingParameters == null)
                return;

            for (int i = 0; i < MainScript.Count; i++)
            {
                string dataKey = GetOffsetAndDataKeys($"{MainScriptKey}.{i}")[1];
                var block = allocatedBlocks[dataKey];
                int pointer = allocatedBlocks.GetAllocatedPointer(dataKey);

                if (block == null || pointer == 0)
                    continue;

                EnglishStringCodec.EncodeBlock(romData, pointer + 4, block.Size - 4, RomConfig.ScriptEncodingParameters);
            }
        }

        private void ReadMiscText(Block romData, StringCodec codec)
        {
            var stream = romData.ToBinaryStream(RomConfig.GetOffset(MenusBankAKey, romData));
            var offsetTableReader = new WideOffsetTableReader(stream);
            offsetTableReader.Skip(0x24);
            Outside = offsetTableReader.ReadStringOffsetTable(codec, false);

            stream.Position = RomConfig.GetOffset(MenusBankBKey, romData);
            offsetTableReader = new WideOffsetTableReader(stream);
            offsetTableReader.Skip(0x58);

            Menus = offsetTableReader.ReadStringOffsetTable(codec, false);
            Memos = offsetTableReader.ReadStringOffsetTable(codec, false);
            EnemyDescriptions = offsetTableReader.ReadStringOffsetTable(codec, false);

            stream.Position = RomConfig.GetOffset(MusicTitlesKey, romData);
            MusicTitles = stream.ReadBxt(codec, RomConfig.IsEnglish);
        }

        private LazyBlockCollection SerializeMiscText(StringCodec codec, List<List<string>> contiguousBlocks)
        {
            var blockCollection = new LazyBlockCollection();

            blockCollection.AddStringOffsetTableBlocks(OutsideKey, codec, Outside, false);
            blockCollection.AddStringOffsetTableBlocks(MenusKey, codec, Menus, false);
            blockCollection.AddStringOffsetTableBlocks(MemosKey, codec, Memos, false);
            blockCollection.AddStringOffsetTableBlocks(EnemyDescriptionsKey, codec, EnemyDescriptions, false);
            blockCollection.Add(MusicTitlesKey, () => TextExtensions.SerializeBxt(MusicTitles, codec, RomConfig.IsEnglish));

            // No way to ensure contiguity with random access blocks within the menu banks,
            // but the string tables should themselves be contiguous always
            contiguousBlocks.Add(new List<string>(GetOffsetAndDataKeys(OutsideKey)));
            contiguousBlocks.Add(new List<string>(GetOffsetAndDataKeys(MenusKey)));
            contiguousBlocks.Add(new List<string>(GetOffsetAndDataKeys(MemosKey)));
            contiguousBlocks.Add(new List<string>(GetOffsetAndDataKeys(EnemyDescriptionsKey)));

            return blockCollection;
        }

        private void UpdateMenuTables(Block romData, AllocatedBlockCollection allocatedBlocks)
        {
            int menusALocation = RomConfig.GetOffset(MenusBankAKey, romData);
            WideOffsetTableWriter.UpdateTableOffsets(romData,
                GetOffsetAndDataKeys(OutsideKey)
                    .Select((k, i) => new IndexLocation(i + 0x24, allocatedBlocks.GetAllocatedPointer(k))),
                menusALocation, menusALocation);

            int menusBLocation = RomConfig.GetOffset(MenusBankBKey, romData);
            WideOffsetTableWriter.UpdateTableOffsets(romData,
                GetOffsetAndDataKeys(MenusKey)
                    .Concat(GetOffsetAndDataKeys(MemosKey))
                    .Concat(GetOffsetAndDataKeys(EnemyDescriptionsKey))
                    .Select((k, i) => new IndexLocation(i + 0x58, allocatedBlocks.GetAllocatedPointer(k))),
                menusBLocation, menusBLocation);
        }

        public override void WriteToRom(Block romData, AllocatedBlockCollection allocatedBlocks)
        {
            UpdateWideOffsetTable(allocatedBlocks, TextBankKey, _textKeys);
            UpdateWideOffsetTable(allocatedBlocks, MainScriptKey, _mainScriptKeys);
            UpdateMenuTables(romData, allocatedBlocks);

            WriteAllocatedBlocks(romData, allocatedBlocks);
            UpdateRomReferences(romData, allocatedBlocks, TextBankKey, MainScriptKey);

            EncodeMainScript(romData, allocatedBlocks);

            if (RomConfig.IsEnglish)
            {
                UpdateRomReferences(romData, allocatedBlocks, EnemyNamesShortKey, GetOffsetAndDataKeys(ItemDescriptionsSpecialKey));
            }

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
            blocks.AddRange(SerializeMiscText(codec, contiguousKeys));

            return new ModuleSerializationResult(blocks, contiguousKeys);
        }
    }
}
