using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using RopeSnake.Mother3.IO;

namespace RopeSnake.Mother3.Text
{
    public static class TextStreamExtensions
    {
        public static StringTable ReadStringTable(this BinaryStream stream, StringCodec codec)
        {
            ushort maxLength = stream.ReadUShort();
            int count = stream.ReadUShort();

            var table = new StringTable { MaxLength = maxLength };
            for (int i = 0; i < count; i++)
            {
                table.Add(codec.ReadCodedString(stream, maxLength));
            }

            return table;
        }

        public static StringTable ReadStringTable(this WideOffsetTableReader offsetTableReader, StringCodec codec)
        {
            offsetTableReader.Next();
            return offsetTableReader.Stream.ReadStringTable(codec);
        }

        public static List<string> ReadStringOffsetTable(this BinaryStream stream, StringCodec codec,
            bool isScript, bool multiplyByTwo, int dataPointer)
        {
            var strings = new List<string>();

            var offsetReader = new ShortOffsetTableReader(stream, dataPointer, multiplyByTwo);
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

        public static List<string> ReadStringOffsetTable(this WideOffsetTableReader offsetTableReader, StringCodec codec,
            bool isScript, bool multiplyByTwo)
        {
            int offsetPointer = offsetTableReader.NextPointer();
            int dataPointer = offsetTableReader.NextPointer();
            var stream = offsetTableReader.Stream;

            if (offsetPointer != 0 && dataPointer != 0)
            {
                stream.Position = offsetPointer;
                return stream.ReadStringOffsetTable(codec, isScript, multiplyByTwo, dataPointer);
            }
            else
            {
                return null;
            }
        }
    }
}
