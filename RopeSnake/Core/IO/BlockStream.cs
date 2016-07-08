using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public class BlockStream
    {
        private Block _block;

        public virtual int Position { get; set; }
        public virtual bool IsLittleEndian { get; }

        private Func<ushort> ushortReader;
        private Action<ushort> ushortWriter;
        private Func<int> intReader;
        private Action<int> intWriter;

        public BlockStream(Block block) : this(block, true) { }

        public BlockStream(Block block, bool littleEndian)
        {
            _block = block;
            IsLittleEndian = littleEndian;
            SetUpEndianness();
        }

        private void SetUpEndianness()
        {
            switch (IsLittleEndian)
            {
                case true:
                    ushortReader = ReadUShortLittleEndian;
                    ushortWriter = WriteUShortLittleEndian;
                    intReader = ReadIntLittleEndian;
                    intWriter = WriteIntLittleEndian;
                    break;

                case false:
                    ushortReader = ReadUShortBigEndian;
                    ushortWriter = WriteUShortBigEndian;
                    intReader = ReadIntBigEndian;
                    intWriter = WriteIntBigEndian;
                    break;
            }
        }

        public virtual byte ReadByte() => _block[Position++];
        public virtual void WriteByte(byte value) => _block[Position++] = value;

        public virtual sbyte ReadSByte() => (sbyte)ReadByte();
        public virtual void WriteSByte(sbyte value) => WriteByte((byte)value);

        public virtual ushort ReadUShort() => ushortReader();
        public virtual void WriteUShort(ushort value) => ushortWriter(value);

        public virtual short ReadShort() => (short)ReadUShort();
        public virtual void WriteShort(short value) => WriteUShort((ushort)value);

        public virtual uint ReadUInt() => (uint)ReadInt();
        public virtual void WriteUInt(uint value) => WriteInt((int)value);

        public virtual int ReadInt() => intReader();
        public virtual void WriteInt(int value) => intWriter(value);

        protected virtual ushort ReadUShortLittleEndian() => (ushort)(ReadByte() | (ReadByte() << 8));
        protected virtual ushort ReadUShortBigEndian() => (ushort)((ReadByte() << 8) | ReadByte());

        protected virtual void WriteUShortLittleEndian(ushort value)
        {
            WriteByte((byte)(value & 0xFF));
            WriteByte((byte)((value >> 8) & 0xFF));
        }

        protected virtual void WriteUShortBigEndian(ushort value)
        {
            WriteByte((byte)((value >> 8) & 0xFF));
            WriteByte((byte)(value & 0xFF));
        }

        protected virtual int ReadIntLittleEndian() => ReadUShortLittleEndian() | (ReadUShortLittleEndian() << 16);
        protected virtual int ReadIntBigEndian() => (ReadUShortBigEndian() << 16) | ReadUShortBigEndian();

        protected virtual void WriteIntLittleEndian(int value)
        {
            WriteUShortLittleEndian((ushort)(value & 0xFFFF));
            WriteUShortLittleEndian((ushort)((value >> 16) & 0xFFFF));
        }

        protected virtual void WriteIntBigEndian(int value)
        {
            WriteUShortBigEndian((ushort)((value >> 16) & 0xFFFF));
            WriteUShortBigEndian((ushort)(value & 0xFFFF));
        }
    }
}
