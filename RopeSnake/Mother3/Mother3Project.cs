using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFileSystem;
using RopeSnake.Core;
using RopeSnake.Mother3.Data;

namespace RopeSnake.Mother3
{
    public sealed class Mother3Project
    {
        private static readonly string CacheFolder = ".cache";
        private static readonly string CacheKeysFile = "Cache.Keys.json";
        private static readonly string FileSystemStatePath = "state.json";

        public Block RomData { get; private set; }
        public Mother3RomConfig RomConfig { get; private set; }
        public Mother3ProjectSettings ProjectSettings { get; private set; }
        public Mother3ModuleCollection Modules { get; private set; }
        public HashSet<object> StaleObjects { get; private set; }

        private Mother3Project(Block romData, Mother3RomConfig romConfig,
            Mother3ProjectSettings projectSettings)
        {
            RomData = romData;
            RomConfig = romConfig;
            ProjectSettings = projectSettings;
            Modules = new Mother3ModuleCollection(romConfig, projectSettings);
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

        public static Mother3Project CreateNew(IFileSystem fileSystem, string romDataPath,
            string romConfigPath)
        {
            var binaryManager = new BinaryFileManager(fileSystem);
            var jsonManager = new JsonFileManager(fileSystem);

            var romData = binaryManager.ReadFile<Block>(romDataPath);
            var romConfig = jsonManager.ReadJson<Mother3RomConfig>(romConfigPath);
            var projectSettings = Mother3ProjectSettings.CreateDefault();

            var project = new Mother3Project(romData, romConfig, projectSettings);
            project.UpdateRomConfig();

            foreach (var module in project.Modules)
                module.ReadFromRom(romData);

            project.StaleObjects = null;

            return project;
        }

        public static Mother3Project Load(IFileSystem fileSystem, string projectSettingsPath)
        {
            var jsonManager = new JsonFileManager(fileSystem);
            var projectSettings = jsonManager.ReadJson<Mother3ProjectSettings>(projectSettingsPath);
            var romConfig = jsonManager.ReadJson<Mother3RomConfig>(projectSettings.RomConfigPath);

            var binaryManager = new BinaryFileManager(fileSystem);
            var romData = binaryManager.ReadFile<Block>(projectSettings.BaseRomPath);

            var project = new Mother3Project(romData, romConfig, projectSettings);
            project.UpdateRomConfig();

            foreach (var module in project.Modules)
                module.ReadFromFiles(fileSystem);

            return project;
        }

        public void Save(IFileSystem fileSystem, string projectSettingsPath)
        {
            foreach (var module in Modules)
                module.WriteToFiles(fileSystem, StaleObjects);

            var jsonManager = new JsonFileManager(fileSystem);
            jsonManager.WriteJson(projectSettingsPath, ProjectSettings);
            jsonManager.WriteJson(ProjectSettings.RomConfigPath, RomConfig);
        }

        public void Compile(IFileSystem fileSystem)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var allocator = new RangeAllocator(RomConfig.FreeRanges);
            var outputRomData = new Block(RomData);

            var staleBlockKeys = new string[] { };// GetStaleBlockKeys(GetChangedPaths(fileSystem));
            var cache = ReadCache(fileSystem, staleBlockKeys);

            var compiler = Compiler.Create(outputRomData, allocator, Modules, cache);
            compiler.AllocationAlignment = 4;
            var compilationResult = compiler.Compile();

            var binaryManager = new BinaryFileManager(fileSystem);
            binaryManager.WriteFile(ProjectSettings.OutputRomPath, outputRomData);

            WriteCache(fileSystem, compilationResult.WrittenBlocks, compilationResult.UpdatedKeys);
            CleanCache(fileSystem);

            var jsonManager = new JsonFileManager(fileSystem);
            //jsonManager.WriteJson(FileSystemStatePath, fileSystem.GetState("^\\.cache"));

            sw.Stop();
            var time = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine(time);
            Console.ReadLine();
        }

        public BlockCollection ReadCache(IFileSystem fileSystem, IEnumerable<string> staleBlockKeys)
        {
            var cache = new BlockCollection();
            var binaryManager = new BinaryFileManager(fileSystem);
            var jsonManager = new JsonFileManager(fileSystem);

            if (fileSystem.Exists(CacheFolder.ToPath()))
            {
                string cacheKeysPath = Path.Combine(CacheFolder, CacheKeysFile);
                if (fileSystem.Exists(cacheKeysPath.ToPath()))
                {
                    var cacheKeys = jsonManager.ReadJson<string[]>(cacheKeysPath);
                    foreach (string cacheKey in cacheKeys.Except(staleBlockKeys))
                    {
                        string cacheFilePath = Path.Combine(CacheFolder, $"{cacheKey}.bin");
                        Block block;
                        if (fileSystem.Exists(cacheFilePath.ToPath()))
                        {
                            block = binaryManager.ReadFile<Block>(cacheFilePath);
                        }
                        else
                        {
                            block = null;
                        }
                        cache.Add(cacheKey, block);
                    }
                }
            }

            return cache;
        }

        public void WriteCache(IFileSystem fileSystem, BlockCollection cache, IEnumerable<string> updatedKeys)
        {
            var binaryManager = new BinaryFileManager(fileSystem);
            var jsonManager = new JsonFileManager(fileSystem);

            jsonManager.WriteJson(Path.Combine(CacheFolder, CacheKeysFile), cache.Keys.ToArray());

            foreach (var key in updatedKeys)
            {
                var block = cache[key];
                string cacheFilePath = Path.Combine(CacheFolder, $"{key}.bin");

                if (block != null)
                {
                    binaryManager.WriteFile(cacheFilePath, block);
                }
                else
                {
                    fileSystem.Delete(cacheFilePath.ToPath());
                }
            }
        }

        public void CleanCache(IFileSystem fileSystem)
        {
            string cacheKeysPath = Path.Combine(CacheFolder, CacheKeysFile);
            var jsonManager = new JsonFileManager(fileSystem);

            if (fileSystem.Exists(cacheKeysPath.ToPath()))
            {
                var cachedFiles = new HashSet<string>(jsonManager.ReadJson<string[]>(cacheKeysPath).Select(f => $"{f}.bin"));

                var cacheFolderPath = CacheFolder.ToPath();
                var existingFiles = fileSystem.GetEntities(cacheFolderPath);

                foreach (var file in existingFiles)
                {
                    if (file == CacheKeysFile.ToPath() || cachedFiles.Contains(file.Path))
                        continue;

                    fileSystem.Delete(Path.Combine(CacheFolder, file.Path).ToPath());
                }
            }
        }

        private IEnumerable<string> GetStaleBlockKeys(IEnumerable<string> paths)
        {
            return Modules.SelectMany(m => paths.SelectMany(p => m.GetBlockKeysForFile(p)));
        }

        private FileSystemState GetPreviousFileSystemState(IFileSystem fileSystem)
        {
            if (!fileSystem.Exists(FileSystemStatePath.ToPath()))
                return new FileSystemState(Enumerable.Empty<FileSystemProperties>());

            var jsonManager = new JsonFileManager(fileSystem);
            return jsonManager.ReadJson<FileSystemState>(FileSystemStatePath);
        }

        //private IEnumerable<string> GetChangedPaths(IFileSystem fileSystem)
        //{
        //    var previousState = GetPreviousFileSystemState(fileSystem);
        //    var currentState = fileSystem.GetState();
        //    var differences = currentState.Compare(previousState);
        //    return differences.Keys;
        //}
    }
}
