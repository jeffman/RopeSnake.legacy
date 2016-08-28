using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFileSystem;
using Newtonsoft.Json;
using RopeSnake.Core;
using RopeSnake.Mother3.IO;

namespace RopeSnake.Mother3.Maps
{
    public sealed class MapModule : Mother3Module
    {
        public override string Name => "Maps";

        #region Static strings

        private static readonly string MapInfoKey = "Maps.Info";

        private static readonly FileSystemPath MapInfoPath = "/maps/map-info.json".ToPath();

        #endregion

        public List<MapInfo> MapInfo { get; set; }

        private string[] _mapInfoKeys;

        public MapModule(Mother3RomConfig romConfig, Mother3ProjectSettings projectSettings)
            : base(romConfig, projectSettings)
        {

        }

        public override void ReadFromFiles(IFileSystem fileSystem)
        {
            var jsonManager = new JsonFileManager(fileSystem);
            RegisterFileManagerProgress(jsonManager);

            MapInfo = jsonManager.ReadJson<List<MapInfo>>(MapInfoPath);
        }

        public override void WriteToFiles(IFileSystem fileSystem, ISet<object> staleObjects)
        {
            var jsonManager = new JsonFileManager(fileSystem);
            RegisterFileManagerProgress(jsonManager);
            jsonManager.StaleObjects = staleObjects;

            jsonManager.WriteJson(MapInfoPath, MapInfo);
        }

        public override void ReadFromRom(Block romData)
        {
            MapInfo = ReadDummyTable(romData, MapInfoKey, MapExtensions.ReadMapInfo);
        }

        public override ModuleSerializationResult Serialize()
        {
            var blocks = new LazyBlockCollection();
            var contiguousKeys = new List<List<string>>();

            _mapInfoKeys = AddDummyResults(SerializeDummyTable(MapInfo, Maps.MapInfo.FieldSize, MapInfoKey, MapExtensions.WriteMapInfo), blocks, contiguousKeys);

            return new ModuleSerializationResult(blocks, contiguousKeys);
        }

        public override void WriteToRom(Block romData, AllocatedBlockCollection allocatedBlocks)
        {
            if (allocatedBlocks[MapInfoKey] == null)
                throw new Exception("One or more offset tables were null.");

            WideOffsetTableWriter.UpdateOffsetTable(allocatedBlocks, MapInfoKey, _mapInfoKeys);

            WriteAllocatedBlocks(romData, allocatedBlocks);
            UpdateRomReferences(romData, allocatedBlocks, MapInfoKey);

            _mapInfoKeys = null;
        }

        public void UpdateNameHints(Text.TextModule textModule)
        {
            for (int i = 0; i < MapInfo.Count; i++)
                MapInfo[i].NameHint = textModule.RoomDescriptions[i];
        }
    }
}
