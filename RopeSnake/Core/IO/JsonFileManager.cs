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
    public sealed class JsonFileManager : FileManagerBase
    {
        private static readonly string JsonExtension = "json";
        private object _lockObj = new object();

        public bool WriteByteArraysAsArrays { get; set; } = true;

        public JsonFileManager(IFileSystem fileSystem)
            : base(fileSystem)
        {

        }

        public T ReadJson<T>(FileSystemPath path)
        {
            lock (_lockObj)
            {
                CurrentIndex = IndexTotal.Single;
                return ReadJsonInternal<T>(path);
            }
        }

        public List<T> ReadJsonList<T>(FileSystemPath directory)
        {
            return ReadFileListAction(directory, JsonExtension, ReadJsonInternal<T>);
        }

        private T ReadJsonInternal<T>(FileSystemPath path)
        {
            T returnValue = default(T);

            ReadFileAction(path, stream =>
            {
                using (var textReader = new StreamReader(stream))
                {
                    using (var jsonReader = new JsonTextReader(textReader))
                    {
                        var serializer = new JsonSerializer();
                        returnValue = serializer.Deserialize<T>(jsonReader);
                    }
                }
            });

            return returnValue;
        }

        public void WriteJson(FileSystemPath path, object value)
        {
            lock (_lockObj)
            {
                CurrentIndex = IndexTotal.Single;
                WriteJsonInternal(path, value);
            }
        }

        public void WriteJsonList<T>(FileSystemPath directory, IList<T> list)
        {
            CreateFileListAction(directory, JsonExtension, list, (p, e) => WriteJsonInternal(p, e));
        }

        private void WriteJsonInternal(FileSystemPath path, object value)
        {
            CreateFileAction(path, value, stream =>
            {
                using (var textWriter = new StreamWriter(stream))
                {
                    using (var jsonWriter = new JsonTextWriter(textWriter))
                    {
                        var serializer = new JsonSerializer();
                        serializer.Formatting = Formatting.Indented;
                        if (WriteByteArraysAsArrays)
                        {
                            serializer.Converters.Add(new ByteArrayJsonConverter());
                        }
                        serializer.Serialize(jsonWriter, value);
                    }
                }
            });
        }
    }
}
