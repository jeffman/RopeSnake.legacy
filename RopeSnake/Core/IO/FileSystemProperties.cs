using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public sealed class FileSystemProperties
    {
        public string Path { get; }
        public int Size { get; }
        public DateTime LastModified { get; }

        public FileSystemProperties(string path, int size, DateTime lastModified)
        {
            Path = path;
            Size = size;
            LastModified = lastModified;
        }

        public override string ToString()
        {
            return $"{Path}, {Size}, {LastModified}";
        }
    }
}
