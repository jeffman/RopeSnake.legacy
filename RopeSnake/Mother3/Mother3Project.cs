using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using RopeSnake.Mother3.Data;

namespace RopeSnake.Mother3
{
    public sealed class Mother3Project
    {
        private static readonly string CacheFolder = ".cache";
        private static readonly string CacheKeysFile = "Cache.Keys.json";

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
            var allocator = new RangeAllocator(RomConfig.FreeRanges);
            var outputRomData = new Block(RomData);
            var cache = ReadCache(fileSystem);
            RemoveStaleObjectsFromCache(cache);

            var compiler = Compiler.Create(outputRomData, allocator, Modules, cache);
            compiler.AllocationAlignment = 4;
            var compilationResult = compiler.Compile();

            var binaryManager = new BinaryFileManager(fileSystem);
            binaryManager.WriteFile(ProjectSettings.OutputRomPath, outputRomData);

            RemoveCacheFromStaleObjects(compilationResult.WrittenBlocks);
            WriteCache(fileSystem, compilationResult.WrittenBlocks, compilationResult.UpdatedKeys);
            CleanCache(fileSystem);
        }

        public BlockCollection ReadCache(IFileSystem fileSystem)
        {
            var cache = new BlockCollection();
            var binaryManager = new BinaryFileManager(fileSystem);
            var jsonManager = new JsonFileManager(fileSystem);

            if (fileSystem.DirectoryExists(CacheFolder))
            {
                string cacheKeysPath = Path.Combine(CacheFolder, CacheKeysFile);
                if (fileSystem.FileExists(cacheKeysPath))
                {
                    var cacheKeys = jsonManager.ReadJson<string[]>(cacheKeysPath);
                    foreach (string cacheKey in cacheKeys)
                    {
                        string cacheFilePath = Path.Combine(CacheFolder, $"{cacheKey}.bin");
                        if (fileSystem.FileExists(cacheFilePath))
                        {
                            var block = binaryManager.ReadFile<Block>(cacheFilePath);
                            cache.Add(cacheKey, block);
                        }
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
                    fileSystem.DeleteFile(cacheFilePath);
                }
            }
        }

        public void CleanCache(IFileSystem fileSystem)
        {
            string cacheKeysPath = Path.Combine(CacheFolder, CacheKeysFile);
            var jsonManager = new JsonFileManager(fileSystem);

            if (fileSystem.FileExists(cacheKeysPath))
            {
                var cachedFiles = new HashSet<string>(jsonManager.ReadJson<string[]>(cacheKeysPath).Select(f => $"{f}.bin"));
                var existingFiles = fileSystem.GetFiles(CacheFolder);

                foreach (string file in existingFiles)
                {
                    if (file == CacheKeysFile || cachedFiles.Contains(file))
                        continue;

                    fileSystem.DeleteFile(Path.Combine(CacheFolder, file));
                }
            }
        }

        public void RemoveStaleObjectsFromCache(BlockCollection cache)
        {
            if (ProjectSettings.StaleCacheKeys == null)
                return;

            foreach (string key in ProjectSettings.StaleCacheKeys)
            {
                cache.Remove(key);
            }
        }

        public void RemoveCacheFromStaleObjects(BlockCollection cache)
        {
            if (ProjectSettings.StaleCacheKeys == null)
                return;

            foreach (var key in cache.Keys)
            {
                ProjectSettings.StaleCacheKeys.Remove(key);
            }
        }
    }
}
