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
using RopeSnake.Gba;
using RopeSnake.Graphics;

namespace RopeSnake.Mother3.Maps
{
    [Validate]
    public sealed class MapModule : Mother3Module
    {
        private static readonly int TilesetBufferSize = 128 * 1024;

        public override string Name => "Maps";

        #region Static strings

        private static readonly string MapInfoKey = "Maps.Info";
        private static readonly string GraphicsInfoKey = "Maps.GraphicsInfo";
        private static readonly string TilesetsKey = "Maps.Tilesets";

        private static readonly FileSystemPath MapInfoPath = "/maps/map-info.json".ToPath();
        private static readonly FileSystemPath GraphicsInfoPath = "/maps/graphics-info.json".ToPath();
        private static readonly FileSystemPath TilesetsFolder = "/maps/tilesets/".ToPath();

        #endregion

        [NotNull(Flags = ValidateFlags.Instance | ValidateFlags.Collection), Validate(Flags = ValidateFlags.Collection)]
        public List<MapInfo> MapInfo { get; set; }

        [NotNull(Flags = ValidateFlags.Instance | ValidateFlags.Collection), Validate(Flags = ValidateFlags.Collection)]
        public List<GraphicsInfo> GraphicsInfo { get; set; }

        [NotNull]
        public List<GbaTileset> Tilesets { get; set; }

        private string[] _tilesetKeys;

        public MapModule(Mother3RomConfig romConfig, Mother3ProjectSettings projectSettings)
            : base(romConfig, projectSettings)
        {

        }

        public override void ReadFromFiles(IFileSystem fileSystem)
        {
            var jsonManager = new JsonFileManager(fileSystem);
            RegisterFileManagerProgress(jsonManager);

            var binaryManager = new BinaryFileManager(fileSystem);
            RegisterFileManagerProgress(binaryManager);

            MapInfo = jsonManager.ReadJson<List<MapInfo>>(MapInfoPath);
            GraphicsInfo = jsonManager.ReadJson<List<GraphicsInfo>>(GraphicsInfoPath);
            Tilesets = binaryManager.ReadFileList<GbaTileset>(TilesetsFolder);

            AddBlockKeysForFile(MapInfoPath, MapInfoKey);
            AddBlockKeysForFile(GraphicsInfoPath, GraphicsInfoKey);
            AddBlockKeysForFileList(binaryManager, TilesetsFolder, GetDataKeys(TilesetsKey, Tilesets.Count));
        }

        public override void WriteToFiles(IFileSystem fileSystem, ISet<object> staleObjects)
        {
            var jsonManager = new JsonFileManager(fileSystem);
            RegisterFileManagerProgress(jsonManager);
            jsonManager.StaleObjects = staleObjects;

            var binaryManager = new BinaryFileManager(fileSystem);
            RegisterFileManagerProgress(binaryManager);
            binaryManager.StaleObjects = staleObjects;

            jsonManager.WriteJson(MapInfoPath, MapInfo);
            jsonManager.WriteJson(GraphicsInfoPath, GraphicsInfo);
            binaryManager.WriteFileList(TilesetsFolder, Tilesets);
        }

        public override void ReadFromRom(Block romData)
        {
            MapInfo = ReadDummyTable(romData, MapInfoKey, MapExtensions.ReadMapInfo);
            GraphicsInfo = ReadDummyTable(romData, GraphicsInfoKey, MapExtensions.ReadGraphicsInfo);
            Tilesets = ReadWideOffsetTable(romData, TilesetsKey, s => s.ReadCompressedTileset(4));
        }

        public override ModuleSerializationResult Serialize()
        {
            var blocks = new LazyBlockCollection();
            var contiguousKeys = new List<List<string>>();

            blocks.Add(MapInfoKey, () => SerializeDummyTable(MapInfo, Maps.MapInfo.FieldSize, MapExtensions.WriteMapInfo));
            blocks.Add(GraphicsInfoKey, () => SerializeDummyTable(GraphicsInfo, Maps.GraphicsInfo.FieldSize, MapExtensions.WriteGraphicsInfo));

            _tilesetKeys = AddWideOffsetTable(blocks, Tilesets, TilesetsKey, TilesetBufferSize, (s, v) => s.WriteCompressedTileset(v, true));

            if (ProjectSettings.OffsetTableMode == OffsetTableMode.Contiguous)
            {
                contiguousKeys.Add(new List<string>(_tilesetKeys));
            }

            return new ModuleSerializationResult(blocks, contiguousKeys);
        }

        public override void WriteToRom(Block romData, AllocatedBlockCollection allocatedBlocks)
        {
            UpdateWideOffsetTable(allocatedBlocks, _tilesetKeys);

            WriteAllocatedBlocks(romData, allocatedBlocks);

            UpdateRomReferences(romData, allocatedBlocks, MapInfoKey, GraphicsInfoKey, TilesetsKey);

            _tilesetKeys = null;
        }

        public override void UpdateNameHints(Text.TextModule textModule)
        {
            base.UpdateNameHints(textModule);

            UpdateNameHints(MapInfo, textModule.RoomDescriptions);
            UpdateNameHints(GraphicsInfo, textModule.RoomDescriptions);
        }
    }
}
