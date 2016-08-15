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
            var stream = romData.ToBinaryStream(RomConfig.GetOffset("Text.Bank", romData));
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

            blockCollection.AddStringOffsetTableBlocks("Text.RoomDescriptions", codec, RoomDescriptions, false, false);
            blockCollection.AddBlock("Text.ItemNames", TextStreamExtensions.SerializeStringTable(codec, ItemNames));
            blockCollection.AddStringOffsetTableBlocks("Text.ItemDescriptions", codec, ItemDescriptions, false, false);
            blockCollection.AddBlock("Text.CharNames", TextStreamExtensions.SerializeStringTable(codec, CharNames));
            blockCollection.AddBlock("Text.PartyCharNames", TextStreamExtensions.SerializeStringTable(codec, PartyCharNames));
            blockCollection.AddBlock("Text.EnemyNames", TextStreamExtensions.SerializeStringTable(codec, EnemyNames));
            blockCollection.AddBlock("Text.PsiNames", TextStreamExtensions.SerializeStringTable(codec, PsiNames));
            blockCollection.AddStringOffsetTableBlocks("Text.PsiDescriptions", codec, PsiDescriptions, false, false);
            blockCollection.AddBlock("Text.Statuses", TextStreamExtensions.SerializeStringTable(codec, Statuses));
            blockCollection.AddBlock("Text.DefaultCharNames", TextStreamExtensions.SerializeStringTable(codec, DefaultCharNames));
            blockCollection.AddBlock("Text.Skills", TextStreamExtensions.SerializeStringTable(codec, Skills));
            blockCollection.AddStringOffsetTableBlocks("Text.SkillDescriptions", codec, SkillDescriptions, false, false);

            _textKeys = blockCollection.Keys.ToArray();
            blockCollection.AddBlock("Text.Bank", WideOffsetTableWriter.CreateOffsetTable(16));

            return blockCollection;
        }

        private void ReadMainScript(Block romData, StringCodec codec)
        {
            var stream = romData.ToBinaryStream(RomConfig.GetOffset("Text.MainScript", romData));
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
                blockCollection.AddStringOffsetTableBlocks($"Text.MainScript.{i}", codec, MainScript[i], true, false);
            }

            _mainScriptKeys = blockCollection.Keys.ToArray();
            blockCollection.AddBlock("Text.MainScript", WideOffsetTableWriter.CreateOffsetTable(MainScript.Count * 2));

            return blockCollection;
        }

        public override void WriteToRom(Block romData, AllocatedBlockCollection allocatedBlocks)
        {
            WideOffsetTableWriter.UpdateOffsetTable(allocatedBlocks, "Text.Bank", _textKeys, 0);
            WideOffsetTableWriter.UpdateOffsetTable(allocatedBlocks, "Text.MainScript", _mainScriptKeys, 0);

            WriteAllocatedBlocks(romData, allocatedBlocks);
            UpdateRomReferences(romData, allocatedBlocks, "Text.Bank", "Text.MainScript");

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
