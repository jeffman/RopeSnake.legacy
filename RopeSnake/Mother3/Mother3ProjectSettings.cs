using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFileSystem;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RopeSnake.Core;

namespace RopeSnake.Mother3
{
    public sealed class Mother3ProjectSettings
    {
        [JsonProperty, JsonConverter(typeof(FileSystemPathConverter))]
        public FileSystemPath BaseRomFile { get; set; }

        [JsonProperty, JsonConverter(typeof(FileSystemPathConverter))]
        public FileSystemPath OutputRomFile { get; set; }

        [JsonProperty, JsonConverter(typeof(FileSystemPathConverter))]
        public FileSystemPath RomConfigFile { get; set; }

        [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
        public OffsetTableMode OffsetTableMode { get; set; }

        public int MaxThreads { get; set; }

        public static Mother3ProjectSettings CreateDefault()
        {
            return new Mother3ProjectSettings
            {
                BaseRomFile = "/base.gba".ToPath(),
                OutputRomFile = "/test.gba".ToPath(),
                RomConfigFile = "/rom.config.json".ToPath(),
                OffsetTableMode = OffsetTableMode.Fragmented,
                MaxThreads = 1
            };
        }
    }

    public enum OffsetTableMode
    {
        Fragmented,
        Contiguous
    }
}
