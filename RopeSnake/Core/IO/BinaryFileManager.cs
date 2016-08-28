using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SharpFileSystem;

namespace RopeSnake.Core
{
    public sealed class BinaryFileManager : FileManagerBase
    {
        private static readonly string BinExtension = "bin";
        private object _lockObj = new object();

        public BinaryFileManager(IFileSystem fileSystem)
            : base(fileSystem)
        {

        }

        public T ReadFile<T>(FileSystemPath path) where T : IBinarySerializable, new()
        {
            lock (_lockObj)
            {
                CurrentIndex = IndexTotal.Single;
                return ReadFileInternal<T>(path);
            }
        }

        private T ReadFileInternal<T>(FileSystemPath path) where T : IBinarySerializable, new()
        {
            var value = new T();
            ReadFile(path, value);
            return value;
        }

        public void ReadFile(FileSystemPath path, IBinarySerializable value)
        {
            lock (_lockObj)
            {
                CurrentIndex = IndexTotal.Single;
                ReadFileInternal(path, value);
            }
        }

        public List<T> ReadFileList<T>(FileSystemPath directory) where T : IBinarySerializable, new()
        {
            return ReadFileListAction(directory, BinExtension, ReadFileInternal<T>);
        }

        private void ReadFileInternal(FileSystemPath path, IBinarySerializable value)
        {
            ReadFileAction(path, s => value.Deserialize(s, (int)s.Length));
        }

        public void WriteFile(FileSystemPath path, IBinarySerializable value)
        {
            lock (_lockObj)
            {
                CurrentIndex = IndexTotal.Single;
                WriteFileInternal(path, value);
            }
        }

        public void WriteFileList<T>(FileSystemPath directory, IList<T> list) where T : IBinarySerializable
        {
            CreateFileListAction(directory, BinExtension, list, (p, e) => WriteFileInternal(p, e));
        }

        private void WriteFileInternal(FileSystemPath path, IBinarySerializable value)
        {
            CreateFileAction(path, value, value.Serialize);
        }
    }
}
