﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace RopeSnake.Core
{
    public class JsonFileManager
    {
        private IFileSystem _fileSystem;

        protected IFileSystem FileSystem { get { return _fileSystem; } }

        public JsonFileManager(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public virtual T ReadJson<T>(string path)
        {
            var serializer = new JsonSerializer();
            using (var stream = _fileSystem.OpenFile(path))
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

        public virtual void WriteJson(string path, object value)
        {
            var serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;

            using (var stream = _fileSystem.CreateFile(path))
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
    }
}
