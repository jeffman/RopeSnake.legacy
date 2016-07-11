using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public class DiskFilesystem : IFilesystem
    {
        public string BasePath { get; set; }

        protected virtual string GetFullPath(string path)
        {
            return Path.Combine(BasePath, path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(GetFullPath(path));
        }

        public Stream CreateFile(string path)
        {
            return File.Create(GetFullPath(path));
        }

        public Stream OpenFile(string path)
        {
            return File.Open(GetFullPath(path), FileMode.Open);
        }

        public int GetFileSize(string path)
        {
            return (int)(new FileInfo(GetFullPath(path))).Length;
        }
    }
}
