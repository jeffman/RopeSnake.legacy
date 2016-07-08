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
        private IFileManager _manager;

        protected IFileManager Manager { get { return _manager; } }

        public BinaryFileManager(IFileManager manager)
        {
            _manager = manager;
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
            int size = _manager.GetFileSize(path);

            using (var stream = _manager.OpenFile(path))
            {
                value.Deserialize(stream, size);
            }
        }

        public void WriteFile(string path, IBinarySerializable value)
        {
            using (var stream = _manager.CreateFile(path))
            {
                value.Serialize(stream);
            }
        }
    }
}
