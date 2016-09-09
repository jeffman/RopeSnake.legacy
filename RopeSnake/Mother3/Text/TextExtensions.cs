using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using RopeSnake.Mother3.IO;
using NLog;

namespace RopeSnake.Mother3.Text
{
    public static class TextExtensions
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private static readonly int StringOffsetTableBufferSize = 256 * 1024;

        public static StringTable ReadStringTable(this BinaryStream stream, StringCodec codec)
        {
            _log.Debug($"Reading string table at 0x{stream.Position:X}");

            ushort maxLength = stream.ReadUShort();
            int count = stream.ReadUShort();

            var table = new StringTable { MaxLength = maxLength };
            for (int i = 0; i < count; i++)
            {
                table.Add(codec.ReadCodedString(stream, maxLength));
            }

            return table;
        }

        public static void WriteStringTable(this BinaryStream stream, StringCodec codec, StringTable table)
        {
            if (table.Count > 0xFFFF)
                throw new InvalidOperationException($"Too many strings in table: {table.Count}");

            stream.WriteUShort(table.MaxLength);
            stream.WriteUShort((ushort)table.Count);

            foreach (string str in table)
            {
                codec.WriteCodedString(stream, table.MaxLength, str == null ? "" : str);
            }
        }

        public static Block SerializeStringTable(StringCodec codec, StringTable table)
        {
            var block = new Block(table.MaxLength * 2 * table.Count + 4);
            block.ToBinaryStream().WriteStringTable(codec, table);
            return block;
        }

        public static StringTable ReadStringTable(this WideOffsetTableReader offsetTableReader, StringCodec codec)
        {
            if (!offsetTableReader.Next())
                return null;

            return offsetTableReader.Stream.ReadStringTable(codec);
        }

        public static List<string> ReadStringOffsetTable(this BinaryStream stream, StringCodec codec,
            bool isScript, int dataPointer)
        {
            _log.Debug($"Reading string offset table at 0x{stream.Position:X} (data at 0x{dataPointer:X})");

            var strings = new List<string>();

            var offsetReader = new ShortOffsetTableReader(stream, dataPointer);
            while (!offsetReader.EndOfTable)
            {
                if (offsetReader.Next())
                {
                    strings.Add(isScript ? codec.ReadScriptString(stream) : codec.ReadCodedString(stream));
                }
                else
                {
                    strings.Add(null);
                }
            }

            return strings;
        }

        public static Block WriteStringShortOffsetTable(this BinaryStream stream, IEnumerable<string> strings,
            StringCodec codec, bool isScript)
        {
            int count = strings.Count();
            var cache = new Dictionary<string, int> { [""] = stream.Position };
            var offsetWriter = new ShortOffsetTableWriter(stream, count);
            var offsetTable = ShortOffsetTableWriter.CreateOffsetTable(count);

            stream.WriteStringOffsetTable(strings, offsetWriter, codec, isScript, cache);

            offsetWriter.UpdateOffsetTable(offsetTable);
            return offsetTable;
        }

        public static void WriteStringOffsetTable(this BinaryStream stream, IEnumerable<string> strings,
            OffsetTableWriter offsetWriter, StringCodec codec, bool isScript, Dictionary<string, int> cache = null)
        {
            if (cache == null)
            {
                cache = new Dictionary<string, int>();
            }

            foreach (string str in strings)
            {
                if (str == null)
                {
                    offsetWriter.AddNull();
                }
                else
                {
                    if (cache.ContainsKey(str))
                    {
                        offsetWriter.AddPointer(cache[str]);
                    }
                    else
                    {
                        cache.Add(str, stream.Position);
                        offsetWriter.AddPointer(stream.Position);

                        if (isScript)
                        {
                            codec.WriteScriptString(stream, str);
                        }
                        else
                        {
                            codec.WriteCodedString(stream, str);
                        }
                    }
                }
            }
        }

        public static Block[] SerializeStringOffsetTable(StringCodec codec,
            IEnumerable<string> strings, bool isScript)
        {
            if (strings == null)
                return new Block[] { null, null };

            var dataBlock = new Block(StringOffsetTableBufferSize);
            var dataStream = dataBlock.ToBinaryStream();
            var offsetTable = dataStream.WriteStringShortOffsetTable(strings, codec, isScript);
            dataBlock.Resize(dataStream.Position);
            return new Block[] { offsetTable, dataBlock };
        }

        public static List<string> ReadStringOffsetTable(this WideOffsetTableReader offsetTableReader, StringCodec codec,
            bool isScript)
        {
            int offsetPointer = offsetTableReader.NextPointer();
            int dataPointer = offsetTableReader.NextPointer();
            var stream = offsetTableReader.Stream;

            if (offsetPointer != 0 && dataPointer != 0)
            {
                stream.Position = offsetPointer;
                return stream.ReadStringOffsetTable(codec, isScript, dataPointer);
            }
            else
            {
                return null;
            }
        }

        public static void AddStringOffsetTableBlocks(this LazyBlockCollection blockCollection, string key,
            StringCodec codec, IEnumerable<string> strings, bool isScript)
        {
            string[] keys = Mother3Module.GetOffsetAndDataKeys(key);
            blockCollection.AddRange(keys, () => SerializeStringOffsetTable(codec, strings, isScript));
        }

        public static Bxt ReadBxt(this BinaryStream stream, StringCodec codec, bool multiplyByTwo)
        {
            var bxt = new Bxt();
            var offsetTableReader = new BxtOffsetTableReader(stream, multiplyByTwo);

            bxt.Unknown = offsetTableReader.Unknown;
            while (!offsetTableReader.EndOfTable)
            {
                if (offsetTableReader.Next())
                {
                    bxt.Add(codec.ReadCodedString(stream));
                }
                else
                {
                    bxt.Add(null);
                }
            }

            return bxt;
        }

        public static Block SerializeBxt(Bxt bxt, StringCodec codec, bool divideByTwo)
        {
            var block = new Block(StringOffsetTableBufferSize);
            var stream = block.ToBinaryStream();
            var offsetWriter = new BxtOffsetTableWriter(stream, bxt.Count, bxt.Unknown, divideByTwo);

            stream.WriteStringOffsetTable(bxt, offsetWriter, codec, false);

            offsetWriter.Finish();
            block.Resize(stream.Position);
            return block;
        }
    }
}
