using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using RopeSnake.Mother3.Data;

namespace RopeSnake.Mother3
{
    public sealed class Mother3Project
    {
        public Block RomData { get; private set; }
        public Mother3RomConfig RomConfig { get; private set; }
        public Mother3ProjectSettings ProjectSettings { get; private set; }
        public Mother3ModuleCollection Modules { get; private set; }

        private Mother3Project(Block romData, Mother3RomConfig romConfig,
            Mother3ProjectSettings projectSettings)
        {
            RomData = romData;
            RomConfig = romConfig;
            ProjectSettings = projectSettings;
            Modules = new Mother3ModuleCollection(romConfig);
        }

        public static Mother3Project CreateNew(Block romData, Mother3RomConfig romConfig,
            Mother3ProjectSettings projectSettings)
        {
            var project = new Mother3Project(romData, romConfig, projectSettings);

            foreach (var module in project.Modules)
                module.ReadFromRom(romData);

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

            foreach (var module in project.Modules)
                module.ReadFromFiles(fileSystem);

            return project;
        }

        public void Save(IFileSystem fileSystem, string projectSettingsPath)
        {
            var jsonManager = new JsonFileManager(fileSystem);
            jsonManager.WriteJson(projectSettingsPath, ProjectSettings);
            jsonManager.WriteJson(ProjectSettings.RomConfigPath, RomConfig);

            foreach (var module in Modules)
                module.WriteToFiles(fileSystem);
        }

        public void Compile(IFileSystem fileSystem)
        {
            var allocator = new RangeAllocator(RomConfig.FreeRanges);
            var outputRomData = new Block(RomData);
            var compiler = Compiler.Create(outputRomData, allocator, Modules);
            compiler.Compile();

            var binaryManager = new BinaryFileManager(fileSystem);
            binaryManager.WriteFile(ProjectSettings.OutputRomPath, outputRomData);
        }
    }
}
