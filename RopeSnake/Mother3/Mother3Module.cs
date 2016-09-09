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
            fileManager.FileRead += FileReadEventHandler;
            fileManager.FileWrite += FileWriteEventHandler;
        }

        protected void UnregisterFileManagerProgress(FileManagerBase fileManager)
        {
            fileManager.FileRead -= FileReadEventHandler;
            fileManager.FileWrite -= FileWriteEventHandler;
        }

        private void FileReadEventHandler(object sender, FileEventArgs e)
        {
            FileEventHandlerInternal(sender, e, "Reading");
        }

        private void FileWriteEventHandler(object sender, FileEventArgs e)
        {
            FileEventHandlerInternal(sender, e, "Writing");
        }

        private void FileEventHandlerInternal(object sender, FileEventArgs e, string action)
        {
            if (Progress == null)
                return;

            string message;
            if (e.Index == IndexTotal.Single)
            {
                message = $"{action} {e.Path.Path}";
            }
            else
            {
                message = $"{action} {e.Path.Path} {e.Index}]";
            }

            Progress.Report(new ProgressPercent(message, e.Index.ToPercent()));
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

        protected Block[] SerializeWideOffsetTable<T>(IEnumerable<T> values, int bufferSize, Action<BinaryStream, T> elementWriter)
        {
            int count = values.Count();
            var offsetTable = WideOffsetTableWriter.CreateOffsetTable(count);
            var allBlocks = new Block[count + 1];
            allBlocks[0] = offsetTable;

            int index = 0;
            foreach (T value in values)
            {
                Block dataBlock;

                if (value != null)
                {
                    dataBlock = new Block(bufferSize);
                    var dataStream = dataBlock.ToBinaryStream();
                    elementWriter(dataStream, value);
                    dataBlock.Resize(dataStream.Position);
                }
                else
                {
                    dataBlock = null;
                }

                allBlocks[index + 1] = dataBlock;
                index++;
            }

            return allBlocks;
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
                try
                {
                    value.NameHint = nameHints[index];
                }
                catch (Exception ex) when (ex is IndexOutOfRangeException || ex is ArgumentOutOfRangeException)
                {
                    // Name hints are totally optional and can fail for many reasons,
                    // none of which are remotely fatal, so we can squash any index exceptions
                }
                finally
                {
                    index++;
                }
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
