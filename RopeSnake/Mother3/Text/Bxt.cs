using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using RopeSnake.Mother3.IO;
using NLog;
using Newtonsoft.Json;
using System.Collections;

namespace RopeSnake.Mother3.Text
{
    [JsonObject]
    public sealed class Bxt : IList<string>
    {
        [JsonProperty(Order = 99)]
        public int Unknown { get; set; }

        [JsonProperty]
        public List<string> Strings { get; private set; } = new List<string>();

        #region IList implementation

        public string this[int index]
        {
            get
            {
                return Strings[index];
            }

            set
            {
                Strings[index] = value;
            }
        }

        [JsonIgnore]
        public int Count
        {
            get
            {
                return Strings.Count;
            }
        }

        [JsonIgnore]
        public bool IsReadOnly => false;

        public void Add(string item)
        {
            Strings.Add(item);
        }

        public void Clear()
        {
            Strings.Clear();
        }

        public bool Contains(string item)
        {
            return Strings.Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            Strings.CopyTo(array, arrayIndex);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return Strings.GetEnumerator();
        }

        public int IndexOf(string item)
        {
            return Strings.IndexOf(item);
        }

        public void Insert(int index, string item)
        {
            Strings.Insert(index, item);
        }

        public bool Remove(string item)
        {
            return Strings.Remove(item);
        }

        public void RemoveAt(int index)
        {
            Strings.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Strings.GetEnumerator();
        }

        #endregion
    }

    public sealed class BxtOffsetTableReader : OffsetTableReader
    {
        public int Unknown { get; }

        public BxtOffsetTableReader(BinaryStream stream, bool multiplyByTwo)
            : base(stream)
        {
            int basePosition = stream.Position;

            string header = stream.ReadString(4);
            if (header != "bxt ")
                throw new Exception($"Expected bxt header but got {header} at 0x{basePosition:X}");

            Unknown = stream.ReadInt();
            int count = stream.ReadInt();

            Pointers = new int[count];
            for (int i = 0; i < count; i++)
            {
                int offset = stream.ReadUShort();
                if (multiplyByTwo)
                {
                    offset *= 2;
                }

                if (offset == 0)
                {
                    Pointers[i] = 0;
                }
                else
                {
                    Pointers[i] = basePosition + offset;
                }
            }
        }
    }

    public sealed class BxtOffsetTableWriter : OffsetTableWriter
    {
        private BinaryStream _stream;
        private int _basePosition;
        private int _remaining;
        private bool _divideByTwo;

        public BxtOffsetTableWriter(BinaryStream stream, int count, int unknown, bool divideByTwo)
        {
            _stream = stream;
            _basePosition = stream.Position;
            _remaining = count;
            _divideByTwo = divideByTwo;

            _stream.WriteString("bxt ", 4);
            _stream.WriteInt(unknown);
            _stream.WriteInt(count);
        }

        public override void AddNull()
            => AddOffset(0);

        public override void AddPointer(int pointer)
            => AddOffset(pointer - _basePosition);

        private void AddOffset(int offset)
        {
            if (_remaining <= 0)
                throw new InvalidOperationException("Exceeded length of table");

            if (offset < 0)
                throw new ArgumentException(nameof(offset));

            if (_divideByTwo)
                offset /= 2;

            if (offset > 0xFFFF)
                throw new InvalidOperationException($"Offset out of range: 0x{offset:X}");

            _stream.WriteUShort((ushort)offset);
        }

        public void Finish()
        {
            if (_remaining != 0)
                throw new InvalidOperationException($"Tried finishing a table with {_remaining} offsets still remaining to be written");

            _stream.WriteString("~bxt", 4);
        }
    }
}
