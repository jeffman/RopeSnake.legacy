using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RopeSnake.Mother3
{
    public sealed class Mother3ProjectSettings
    {
        [JsonProperty]
        public string BaseRomPath { get; set; }

        [JsonProperty]
        public string OutputRomPath { get; set; }

        [JsonProperty]
        public string RomConfigPath { get; set; }

        [JsonProperty]
        public OffsetTableMode OffsetTableMode { get; set; }

        public static Mother3ProjectSettings CreateDefault()
        {
            return new Mother3ProjectSettings
            {
                BaseRomPath = "base.gba",
                OutputRomPath = "test.gba",
                RomConfigPath = "rom.config.json",
                OffsetTableMode = OffsetTableMode.Fragmented
            };
        }
    }

    public enum OffsetTableMode
    {
        Fragmented,
        Contiguous
    }
}
