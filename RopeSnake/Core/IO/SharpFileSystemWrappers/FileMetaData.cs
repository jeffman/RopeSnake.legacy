using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFileSystem;

namespace RopeSnake.Core
{
    public sealed class FileMetaData
    {
        public FileSystemPath Path { get; private set; }
        public DateTime LastModified { get; private set; }
        public long Size { get; private set; }

        public FileMetaData(FileSystemPath path, DateTime lastModified, long size)
        {
            Path = path;
            LastModified = lastModified;
            Size = size;
        }
    }
}
