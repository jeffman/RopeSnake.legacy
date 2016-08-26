using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFileSystem;
using Newtonsoft.Json;

namespace RopeSnake.Core
{
    public sealed class FileMetaData
    {
        [JsonProperty, JsonConverter(typeof(FileSystemPathConverter))]
        public FileSystemPath Path { get; private set; }

        [JsonProperty]
        public DateTime LastModified { get; private set; }

        [JsonProperty]
        public long Size { get; private set; }

        public FileMetaData(FileSystemPath path, DateTime lastModified, long size)
        {
            Path = path;
            LastModified = lastModified;
            Size = size;
        }
    }
}
