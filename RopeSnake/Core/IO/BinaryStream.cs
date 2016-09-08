using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public class BinaryStream
    {
        public Stream BaseStream { get; }

        public virtual int Position
        {
            get
            {
                return (int)BaseStream.Position;
            }

            set
            {
                BaseStream.Seek(value, SeekOrigin.Begin);
            }
        }

        public virtual bool IsLittleEndian { get; }

        private Func<ushort> ushortReader;
        private Action<ushort> ushortWriter;
        private Func<int> intReader;
        private Action<int> intWriter;

        public BinaryStream(Stream stream) : this(stream, true) { }

        public BinaryStream(Stream stream, bool littleEndian)
        {
            BaseStream = stream;
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

        public virtual byte ReadByte()
        {
            int value = BaseStream.ReadByte();
            if (value == -1)
            {
                throw new IOException("Could not read from base stream");
            }

            return (byte)value;
        }

        public virtual void WriteByte(byte value) => BaseStream.WriteByte(value);
        public virtual byte PeekByte()
        {
            byte value = ReadByte();
            Position--;
            return value;
        }

        public virtual sbyte ReadSByte() => (sbyte)ReadByte();
        public virtual void WriteSByte(sbyte value) => WriteByte((byte)value);
        public virtual sbyte PeekSByte() => (sbyte)PeekByte();

        public virtual ushort ReadUShort() => ushortReader();
        public virtual void WriteUShort(ushort value) => ushortWriter(value);
        public virtual ushort PeekUShort()
        {
            ushort value = ReadUShort();
            Position -= 2;
            return value;
        }

        public virtual short ReadShort() => (short)ReadUShort();
        public virtual void WriteShort(short value) => WriteUShort((ushort)value);
        public virtual short PeekShort() => (short)PeekUShort();

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

        public virtual bool ReadBool() => ReadByte() != 0;
        public virtual void WriteBool(bool value) => WriteByte((byte)(value ? 1 : 0));

        public virtual void ReadBytes(byte[] dest, int destOffset, int count) => BaseStream.Read(dest, destOffset, count);
        public virtual void WriteBytes(byte[] source, int sourceOffset, int count) => BaseStream.Write(source, sourceOffset, count);

        public virtual string ReadString()
        {
            var builder = new StringBuilder();

            byte ch;
            while ((ch = ReadByte()) != 0)
            {
                builder.Append((char)ch);
            }

            return builder.ToString();
        }

        public virtual string ReadString(int length)
        {
            if (length < 1)
                throw new ArgumentException(nameof(length));

            var builder = new StringBuilder(length, length);

            byte ch;
            int bytesRead = 0;

            for (;;)
            {
                ch = ReadByte();
                bytesRead++;

                if (ch == 0 || bytesRead == length)
                {
                    break;
                }

                builder.Append((char)ch);
            }

            // Read out the remaining bytes: exactly length bytes should be read by this method
            for (int i = bytesRead; i <= length; i++)
            {
                ReadByte();
            }

            return builder.ToString();
        }

        public virtual void WriteString(string str)
        {
            foreach (char c in str)
            {
                WriteByte((byte)c);
            }
            WriteByte(0);
        }

        public virtual void WriteString(string str, int length)
        {
            if (str.Length > length)
                throw new ArgumentException(nameof(str));

            for (int i = 0; i < length; i++)
            {
                if (i < str.Length)
                {
                    WriteByte((byte)str[i]);
                }
                else
                {
                    WriteByte(0);
                }
            }
        }
    }
}
