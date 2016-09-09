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

        public override IEnumerable<Tuple<FileSystemPath, int>> EnumerateFileListPaths(int count, FileSystemPath directory)
        {
            if (!directory.IsDirectory)
                throw new ArgumentException(nameof(directory));

            for (int i = 0; i < count; i++)
            {
                yield return Tuple.Create(directory.AppendFile($"{i}.{JsonExtension}"), i);
            }
        }

        public override IEnumerable<Tuple<FileSystemPath, string>> EnumerateFileDictionaryPaths(IEnumerable<string> keys, FileSystemPath directory)
        {
            if (!directory.IsDirectory)
                throw new ArgumentException(nameof(directory));

            foreach (string key in keys)
            {
                yield return Tuple.Create(directory.AppendFile($"{key}.{JsonExtension}"), key);
            }
        }

        public List<T> ReadJsonList<T>(FileSystemPath directory)
        {
            return ReadFileListAction(directory, ReadJsonInternal<T>);
        }

        public Dictionary<string, T> ReadJsonDictionary<T>(FileSystemPath directory, IEnumerable<string> keysToIgnore = null)
        {
            return ReadFileDictionaryAction(directory, keysToIgnore, ReadJsonInternal<T>);
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
            CreateFileListAction(directory, list, (p, e) => WriteJsonInternal(p, e));
        }

        public void WriteFileDictionary<T>(FileSystemPath directory, IDictionary<string, T> dict)
        {
            CreateFileDictionaryAction(directory, dict, (p, e) => WriteJsonInternal(p, e));
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
