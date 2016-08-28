using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFileSystem;
using RopeSnake.Core;
using RopeSnake.Mother3.Data;
using System.Diagnostics;
using NLog;

namespace RopeSnake.Mother3
{
    public sealed class Mother3Project
    {
        private static readonly string[] DefaultModules =
        {
            "Data",
            "Text",
            "Maps"
        };

        private static readonly Logger _log = LogManager.GetLogger(nameof(Mother3Project));

        private static readonly FileSystemPath CachePath = "/.cache/".ToPath();
        private static readonly FileSystemPath CacheKeysFile = CachePath.AppendFile("Cache.Keys.json");
        private static readonly FileSystemPath FileSystemStatePath = CachePath.AppendFile("State.json");
        private static readonly FileSystemPath CompilationReportLog = "/compile.log".ToPath();

        public Block RomData { get; private set; }
        public Mother3RomConfig RomConfig { get; private set; }
        public Mother3ProjectSettings ProjectSettings { get; private set; }
        public Mother3ModuleCollection Modules { get; private set; }
        public HashSet<object> StaleObjects { get; private set; }

        private Mother3Project(Block romData, Mother3RomConfig romConfig,
            Mother3ProjectSettings projectSettings, params string[] modulesToLoad)
        {
            RomData = romData;
            RomConfig = romConfig;
            ProjectSettings = projectSettings;
            Modules = new Mother3ModuleCollection(romConfig, projectSettings, modulesToLoad);
            StaleObjects = new HashSet<object>();
        }

        private void UpdateRomConfig()
        {
            if (RomConfig.IsJapanese)
                RomConfig.AddJapaneseCharsToLookup(RomData);

            if (RomConfig.ScriptEncodingParameters != null)
                RomConfig.ReadEncodingPadData(RomData);

            RomConfig.UpdateLookups();
        }

        public static Mother3Project CreateNew(IFileSystem fileSystem, FileSystemPath romDataPath,
            FileSystemPath romConfigPath)
        {
            _log.Info($"Creating new project with ROM file \"{romDataPath.Path}\" and config file \"{romConfigPath.Path}\"");

            var binaryManager = new BinaryFileManager(fileSystem);
            var jsonManager = new JsonFileManager(fileSystem);

            var romData = binaryManager.ReadFile<Block>(romDataPath);
            var romConfig = jsonManager.ReadJson<Mother3RomConfig>(romConfigPath);
            var projectSettings = Mother3ProjectSettings.CreateDefault();

            var project = new Mother3Project(romData, romConfig, projectSettings, DefaultModules);
            project.UpdateRomConfig();

            foreach (var module in project.Modules)
            {
                _log.Info($"Reading module from ROM: {module.Name}");
                module.ReadFromRom(romData);
            }

            project.StaleObjects = null;

            return project;
        }

        public static Mother3Project Load(IFileSystem fileSystem, FileSystemPath projectSettingsPath,
            params string[] modulesToLoad)
        {
            _log.Info($"Loading project \"{projectSettingsPath.Path}\" with modules {string.Join(", ", modulesToLoad)}");

            var jsonManager = new JsonFileManager(fileSystem);
            var projectSettings = jsonManager.ReadJson<Mother3ProjectSettings>(projectSettingsPath);
            var romConfig = jsonManager.ReadJson<Mother3RomConfig>(projectSettings.RomConfigFile);

            var binaryManager = new BinaryFileManager(fileSystem);
            var romData = binaryManager.ReadFile<Block>(projectSettings.BaseRomFile);

            if (modulesToLoad == null || modulesToLoad.Length == 0)
                modulesToLoad = DefaultModules;

            var project = new Mother3Project(romData, romConfig, projectSettings, modulesToLoad);
            project.UpdateRomConfig();

            foreach (var module in project.Modules)
            {
                _log.Info($"Loading module: {module.Name}");
                module.ReadFromFiles(fileSystem);
            }

            return project;
        }

        public void Save(IFileSystem fileSystem, FileSystemPath projectSettingsPath)
        {
            _log.Info($"Loading project \"{projectSettingsPath.Path}\"");

            foreach (var module in Modules)
            {
                _log.Info($"Writing module {module.Name}");
                module.WriteToFiles(fileSystem, StaleObjects);
            }

            var jsonManager = new JsonFileManager(fileSystem);
            jsonManager.WriteJson(projectSettingsPath, ProjectSettings);
            jsonManager.WriteJson(ProjectSettings.RomConfigFile, RomConfig);

            if (!fileSystem.Exists(ProjectSettings.BaseRomFile))
            {
                var binaryManager = new BinaryFileManager(fileSystem);
                binaryManager.WriteFile(ProjectSettings.BaseRomFile, RomData);
            }
        }

        public void Compile(IFileSystemWrapper fileSystem, bool useCache)
        {
            _log.Info("Compiling project");

            var freeRanges = Modules.SelectMany(m => RomConfig.FreeRanges[m.Name])
                .Concat(RomConfig.FreeRanges["Nullspace"]);
            var allocator = new RangeAllocator(freeRanges);

            var outputRomData = new Block(RomData);

            BlockCollection cache;
            if (useCache)
            {
                _log.Info("Reading cache");
                var staleBlockKeys = GetStaleBlockKeys(GetChangedPaths(fileSystem));
                cache = ReadCache(fileSystem, staleBlockKeys);
            }
            else
            {
                cache = new BlockCollection();
            }

            _log.Info("Executing compiler");
            var compiler = Compiler.Create(outputRomData, allocator, Modules, cache);
            compiler.AllocationAlignment = 4;
            var compilationResult = compiler.Compile();

            _log.Debug("Filling free ranges with 0xFF");
            FillFreeRanges(outputRomData, allocator.Ranges, 0xFF);

            _log.Info($"Writing output ROM file to {ProjectSettings.OutputRomFile.Path}");
            var binaryManager = new BinaryFileManager(fileSystem);
            binaryManager.WriteFile(ProjectSettings.OutputRomFile, outputRomData);

            if (useCache)
            {
                _log.Info("Writing cache");
                WriteCache(fileSystem, compilationResult.WrittenBlocks, compilationResult.UpdatedKeys);
                CleanCache(fileSystem);

                var jsonManager = new JsonFileManager(fileSystem);
                jsonManager.WriteJson(FileSystemStatePath, fileSystem.GetState(FileSystemPath.Root, CachePath));
            }

            LogCompilationResult(fileSystem, compilationResult, allocator.Ranges);
        }

        public BlockCollection ReadCache(IFileSystem fileSystem, IEnumerable<string> staleBlockKeys)
        {
            var cache = new BlockCollection();
            var binaryManager = new BinaryFileManager(fileSystem);
            var jsonManager = new JsonFileManager(fileSystem);

            if (fileSystem.Exists(CacheKeysFile))
            {
                var cacheKeys = jsonManager.ReadJson<string[]>(CacheKeysFile);
                foreach (string key in cacheKeys.Except(staleBlockKeys))
                {
                    var cacheFilePath = CachePath.AppendFile($"{key}.bin");
                    Block block;

                    if (fileSystem.Exists(cacheFilePath))
                    {
                        block = binaryManager.ReadFile<Block>(cacheFilePath);
                    }
                    else
                    {
                        block = null;
                    }

                    cache.Add(key, block);
                }
            }

            return cache;
        }

        public void WriteCache(IFileSystem fileSystem, BlockCollection cache, IEnumerable<string> updatedKeys)
        {
            var binaryManager = new BinaryFileManager(fileSystem);
            var jsonManager = new JsonFileManager(fileSystem);

            jsonManager.WriteJson(CacheKeysFile, cache.Keys.ToArray());

            foreach (var key in updatedKeys)
            {
                var block = cache[key];
                var cacheFilePath = CachePath.AppendFile($"{key}.bin");

                if (block != null)
                {
                    binaryManager.WriteFile(cacheFilePath, block);
                }
                else
                {
                    fileSystem.Delete(cacheFilePath);
                }
            }
        }

        public void CleanCache(IFileSystem fileSystem)
        {
            var jsonManager = new JsonFileManager(fileSystem);

            if (fileSystem.Exists(CacheKeysFile))
            {
                var cachedFiles = new HashSet<FileSystemPath>(jsonManager.ReadJson<string[]>(CacheKeysFile).Select(f => CachePath.AppendFile($"{f}.bin")));
                var existingFiles = fileSystem.GetEntities(CachePath);

                foreach (var file in existingFiles)
                {
                    if (file == CacheKeysFile || cachedFiles.Contains(file))
                        continue;

                    fileSystem.Delete(file);
                }
            }
        }

        private IEnumerable<string> GetStaleBlockKeys(IEnumerable<FileSystemPath> paths)
        {
            return Modules.SelectMany(m => paths.SelectMany(p => m.GetBlockKeysForFile(p)));
        }

        private FileSystemState GetPreviousFileSystemState(IFileSystem fileSystem)
        {
            if (!fileSystem.Exists(FileSystemStatePath))
                return new FileSystemState(Enumerable.Empty<FileMetaData>());

            var jsonManager = new JsonFileManager(fileSystem);
            return jsonManager.ReadJson<FileSystemState>(FileSystemStatePath);
        }

        private IEnumerable<FileSystemPath> GetChangedPaths(IFileSystemWrapper fileSystem)
        {
            var previousState = GetPreviousFileSystemState(fileSystem);
            var currentState = fileSystem.GetState(FileSystemPath.Root, CachePath);
            var differences = currentState.Compare(previousState);
            return differences.Keys;
        }

        private static void LogCompilationResult(IFileSystem fileSystem, Compiler.CompilationResult result,
            IEnumerable<Range> remainingRanges)
        {
            using (var logFile = fileSystem.CreateFile(CompilationReportLog))
            {
                using (var writer = new StreamWriter(logFile))
                {
                    writer.WriteLine($"Compilation report, {DateTime.Now}");
                    writer.WriteLine("====");
                    writer.WriteLine();

                    if (remainingRanges.Count() > 0)
                    {
                        int totalFree = remainingRanges.Sum(r => r.Size);
                        writer.WriteLine($"Remaining free ranges: {totalFree} (0x{totalFree:X}) bytes total");

                        var logger = new LogTableWriter(writer);
                        logger.AddHeader("Start", 12);
                        logger.AddHeader("End", 12);
                        logger.AddHeader("Size", 12);
                        logger.AddHeader("Size (hex)", 12);
                        logger.WriteHeader(2);
                        foreach (var range in remainingRanges)
                        {
                            logger.WriteLine(2, $"0x{range.Start:X}", $"0x{range.End:X}", range.Size, $"0x{range.Size:X}");
                        }
                        writer.WriteLine();
                    }

                    if (result.WrittenBlocks.Count > 0)
                    {
                        writer.WriteLine("Blocks written to ROM:");

                        var logger = new LogTableWriter(writer);
                        logger.AddHeader("", 4);
                        logger.AddHeader("Key", 40);
                        logger.AddHeader("Size", 12);
                        logger.AddHeader("Size (hex)", 12);
                        logger.AddHeader("Location", 12);
                        logger.WriteHeader(2);
                        foreach (var kv in result.WrittenBlocks.Where(kv => kv.Value != null)
                            .OrderBy(kv => result.AllocationResult[kv.Key]))
                        {
                            var key = kv.Key;
                            var block = kv.Value;
                            logger.WriteLine(2, result.UpdatedKeys.Contains(key) ? "[*]" : "", key, block.Size, $"0x{block.Size:X}", $"0x{result.AllocationResult[key]:X}");
                        }
                    }
                }
            }
        }

        private void FillFreeRanges(Block romData, IEnumerable<Range> freeRanges, byte fillValue)
        {
            foreach (var range in freeRanges)
            {
                for (int position = range.Start; position <= range.End; position++)
                {
                    romData[position] = fillValue;
                }
            }
        }
    }
}
