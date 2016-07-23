using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3
{
    public sealed class Mother3Project
    {
        public Block RomData { get; private set; }
        public Mother3RomConfig RomConfig { get; private set; }
        public Mother3ProjectSettings ProjectSettings { get; private set; }

        private Mother3Project() { }

        public static Mother3Project CreateNew(Block romData, Mother3RomConfig romConfig,
            Mother3ProjectSettings projectSettings)
        {
            return new Mother3Project
            {
                RomData = romData,
                RomConfig = romConfig,
                ProjectSettings = projectSettings
            };
        }

        public static Mother3Project Load(IFileSystem fileSystem, string projectSettingsPath)
        {
            var jsonManager = new JsonFileManager(fileSystem);
            var projectSettings = jsonManager.ReadJson<Mother3ProjectSettings>(projectSettingsPath);
            var romConfig = jsonManager.ReadJson<Mother3RomConfig>(projectSettings.RomConfigPath);

            var binaryManager = new BinaryFileManager(fileSystem);
            var romData = binaryManager.ReadFile<Block>(projectSettings.BaseRomPath);

            return CreateNew(romData, romConfig, projectSettings);
        }

        public void Save(IFileSystem fileSystem, string projectSettingsPath)
        {
            var jsonManager = new JsonFileManager(fileSystem);
            jsonManager.WriteJson(projectSettingsPath, ProjectSettings);
            jsonManager.WriteJson(ProjectSettings.RomConfigPath, RomConfig);
        }
    }
}
