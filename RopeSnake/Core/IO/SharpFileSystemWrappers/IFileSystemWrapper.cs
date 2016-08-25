using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFileSystem;

namespace RopeSnake.Core
{
    public interface IFileSystemWrapper : IFileSystem
    {
        FileMetaData GetMetaData(FileSystemPath path);
    }
}
