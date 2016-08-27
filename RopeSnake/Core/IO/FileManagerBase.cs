using SharpFileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public abstract class FileManagerBase
    {
        private IFileSystem _fileSystem;
        protected IFileSystem FileSystem { get { return _fileSystem; } }

        public ISet<object> StaleObjects { get; set; }
        public event FileEventDelegate FileRead;
        public event FileEventDelegate FileWrite;

        protected FileManagerBase(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        protected virtual void OnFileRead(FileSystemPath path)
        {
            if (FileRead != null)
            {
                FileRead(this, new FileEventArgs(path));
            }
        }

        protected virtual void OnFileWrite(FileSystemPath path)
        {
            if (FileWrite != null)
            {
                FileWrite(this, new FileEventArgs(path));
            }
        }
    }
}
