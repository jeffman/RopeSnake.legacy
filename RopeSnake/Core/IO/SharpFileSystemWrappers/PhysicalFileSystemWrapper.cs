using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SharpFileSystem;
using SharpFileSystem.FileSystems;

namespace RopeSnake.Core
{
    public class PhysicalFileSystemWrapper : PhysicalFileSystem, IFileSystemWrapper
    {
        public PhysicalFileSystemWrapper(string physicalRoot)
            : base(physicalRoot)
        {

        }

        public FileMetaData GetMetaData(FileSystemPath path)
        {
            var physicalPath = GetPhysicalPath(path);

            if (!Exists(path))
                throw new FileNotFoundException($"File or directory not found: {Path.GetFullPath(physicalPath)}", Path.GetFullPath(physicalPath));

            if (path.IsDirectory)
            {
                var directoryInfo = new DirectoryInfo(physicalPath);
                return new FileMetaData(path, directoryInfo.LastWriteTimeUtc, -1);
            }
            else
            {
                var fileInfo = new FileInfo(physicalPath);
                return new FileMetaData(path, fileInfo.LastWriteTimeUtc, fileInfo.Length);
            }
        }
    }
}
