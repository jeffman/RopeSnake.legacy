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
using File = System.IO.File;

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
        private static readonly FileSystemPath CacheKeysFile = CachePath.AppendFile("keys.txt");
        private static readonly FileSystemPath FileSystemStatePath = CachePath.AppendFile("state.json");
        private static readonly FileSystemPath CompilationReportLog = "/compile.log".ToPath();

        public Mother3RomConfig RomConfig { get; private set; }
        public Mother3ProjectSettings ProjectSettings { get; private set; }
        public Mother3ModuleCollection Modules { get; private set; }
        public HashSet<object> StaleObjects { get; private set; }

        private Mother3Project(Mother3RomConfig romConfig,
            Mother3ProjectSettings projectSettings, params string[] modulesToLoad)
        {
            RomConfig = romConfig;
            ProjectSettings = projectSettings;
            Modules = new Mother3ModuleCollection(romConfig, projectSettings, modulesToLoad);
            StaleObjects = new HashSet<object>();
        }

        private void UpdateRomConfig(Block romData)
        {
            if (romData != null)
            {
                if (RomConfig.IsJapanese)
                    RomConfig.AddJapaneseCharsToLookup(romData);

                if (RomConfig.ScriptEncodingParameters != null)
                    RomConfig.ReadEncodingPadData(romData);
            }

            RomConfig.UpdateLookups();
        }

        public static Mother3Project CreateNew(string romDataPath, string romConfigPath, string outputDirectory)
        {
            foreach (var filePath in new[] { romDataPath, romConfigPath })
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"File not found: {filePath}", filePath);
            }

            if (File.Exists(outputDirectory))
                throw new Exception("Output path must be a directory");

            _log.Info($"Creating new project with ROM file \"{romDataPath}\" and config file \"{romConfigPath}\"");

            Block romData;
            var romDataFileInfo = new FileInfo(romDataPath);
            using (var disk = new PhysicalFileSystemWrapper(romDataFileInfo.DirectoryName))
            {
                var binaryManager = new BinaryFileManager(disk);
                romData = binaryManager.ReadFile<Block>(romDataFileInfo.Name.ToPath());
            }

            Mother3RomConfig romConfig;
            var romConfigFileInfo = new FileInfo(romConfigPath);
            using (var disk = new PhysicalFileSystemWrapper(romConfigFileInfo.DirectoryName))
            {
                var jsonManager = new JsonFileManager(disk);
                romConfig = jsonManager.ReadJson<Mother3RomConfig>(romConfigFileInfo.Name.ToPath());
            }

            if (romConfig.Patches != null)
            {
                foreach (var patchCollection in romConfig.Patches)
                {
                    _log.Info($"Applying patch: {patchCollection.Description}");
                    foreach (var patch in patchCollection)
                    {
                        patch.Apply(romData);
                    }
                }
            }

            var projectSettings = Mother3ProjectSettings.CreateDefault();
            var project = new Mother3Project(romConfig, projectSettings, DefaultModules);
            project.UpdateRomConfig(romData);

            foreach (var module in project.Modules)
            {
                _log.Info($"Reading module from ROM: {module.Name}");
                module.ReadFromRom(romData);
            }

            project.Modules.Data.UpdateNameHints(project.Modules.Text);
            project.Modules.Maps.UpdateNameHints(project.Modules.Text);

            // When we initially create a project, we want *all* files written, so we need to
            // set project.StaleObject to null
            project.StaleObjects = null;
            using (var disk = new PhysicalFileSystemWrapper(outputDirectory))
            {
                project.Save(disk, "/project.json".ToPath());
            }

            _log.Info("Copying base ROM to output directory");
            File.Copy(romDataPath, Path.Combine(outputDirectory, projectSettings.BaseRomFile.EntityName), true);

            _log.Info("Finished creating project");
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
            var baseRom = binaryManager.ReadFile<Block>(projectSettings.BaseRomFile);

            if (modulesToLoad == null || modulesToLoad.Length == 0)
                modulesToLoad = DefaultModules;

            var project = new Mother3Project(romConfig, projectSettings, modulesToLoad);
            project.UpdateRomConfig(baseRom);

            foreach (var module in project.Modules)
            {
                _log.Info($"Loading module: {module.Name}");
                module.ReadFromFiles(fileSystem);
            }

            return project;
        }

        public void Save(IFileSystem fileSystem, FileSystemPath projectSettingsPath)
        {
            _log.Info($"Saving project \"{projectSettingsPath.Path}\"");

            foreach (var module in Modules)
            {
                _log.Info($"Writing module {module.Name}");
                module.WriteToFiles(fileSystem, StaleObjects);
            }
            _log.Info("Finished writing modules");

            var jsonManager = new JsonFileManager(fileSystem);
            jsonManager.WriteJson(projectSettingsPath, ProjectSettings);
            jsonManager.WriteJson(ProjectSettings.RomConfigFile, RomConfig);
        }

        public void Compile(IFileSystemWrapper fileSystem, bool useCache)
        {
            _log.Info("Compiling project");

            var freeRanges = Modules.SelectMany(m => RomConfig.FreeRanges[m.Name])
                .Concat(RomConfig.FreeRanges["Nullspace"]);
            var allocator = new RangeAllocator(freeRanges);

            var binaryManager = new BinaryFileManager(fileSystem);
            var baseRom = binaryManager.ReadFile<Block>(ProjectSettings.BaseRomFile);
            var outputRomData = new Block(baseRom);

            BlockCollection cache;
            if (useCache)
            {
                _log.Info("Reading cache");
                var staleBlockKeys = GetStaleBlockKeys(fileSystem, GetChangedPaths(fileSystem));
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
            binaryManager.WriteFile(ProjectSettings.OutputRomFile, outputRomData);

            if (useCache)
            {
                _log.Info("Writing cache");
                WriteCache(fileSystem, compilationResult.WrittenBlocks, compilationResult.UpdatedKeys);

                var jsonManager = new JsonFileManager(fileSystem);
                jsonManager.WriteJson(FileSystemStatePath, fileSystem.GetState(FileSystemPath.Root, CachePath));
            }

            LogCompilationResult(fileSystem, compilationResult, allocator.Ranges);
            _log.Info("Finished compiling");
        }

        public BlockCollection ReadCache(IFileSystem fileSystem, IEnumerable<string> staleBlockKeys)
        {
            if (fileSystem.Exists(CachePath))
            {
                var binaryManager = new BinaryFileManager(fileSystem);
                return new BlockCollection(binaryManager.ReadFileDictionary<Block>(CachePath, staleBlockKeys));
            }
            else
            {
                return new BlockCollection();
            }
        }

        public void WriteCache(IFileSystem fileSystem, BlockCollection cache, IEnumerable<string> updatedKeys)
        {
            var binaryManager = new BinaryFileManager(fileSystem);
            binaryManager.StaleObjects = new HashSet<object>(updatedKeys.Select(k => cache[k]));
            binaryManager.WriteFileDictionary(CachePath, cache);
        }

        public bool Validate()
        {
            bool success = true;
            foreach (var module in Modules)
            {
                success &= module.Validate(new LazyString(module.Name));
            }
            return success;
        }

        private IEnumerable<string> GetStaleBlockKeys(IFileSystem fileSystem, IEnumerable<FileSystemPath> paths)
        {
            return Modules.SelectMany(m => paths.SelectMany(p => m.GetStaleBlockKeys(fileSystem, p)));
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
