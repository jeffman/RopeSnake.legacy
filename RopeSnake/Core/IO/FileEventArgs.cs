using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFileSystem;

namespace RopeSnake.Core
{
    public sealed class FileEventArgs
    {
        public FileSystemPath Path { get; private set; }

        public FileEventArgs(FileSystemPath path)
        {
            Path = path;
        }
    }

    public delegate void FileEventDelegate(object sender, FileEventArgs e);
}
