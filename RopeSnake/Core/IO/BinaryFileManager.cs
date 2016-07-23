using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public class BinaryFileManager
    {
        private IFileSystem _fileSystem;

        protected IFileSystem Manager { get { return _fileSystem; } }

        public BinaryFileManager(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public T ReadFile<T>(string path)
            where T : IBinarySerializable, new()
        {
            var value = new T();
            ReadFile(path, value);
            return value;
        }

        public void ReadFile(string path, IBinarySerializable value)
        {
            int size = _fileSystem.GetFileSize(path);

            using (var stream = _fileSystem.OpenFile(path))
            {
                value.Deserialize(stream, size);
            }
        }

        public void WriteFile(string path, IBinarySerializable value)
        {
            using (var stream = _fileSystem.CreateFile(path))
            {
                value.Serialize(stream);
            }
        }
    }
}
