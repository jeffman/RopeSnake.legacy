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
            blockCollection.Add(ItemNamesKey, () => TextExtensions.SerializeStringTable(codec, ItemNames));
            blockCollection.AddStringOffsetTableBlocks(ItemDescriptionsKey, codec, ItemDescriptions, false, false);
            blockCollection.Add(CharNamesKey, () => TextExtensions.SerializeStringTable(codec, CharNames));
            blockCollection.Add(PartyCharNamesKey, () => TextExtensions.SerializeStringTable(codec, PartyCharNames));
            blockCollection.Add(EnemyNamesKey, () => TextExtensions.SerializeStringTable(codec, EnemyNames));
            blockCollection.Add(PsiNamesKey, () => TextExtensions.SerializeStringTable(codec, PsiNames));
            blockCollection.AddStringOffsetTableBlocks(PsiDescriptionsKey, codec, PsiDescriptions, false, false);
            blockCollection.Add(StatusesKey, () => TextExtensions.SerializeStringTable(codec, Statuses));
            blockCollection.Add(DefaultCharNamesKey, () => TextExtensions.SerializeStringTable(codec, DefaultCharNames));
            blockCollection.Add(SkillsKey, () => TextExtensions.SerializeStringTable(codec, Skills));
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

        private void EncodeMainScript(AllocatedBlockCollection allocatedBlocks)
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

                EnglishStringCodec.EncodeBlock(block, pointer, RomConfig.ScriptEncodingParameters);
            }
        }

        public override void WriteToRom(Block romData, AllocatedBlockCollection allocatedBlocks)
        {
            if (allocatedBlocks[TextBankKey] == null || allocatedBlocks[MainScriptKey] == null)
                throw new Exception("One or more offset tables were null.");

            WideOffsetTableWriter.UpdateOffsetTable(allocatedBlocks, TextBankKey, _textKeys);
            WideOffsetTableWriter.UpdateOffsetTable(allocatedBlocks, MainScriptKey, _mainScriptKeys);

            EncodeMainScript(allocatedBlocks);

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
