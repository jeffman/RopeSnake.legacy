using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFileSystem;
using Newtonsoft.Json;
using RopeSnake.Core;
using RopeSnake.Mother3.IO;
using RopeSnake.Core.Validation;

namespace RopeSnake.Mother3.Maps
{
    [Validate]
    public sealed class MapModule : Mother3Module
    {
        public override string Name => "Maps";

        #region Static strings

        private static readonly string MapInfoKey = "Maps.Info";
        private static readonly string GraphicsInfoKey = "Maps.GraphicsInfo";

        private static readonly FileSystemPath MapInfoPath = "/maps/map-info.json".ToPath();
        private static readonly FileSystemPath GraphicsInfoPath = "/maps/graphics-info.json".ToPath();

        #endregion

        [NotNull(Flags = ValidateFlags.Instance | ValidateFlags.Collection), Validate(Flags = ValidateFlags.Collection)]
        public List<MapInfo> MapInfo { get; set; }

        [NotNull(Flags = ValidateFlags.Instance | ValidateFlags.Collection), Validate(Flags = ValidateFlags.Collection)]
        public List<GraphicsInfo> GraphicsInfo { get; set; }

        public MapModule(Mother3RomConfig romConfig, Mother3ProjectSettings projectSettings)
            : base(romConfig, projectSettings)
        {

        }

        public override void ReadFromFiles(IFileSystem fileSystem)
        {
            var jsonManager = new JsonFileManager(fileSystem);
            RegisterFileManagerProgress(jsonManager);

            MapInfo = jsonManager.ReadJson<List<MapInfo>>(MapInfoPath);
            GraphicsInfo = jsonManager.ReadJson<List<GraphicsInfo>>(GraphicsInfoPath);
        }

        public override void WriteToFiles(IFileSystem fileSystem, ISet<object> staleObjects)
        {
            var jsonManager = new JsonFileManager(fileSystem);
            RegisterFileManagerProgress(jsonManager);
            jsonManager.StaleObjects = staleObjects;

            jsonManager.WriteJson(MapInfoPath, MapInfo);
            jsonManager.WriteJson(GraphicsInfoPath, GraphicsInfo);
        }

        public override void ReadFromRom(Block romData)
        {
            MapInfo = ReadDummyTable(romData, MapInfoKey, MapExtensions.ReadMapInfo);
            GraphicsInfo = ReadDummyTable(romData, GraphicsInfoKey, MapExtensions.ReadGraphicsInfo);
        }

        public override ModuleSerializationResult Serialize()
        {
            var blocks = new LazyBlockCollection();
            var contiguousKeys = new List<List<string>>();

            blocks.Add(MapInfoKey, () => SerializeDummyTable(MapInfo, Maps.MapInfo.FieldSize, MapExtensions.WriteMapInfo));
            blocks.Add(GraphicsInfoKey, () => SerializeDummyTable(GraphicsInfo, Maps.GraphicsInfo.FieldSize, MapExtensions.WriteGraphicsInfo));

            return new ModuleSerializationResult(blocks, contiguousKeys);
        }

        public override void WriteToRom(Block romData, AllocatedBlockCollection allocatedBlocks)
        {
            WriteAllocatedBlocks(romData, allocatedBlocks);
            UpdateRomReferences(romData, allocatedBlocks, MapInfoKey, GraphicsInfoKey);
        }

        public override void UpdateNameHints(Text.TextModule textModule)
        {
            base.UpdateNameHints(textModule);

            UpdateNameHints(MapInfo, textModule.RoomDescriptions);
            UpdateNameHints(GraphicsInfo, textModule.RoomDescriptions);
        }
    }
}
