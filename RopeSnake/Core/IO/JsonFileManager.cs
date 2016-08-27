using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using SharpFileSystem;

namespace RopeSnake.Core
{
    public class JsonFileManager : FileManagerBase
    {
        public bool WriteByteArraysAsArrays { get; set; } = true;

        public JsonFileManager(IFileSystem fileSystem)
            : base(fileSystem)
        {

        }

        public virtual T ReadJson<T>(FileSystemPath path)
        {
            OnFileRead(path);

            var serializer = new JsonSerializer();
            if (WriteByteArraysAsArrays)
            {
                serializer.Converters.Add(new ByteArrayJsonConverter());
            }

            using (var stream = FileSystem.OpenFile(path, FileAccess.Read))
            {
                using (var textReader = new StreamReader(stream))
                {
                    using (var jsonReader = new JsonTextReader(textReader))
                    {
                        return serializer.Deserialize<T>(jsonReader);
                    }
                }
            }
        }

        public virtual void WriteJson(FileSystemPath path, object value)
        {
            if (!IsStale(value))
                return;

            OnFileWrite(path);

            var serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            if (WriteByteArraysAsArrays)
            {
                serializer.Converters.Add(new ByteArrayJsonConverter());
            }

            if (!FileSystem.Exists(path.ParentPath))
            {
                FileSystem.CreateDirectoryRecursive(path.ParentPath);
            }

            using (var stream = FileSystem.CreateFile(path))
            {
                using (var textWriter = new StreamWriter(stream))
                {
                    using (var jsonWriter = new JsonTextWriter(textWriter))
                    {
                        serializer.Serialize(jsonWriter, value);
                    }
                }
            }
        }

        protected virtual bool IsStale(object value)
        {
            if (StaleObjects == null)
                return true;

            if (StaleObjects.Contains(value))
                return true;

            var enumerable = value as IEnumerable;
            foreach (var child in enumerable)
            {
                if (StaleObjects.Contains(child))
                    return true;
            }

            return false;
        }
    }
}
