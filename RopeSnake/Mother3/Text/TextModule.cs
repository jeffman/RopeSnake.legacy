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

        public TextModule(Mother3RomConfig romConfig)
            : base(romConfig)
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

        public override void WriteToRom(Block romData, AllocatedBlockCollection allocatedBlocks)
        {
            throw new NotImplementedException();
        }

        public override BlockCollection Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
