using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SharpFileSystem;

namespace RopeSnake.Core
{
    public class BinaryFileManager : FileManagerBase
    {
        public BinaryFileManager(IFileSystem fileSystem)
            : base(fileSystem)
        {

        }

        public T ReadFile<T>(FileSystemPath path)
            where T : IBinarySerializable, new()
        {
            var value = new T();
            ReadFile(path, value);
            return value;
        }

        public void ReadFile(FileSystemPath path, IBinarySerializable value)
        {
            OnFileRead(path);

            if (!FileSystem.Exists(path))
                throw new FileNotFoundException("File not found", path.Path);

            using (var stream = FileSystem.OpenFile(path, FileAccess.Read))
            {
                value.Deserialize(stream, (int)stream.Length);
            }
        }

        public void WriteFile(FileSystemPath path, IBinarySerializable value)
        {
            OnFileWrite(path);

            using (var stream = FileSystem.CreateFile(path))
            {
                value.Serialize(stream);
            }
        }
    }
}
