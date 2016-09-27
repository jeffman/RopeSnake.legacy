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
        private static readonly int PaletteBufferSize = 16 * 16 * 2;
        private static readonly int TilemapBufferSize = 128 * 128 * 2;
        private static readonly int BigtileBufferSize = 0x300 * 8;

        public override string Name => "Maps";

        #region Static strings

        private static readonly string MapInfoKey = "Maps.Info";
        private static readonly string GraphicsInfoKey = "Maps.GraphicsInfo";
        private static readonly string TilesetsKey = "Maps.Tilesets";
        private static readonly string PalettesKey = "Maps.Palettes";
        private static readonly string TilemapsKey = "Maps.Tilemaps";
        private static readonly string BigtilesKey = "Maps.Bigtiles";

        private static readonly FileSystemPath MapInfoPath = "/maps/map-info.json".ToPath();
        private static readonly FileSystemPath GraphicsInfoPath = "/maps/graphics-info.json".ToPath();
        private static readonly FileSystemPath TilesetsFolder = "/maps/tilesets/".ToPath();
        private static readonly FileSystemPath PalettesFolder = "/maps/palettes/".ToPath();
        private static readonly FileSystemPath TilemapsFolder = "/maps/tilemaps/".ToPath();
        private static readonly FileSystemPath BigtilesFolder = "/maps/bigtiles/".ToPath();

        #endregion

        [NotNull(Flags = ValidateFlags.Instance | ValidateFlags.Collection), Validate(Flags = ValidateFlags.Collection)]
        public List<MapInfo> MapInfo { get; set; }

        [NotNull(Flags = ValidateFlags.Instance | ValidateFlags.Collection), Validate(Flags = ValidateFlags.Collection)]
        public List<GraphicsInfo> GraphicsInfo { get; set; }

        [NotNull]
        public List<GbaTileset> Tilesets { get; set; }

        [NotNull]
        public List<Palette> Palettes { get; set; }

        [NotNull]
        public List<GbaTilemap> Tilemaps { get; set; }

        [NotNull, Validate(Flags = ValidateFlags.Collection)]
        public List<BigtileSet> Bigtiles { get; set; }

        private string[] _tilesetKeys;
        private string[] _paletteKeys;
        private string[] _tilemapKeys;
        private string[] _bigtileKeys;

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
            Palettes = jsonManager.ReadJsonList<Palette>(PalettesFolder);
            Tilemaps = binaryManager.ReadFileList<GbaTilemap>(TilemapsFolder);
            Bigtiles = binaryManager.ReadFileList<BigtileSet>(BigtilesFolder);

            AddBlockKeysForFile(MapInfoPath, MapInfoKey);
            AddBlockKeysForFile(GraphicsInfoPath, GraphicsInfoKey);
            AddBlockKeysForFileList(binaryManager, TilesetsFolder, GetDataKeys(TilesetsKey, Tilesets.Count));
            AddBlockKeysForFileList(jsonManager, PalettesFolder, GetDataKeys(PalettesKey, Palettes.Count));
            AddBlockKeysForFileList(binaryManager, TilemapsFolder, GetDataKeys(TilemapsKey, Tilemaps.Count));
            AddBlockKeysForFileList(binaryManager, BigtilesFolder, GetDataKeys(BigtilesKey, Bigtiles.Count));
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
            jsonManager.WriteJsonList(PalettesFolder, Palettes);
            binaryManager.WriteFileList(TilemapsFolder, Tilemaps);
            binaryManager.WriteFileList(BigtilesFolder, Bigtiles);
        }

        public override void ReadFromRom(Block romData)
        {
            MapInfo = ReadDummyTable(romData, MapInfoKey, MapExtensions.ReadMapInfo);
            GraphicsInfo = ReadDummyTable(romData, GraphicsInfoKey, MapExtensions.ReadGraphicsInfo);
            Tilesets = ReadWideOffsetTable(romData, TilesetsKey, s => s.ReadCompressedTileset(4));
            Palettes = ReadWideOffsetTable(romData, PalettesKey, s => s.ReadPalette(16, 16));

            Tilemaps = ReadWideOffsetTable(romData, TilemapsKey, (s, i) => s.ReadCompressed((ss, l) =>
            {
                var mapInfo = MapInfo[i / 3];
                var layerInfo = mapInfo.Layers[i % 3];
                return ss.ReadMapTilemap(layerInfo.Width * 16, layerInfo.Height * 16);
            }));

            Bigtiles = ReadWideOffsetTable(romData, BigtilesKey, s => s.ReadCompressed((ss, l) => ss.ReadBigtileSet()));
        }

        public override ModuleSerializationResult Serialize()
        {
            var blocks = new LazyBlockCollection();
            var contiguousKeys = new List<List<string>>();

            blocks.Add(MapInfoKey, () => SerializeDummyTable(MapInfo, Maps.MapInfo.FieldSize, MapExtensions.WriteMapInfo));
            blocks.Add(GraphicsInfoKey, () => SerializeDummyTable(GraphicsInfo, Maps.GraphicsInfo.FieldSize, MapExtensions.WriteGraphicsInfo));

            _tilesetKeys = AddWideOffsetTable(blocks, Tilesets, TilesetsKey, TilesetBufferSize, (s, v) => s.WriteCompressedTileset(v, true));
            _paletteKeys = AddWideOffsetTable(blocks, Palettes, PalettesKey, PaletteBufferSize, (s, v) => s.WritePalette(v));
            _tilemapKeys = AddWideOffsetTable(blocks, Tilemaps, TilemapsKey, TilemapBufferSize, (s, v) => s.WriteCompressed(true, ss => ss.WriteMapTilemap(v)));
            _bigtileKeys = AddWideOffsetTable(blocks, Bigtiles, BigtilesKey, BigtileBufferSize, (s, v) => s.WriteCompressed(true, ss => ss.WriteBigtileSet(v)));

            if (ProjectSettings.OffsetTableMode == OffsetTableMode.Contiguous)
            {
                contiguousKeys.Add(new List<string>(_tilesetKeys));
                contiguousKeys.Add(new List<string>(_paletteKeys));
                contiguousKeys.Add(new List<string>(_tilemapKeys));
                contiguousKeys.Add(new List<string>(_bigtileKeys));
            }

            return new ModuleSerializationResult(blocks, contiguousKeys);
        }

        public override void WriteToRom(Block romData, AllocatedBlockCollection allocatedBlocks)
        {
            UpdateWideOffsetTable(allocatedBlocks, _tilesetKeys);
            UpdateWideOffsetTable(allocatedBlocks, _paletteKeys);
            UpdateWideOffsetTable(allocatedBlocks, _tilemapKeys);
            UpdateWideOffsetTable(allocatedBlocks, _bigtileKeys);

            WriteAllocatedBlocks(romData, allocatedBlocks);

            UpdateRomReferences(romData, allocatedBlocks, MapInfoKey, GraphicsInfoKey, TilesetsKey, PalettesKey, TilemapsKey, BigtilesKey);

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
