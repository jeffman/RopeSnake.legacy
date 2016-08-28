using SharpFileSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace RopeSnake.Core
{
    public abstract class FileManagerBase
    {
        private Logger _log;
        protected Logger Log => _log ?? (_log = LogManager.GetLogger(GetType().Name));

        private IFileSystem _fileSystem;
        private object _lockObj = new object();
        protected IFileSystem FileSystem { get { return _fileSystem; } }
        protected IndexTotal CurrentIndex { get; set; }

        protected string CountFile { get; } = "count.txt";
        protected string KeysFile { get; } = "keys.txt";

        public ISet<object> StaleObjects { get; set; }
        public event FileEventDelegate FileRead;
        public event FileEventDelegate FileWrite;
        public bool SuppressEvents { get; set; } = false;

        protected FileManagerBase(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;

            if (Log.IsTraceEnabled)
            {
                FileRead += (s, e) => Log.Trace($"Reading {e.Path.Path} {e.Index}");
                FileWrite += (s, e) => Log.Trace($"Writing {e.Path.Path} {e.Index}");
            }
        }

        protected virtual void OnFileRead(FileSystemPath path, IndexTotal index)
        {
            if (FileRead != null && !SuppressEvents)
            {
                FileRead(this, new FileEventArgs(path, index));
            }
        }

        protected virtual void OnFileWrite(FileSystemPath path, IndexTotal index)
        {
            if (FileWrite != null && !SuppressEvents)
            {
                FileWrite(this, new FileEventArgs(path, index));
            }
        }

        protected void ReadFileAction(FileSystemPath path, Action<Stream> action)
        {
            if (!path.IsFile)
                throw new ArgumentException(nameof(path));

            if (!FileSystem.Exists(path))
                throw new FileNotFoundException("File not found", path.Path);

            OnFileRead(path, CurrentIndex);

            using (var stream = FileSystem.OpenFile(path, FileAccess.Read))
            {
                action(stream);
            }
        }

        protected void CreateFileAction(FileSystemPath path, object value, Action<Stream> action)
        {
            if (!path.IsFile)
                throw new ArgumentException(nameof(path));

            if (!IsStale(value))
                return;

            OnFileWrite(path, CurrentIndex);

            if (!FileSystem.Exists(path.ParentPath))
            {
                FileSystem.CreateDirectoryRecursive(path.ParentPath);
            }

            using (var stream = FileSystem.CreateFile(path))
            {
                action(stream);
            }
        }

        protected List<T> ReadFileListAction<T>(FileSystemPath directory, string extension, Func<FileSystemPath, T> action)
        {
            if (!directory.IsDirectory)
                throw new ArgumentException(nameof(directory));

            var list = new List<T>();

            lock (_lockObj)
            {
                CurrentIndex = IndexTotal.Single;
                var countPath = directory.AppendFile(CountFile);

                if (!FileSystem.Exists(countPath))
                    throw new Exception($"The count file {CountFile} is missing from {directory.Path}.");

                int count = 0;
                ReadFileAction(countPath, stream =>
                {
                    using (var reader = new StreamReader(stream))
                    {
                        if (!int.TryParse(reader.ReadToEnd(), out count))
                        {
                            throw new FormatException("The count file must contain a single integer string.");
                        }
                    }
                });

                for (int i = 0; i < count; i++)
                {
                    CurrentIndex = new IndexTotal(i + 1, count);
                    var path = directory.AppendFile($"{i}.{extension}");

                    if (FileSystem.Exists(path))
                    {
                        list.Add(action(path));
                    }
                    else
                    {
                        list.Add(default(T));
                    }
                }
            }

            return list;
        }

        protected void CreateFileListAction<T>(FileSystemPath directory, string extension, IList<T> list, Action<FileSystemPath, T> action)
        {
            if (!directory.IsDirectory)
                throw new ArgumentException(nameof(directory));

            lock (_lockObj)
            {
                CurrentIndex = IndexTotal.Single;
                CreateFileAction(directory.AppendFile(CountFile), null, stream =>
                {
                    using (var writer = new StreamWriter(stream))
                        writer.Write(list.Count.ToString());
                });

                for (int i = 0; i < list.Count; i++)
                {
                    CurrentIndex = new IndexTotal(i + 1, list.Count);
                    var path = directory.AppendFile($"{i}.{extension}");

                    T value = list[i];
                    if (value != null)
                    {
                        action(path, value);
                    }
                    else
                    {
                        FileSystem.Delete(path);
                    }
                }
            }
        }

        protected Dictionary<string, T> ReadFileDictionaryAction<T>(FileSystemPath directory, string extension,
            IEnumerable<string> keysToIgnore, Func<FileSystemPath, T> action)
        {
            if (!directory.IsDirectory)
                throw new ArgumentException(nameof(directory));

            var dict = new Dictionary<string, T>();

            lock (_lockObj)
            {
                CurrentIndex = IndexTotal.Single;
                var keysPath = directory.AppendFile(KeysFile);

                if (!FileSystem.Exists(keysPath))
                    throw new Exception($"The keys file {KeysFile} is missing from {directory.Path}.");

                var keys = new List<string>();
                ReadFileAction(keysPath, stream =>
                {
                    using (var reader = new StreamReader(stream))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (keysToIgnore != null && !keysToIgnore.Contains(line))
                                keys.Add(line);
                        }
                    }
                });

                for (int i = 0; i < keys.Count; i++)
                {
                    string key = keys[i];
                    CurrentIndex = new IndexTotal(i + 1, keys.Count);
                    var path = directory.AppendFile($"{key}.{extension}");

                    if (FileSystem.Exists(path))
                    {
                        dict.Add(key, action(path));
                    }
                    else
                    {
                        dict.Add(key, default(T));
                    }
                }
            }

            return dict;
        }

        protected void CreateFileDictionaryAction<T>(FileSystemPath directory, string extension, IEnumerable<KeyValuePair<string, T>> dict,
            Action<FileSystemPath, T> action)
        {
            if (!directory.IsDirectory)
                throw new ArgumentException(nameof(directory));

            lock (_lockObj)
            {
                CurrentIndex = IndexTotal.Single;
                int count = 0;
                CreateFileAction(directory.AppendFile(KeysFile), null, stream =>
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        foreach (var kv in dict)
                        {
                            writer.WriteLine(kv.Key);
                            count++;
                        }
                    }
                });

                int i = 0;
                foreach (var kv in dict)
                {
                    string key = kv.Key;
                    CurrentIndex = new IndexTotal(i + 1, count);
                    var path = directory.AppendFile($"{key}.{extension}");

                    T value = kv.Value;
                    if (value != null)
                    {
                        action(path, kv.Value);
                    }
                    else
                    {
                        FileSystem.Delete(path);
                    }

                    i++;
                }
            }
        }

        protected virtual bool IsStale(object value)
        {
            if (StaleObjects == null || value == null)
                return true;

            if (StaleObjects.Contains(value))
                return true;

            var enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                foreach (var child in enumerable)
                {
                    if (StaleObjects.Contains(child))
                        return true;
                }
            }

            return false;
        }
    }
}
