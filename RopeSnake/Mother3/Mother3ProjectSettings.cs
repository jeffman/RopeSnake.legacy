﻿using System;
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
        public string BaseRomPath { get; private set; }

        [JsonProperty]
        public string OutputRomPath { get; private set; }

        [JsonProperty]
        public string RomConfigPath { get; private set; }
    }
}
