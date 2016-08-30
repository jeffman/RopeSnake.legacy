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
            AllocatedBlockCollection allocatedBlocks, params string[] keys)
        {
            foreach (string key in keys)
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
            return ReadTable(stream, count, elementReader);
        }

        protected List<T> ReadTable<T>(BinaryStream stream, int count, Func<BinaryStream, T> elementReader)
        {
            var list = new List<T>();
            for (int i = 0; i < count; i++)
                list.Add(elementReader(stream));

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

            return ReadTable(stream, count, elementReader);
        }

        protected static Block SerializeTable<T>(List<T> list, int fieldSize, Action<BinaryStream, T> elementWriter)
        {
            var block = new Block(list.Count * fieldSize);
            var stream = block.ToBinaryStream();

            foreach (T element in list)
                elementWriter(stream, element);

            return block;
        }

        protected static SerializeDummyTableResult SerializeDummyTable<T>(List<T> list, int fieldSize, string key, Action<BinaryStream, T> elementWriter)
        {
            string[] keys = { key, $"{key}.Table" };
            var blocks = new LazyBlockCollection();
            blocks.Add(key, () => WideOffsetTableWriter.CreateOffsetTable(1));
            blocks.Add(keys[1], () => SerializeTable(list, fieldSize, elementWriter));
            return new SerializeDummyTableResult(blocks, keys);
        }

        protected static string[] AddDummyResults(SerializeDummyTableResult result, LazyBlockCollection blocks, List<List<string>> contiguousKeys)
        {
            blocks.AddRange(result.Blocks);
            contiguousKeys.Add(new List<string>(result.Keys));
            return result.Keys;
        }

        public static string[] GetOffsetAndDataKeys(string key)
        {
            return new string[] { $"{key}.OffsetTable", $"{key}.Data" };
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

        protected class SerializeDummyTableResult
        {
            public LazyBlockCollection Blocks { get; private set; }
            public string[] Keys { get; private set; }

            public SerializeDummyTableResult(LazyBlockCollection blocks, string[] keys)
            {
                Blocks = blocks;
                Keys = keys;
            }
        }

        #endregion

        public override string ToString() => Name;

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

        public virtual IEnumerable<string> GetBlockKeysForFile(FileSystemPath path)
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
