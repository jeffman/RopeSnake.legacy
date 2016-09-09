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

        public override IEnumerable<Tuple<FileSystemPath, int>> EnumerateFileListPaths(int count, FileSystemPath directory)
        {
            if (!directory.IsDirectory)
                throw new ArgumentException(nameof(directory));

            for (int i = 0; i < count; i++)
            {
                yield return Tuple.Create(directory.AppendFile($"{i}.{BinExtension}"), i);
            }
        }

        public override IEnumerable<Tuple<FileSystemPath, string>> EnumerateFileDictionaryPaths(IEnumerable<string> keys, FileSystemPath directory)
        {
            if (!directory.IsDirectory)
                throw new ArgumentException(nameof(directory));

            foreach (string key in keys)
            {
                yield return Tuple.Create(directory.AppendFile($"{key}.{BinExtension}"), key);
            }
        }

        public List<T> ReadFileList<T>(FileSystemPath directory) where T : IBinarySerializable, new()
        {
            return ReadFileListAction(directory, ReadFileInternal<T>);
        }

        public Dictionary<string, T> ReadFileDictionary<T>(FileSystemPath directory, IEnumerable<string> keysToIgnore = null)
            where T : IBinarySerializable, new()
        {
            return ReadFileDictionaryAction(directory, keysToIgnore, ReadFileInternal<T>);
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
            CreateFileListAction(directory, list, (p, e) => WriteFileInternal(p, e));
        }

        public void WriteFileDictionary<T>(FileSystemPath directory, IDictionary<string, T> dict) where T : IBinarySerializable
        {
            CreateFileDictionaryAction(directory, dict, (p, e) => WriteFileInternal(p, e));
        }

        private void WriteFileInternal(FileSystemPath path, IBinarySerializable value)
        {
            CreateFileAction(path, value, value.Serialize);
        }
    }
}
