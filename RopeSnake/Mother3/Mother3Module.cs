using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFileSystem;
using RopeSnake.Core;
using RopeSnake.Core.Validation;
using RopeSnake.Gba;
using RopeSnake.Mother3.IO;
using RopeSnake.Mother3.Text;
using NLog;

namespace RopeSnake.Mother3
{
    public abstract class Mother3Module : IModule
    {
        private Logger _log;
        protected Logger Log => _log ?? (_log = LogManager.GetLogger(GetType().Name));

        protected Mother3RomConfig RomConfig { get; }
        protected Mother3ProjectSettings ProjectSettings { get; }
        protected Dictionary<FileSystemPath, IEnumerable<string>> BlockKeysForFiles { get; }

        public IProgress<ProgressPercent> Progress { get; set; }

        protected Mother3Module(Mother3RomConfig romConfig, Mother3ProjectSettings projectSettings)
        {
            RomConfig = romConfig;
            ProjectSettings = projectSettings;
            BlockKeysForFiles = new Dictionary<FileSystemPath, IEnumerable<string>>();
        }

        #region Helpers

        protected void RegisterFileManagerProgress(FileManagerBase fileManager)
        {
            fileManager.FileRead += (s, e) => FileManagerBase.FileReadEventProgressHandler(s, e, Progress);
            fileManager.FileWrite += (s, e) => FileManagerBase.FileWriteEventProgressHandler(s, e, Progress);
        }

        protected void UnregisterFileManagerProgress(FileManagerBase fileManager)
        {
            fileManager.FileRead -= (s, e) => FileManagerBase.FileReadEventProgressHandler(s, e, Progress);
            fileManager.FileWrite -= (s, e) => FileManagerBase.FileWriteEventProgressHandler(s, e, Progress);
        }

        protected void UpdateRomReferences(Block romData, string key, int value)
        {
            var stream = romData.ToBinaryStream();
            var references = RomConfig.GetReferences(key);

            Log.Debug($"Updating references for {key}: new pointer = 0x{value:X}, ref locations = {string.Join(", ", references.Select(r => $"0x{r:X}"))}");

            foreach (int reference in references)
            {
                stream.Position = reference;
                stream.WriteGbaPointer(value);
            }
        }

        protected void UpdateRomReferences(Block romData,
            AllocatedBlockCollection allocatedBlocks, params object[] keys)
        {
            // It's just convenient to allow both strings and string arrays as params, so let's flatten them into one list
            var actualKeys = new HashSet<string>();

            foreach (var key in keys)
            {
                var str = key as string;
                if (str != null)
                {
                    actualKeys.Add(str);
                    continue;
                }

                var enumerable = key as IEnumerable<string>;
                if (enumerable != null)
                {
                    actualKeys.AddRange(enumerable);
                    continue;
                }

                throw new ArgumentException(nameof(keys));
            }

            foreach (string key in actualKeys)
            {
                int pointer = allocatedBlocks.GetAllocatedPointer(key);
                UpdateRomReferences(romData, key, pointer);
            }
        }

        protected static void WriteAllocatedBlocks(Block romData, AllocatedBlockCollection allocatedBlocks)
        {
            foreach (string key in allocatedBlocks.Keys)
            {
                var block = allocatedBlocks[key];
                if (block == null || block.Size == 0)
                    continue;

                int pointer = allocatedBlocks.GetAllocatedPointer(key);
                if (pointer == 0)
                    throw new Exception($"Attempted to write block with null pointer: {key}");

                block.CopyTo(romData.Data, pointer, 0, block.Size);
            }
        }

        protected List<T> ReadTable<T>(Block romData, string key, Func<BinaryStream, T> elementReader)
        {
            int offset = RomConfig.GetOffset(key, romData);
            int count = RomConfig.GetParameter<int>(key + ".Count");
            var stream = romData.ToBinaryStream(offset);
            return ReadTable(stream, count, key, elementReader);
        }

        protected List<T> ReadTable<T>(BinaryStream stream, int count, string key, Func<BinaryStream, T> elementReader)
        {
            var list = new List<T>();
            for (int i = 0; i < count; i++)
            {
                Log.Trace($"Reading element {i} at 0x{stream.Position:X} from table {key}");

                Progress?.Report(new ProgressPercent($"Reading {key} [{i + 1} / {count}]",
                    i * 100f / count));

                list.Add(elementReader(stream));
            }
            return list;
        }

        protected List<T> ReadDummyTable<T>(Block romData, string key, Func<BinaryStream, T> elementReader)
        {
            int offset = RomConfig.GetOffset(key, romData);
            int count = RomConfig.GetParameter<int>(key + ".Count");
            var stream = romData.ToBinaryStream(offset);
            var offsetTableReader = new WideOffsetTableReader(stream);

            if (offsetTableReader.Count != 1)
                throw new InvalidOperationException($"Tried to read a dummy table that contains {offsetTableReader.Count} offsets: {key}");

            if (!offsetTableReader.Next())
                throw new InvalidOperationException($"Null pointer in dummy table: {key}");

            return ReadTable(stream, count, key, elementReader);
        }

        protected static Block SerializeTable<T>(List<T> list, int fieldSize, Action<BinaryStream, T> elementWriter)
        {
            var block = new Block(list.Count * fieldSize);
            var stream = block.ToBinaryStream();

            foreach (T element in list)
                elementWriter(stream, element);

            return block;
        }

        protected static Block SerializeDummyTable<T>(IList<T> table, int fieldSize, Action<BinaryStream, T> elementWriter)
        {
            var block = WideOffsetTableWriter.CreateDummyTable(fieldSize, table.Count);
            var stream = block.ToBinaryStream(12);

            foreach (T element in table)
            {
                elementWriter(stream, element);
            }

            return block;
        }

        public static string[] GetOffsetAndDataKeys(string key)
        {
            return new string[] { $"{key}.Offsets", $"{key}.Data" };
        }

        protected void AddBlockKeysForFile(FileSystemPath path, params object[] keys)
        {
            var keySet = new HashSet<string>();

            foreach (var key in keys)
            {
                var keyString = key as string;
                if (keyString != null)
                {
                    keySet.Add(keyString);
                    continue;
                }

                var keyEnumerable = key as IEnumerable<string>;
                if (keyEnumerable != null)
                {
                    keySet.AddRange(keyEnumerable);
                    continue;
                }

                throw new ArgumentException(nameof(keys));
            }

            BlockKeysForFiles.Add(path, keySet);
        }

        protected void AddBlockKeysForFileList(FileManagerBase fileManager, FileSystemPath directory, string[] allKeys)
        {
            AddBlockKeysForFile(directory.AppendFile(FileManagerBase.CountFile), allKeys[0]);
            foreach (var pathItem in fileManager.EnumerateFileListPaths(allKeys.Length - 1, directory))
            {
                AddBlockKeysForFile(pathItem.Item1, allKeys[0], allKeys[pathItem.Item2 + 1]);
            }
        }

        protected static void UpdateWideOffsetTable(AllocatedBlockCollection allocatedBlocks, string[] allKeys)
            => UpdateWideOffsetTable(allocatedBlocks, allKeys[0], allKeys.Skip(1).ToArray());

        protected static void UpdateWideOffsetTable(AllocatedBlockCollection allocatedBlocks, string tableKey, string[] blockKeys)
        {
            if (!allocatedBlocks.ContainsKey(tableKey))
                throw new Exception($"Table key was not present: {tableKey}");

            var offsetTable = allocatedBlocks[tableKey];

            if (offsetTable == null)
                throw new Exception($"Offset table was null: {tableKey}");

            int offsetTableBase = allocatedBlocks.GetAllocatedPointer(tableKey);
            var newLocations = blockKeys.Select((k, i) => new IndexLocation(i, allocatedBlocks.GetAllocatedPointer(k)));
            int newCount = blockKeys.Length;

            WideOffsetTableWriter.UpdateTableOffsets(offsetTable, newLocations, offsetTableBase);
            WideOffsetTableWriter.UpdateTableCount(offsetTable, newCount);
        }

        protected List<T> ReadWideOffsetTable<T>(Block romData, string tableKey, Func<BinaryStream, T> elementReader)
        {
            var list = new List<T>();
            var stream = romData.ToBinaryStream(RomConfig.GetOffset(tableKey, romData));
            var offsetTableReader = new WideOffsetTableReader(stream);

            while (!offsetTableReader.EndOfTable)
            {
                Progress?.Report(new ProgressPercent($"Reading {tableKey} [{offsetTableReader.CurrentIndex + 1}/{offsetTableReader.Count}]",
                    offsetTableReader.CurrentIndex * 100f / offsetTableReader.Count));

                if (offsetTableReader.Next())
                {
                    list.Add(elementReader(stream));
                }
                else
                {
                    list.Add(default(T));
                }
            }

            return list;
        }

        protected Func<Block>[] SerializeWideOffsetTable<T>(IEnumerable<T> values, int bufferSize, Action<BinaryStream, T> elementWriter)
        {
            int count = values.Count();
            Func<Block> offsetTable = () => WideOffsetTableWriter.CreateOffsetTable(count);
            var allBlocks = new Func<Block>[count + 1];
            allBlocks[0] = offsetTable;

            int index = 0;
            foreach (T value in values)
            {
                Func<Block> dataBlock;

                if (value != null)
                {
                    dataBlock = () =>
                    {
                        var block = new Block(bufferSize);
                        var dataStream = block.ToBinaryStream();
                        elementWriter(dataStream, value);
                        block.Resize(dataStream.Position);
                        return block;
                    };
                }
                else
                {
                    dataBlock = () => null;
                }

                allBlocks[index + 1] = dataBlock;
                index++;
            }

            return allBlocks;
        }

        protected string[] AddWideOffsetTable<T>(LazyBlockCollection blockCollection, IEnumerable<T> values, string tableKey,
            int bufferSize, Action<BinaryStream, T> elementWriter)
        {
            var keys = GetDataKeys(tableKey, values.Count());
            var lazyBlocks = SerializeWideOffsetTable(values, bufferSize, elementWriter);
            blockCollection.AddRange(keys.Zip(lazyBlocks, (k, f) => new KeyValuePair<string, Func<Block>>(k, f)));
            return keys;
        }

        protected static string[] GetDataKeys(string key, int count)
        {
            return Enumerable.Repeat(key, 1).Concat(Enumerable.Range(0, count).Select(i => $"{key}.{i}")).ToArray();
        }

        #endregion

        public override string ToString() => Name;

        public virtual void UpdateNameHints(TextModule textModule)
        {

        }

        protected static void UpdateNameHints(IEnumerable<INameHint> values, IList<string> nameHints)
        {
            int index = 0;
            foreach (var value in values)
            {
                if (index < nameHints.Count)
                    value.NameHint = nameHints[index];
                else
                    break;

                index++;
            }
        }

        #region IModule implementation

        public abstract string Name { get; }
        public abstract void ReadFromRom(Block romData);
        public abstract void WriteToRom(Block romData, AllocatedBlockCollection allocatedBlocks);
        public abstract void ReadFromFiles(IFileSystem fileSystem);
        public abstract void WriteToFiles(IFileSystem fileSystem, ISet<object> staleObjects);
        public abstract ModuleSerializationResult Serialize();

        public virtual bool Validate(LazyString path)
        {
            return Validator.Object(this, path, Log);
        }

        public virtual IEnumerable<string> GetStaleBlockKeys(IFileSystem fileSystem, FileSystemPath path)
        {
            IEnumerable<string> keys;
            if (BlockKeysForFiles.TryGetValue(path, out keys))
            {
                return keys;
            }
            return Enumerable.Empty<string>();
        }

        #endregion
    }
}
