using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public class DiskFileSystem : IFileSystem
    {
        public string BasePath { get; set; } = "";

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
            var fileInfo = new FileInfo(GetFullPath(path));
            CreateDirectoryInternal(fileInfo.Directory.FullName);
            return File.Create(fileInfo.FullName);
        }

        public Stream OpenFile(string path)
        {
            return File.Open(GetFullPath(path), FileMode.Open);
        }

        public bool DeleteFile(string path)
        {
            var fileInfo = new FileInfo(GetFullPath(path));
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
                return true;
            }
            return false;
        }

        public FileSystemProperties GetFileProperties(string path)
        {
            var fileInfo = new FileInfo(GetFullPath(path));
            return new FileSystemProperties(path, (int)fileInfo.Length, fileInfo.LastWriteTimeUtc);
        }

        public bool DirectoryExists(string path)
            => Directory.Exists(GetFullPath(path));

        public void CreateDirectory(string path)
            => CreateDirectoryInternal(GetFullPath(path));

        private void CreateDirectoryInternal(string fullPath)
        {
            var directoryInfo = new DirectoryInfo(fullPath);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
        }

        public string[] GetDirectories(string path)
        {
            var di = new DirectoryInfo(GetFullPath(path));

            if (!di.Exists)
                return null;

            return di.GetDirectories().Select(d => d.Name).ToArray();
        }

        public string[] GetFiles(string path)
        {
            var di = new DirectoryInfo(GetFullPath(path));
            return di.GetFiles().Select(f => f.Name).ToArray();
        }
    }
}
