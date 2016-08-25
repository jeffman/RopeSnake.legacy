using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SharpFileSystem;

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
            var filePath = path.ToPath();

            if (!_fileSystem.Exists(filePath))
                throw new FileNotFoundException("File not found", path);

            using (var stream = _fileSystem.OpenFile(filePath, FileAccess.Read))
            {
                value.Deserialize(stream, (int)stream.Length);
            }
        }

        public void WriteFile(string path, IBinarySerializable value)
        {
            using (var stream = _fileSystem.CreateFile(path.ToPath()))
            {
                value.Serialize(stream);
            }
        }
    }
}
