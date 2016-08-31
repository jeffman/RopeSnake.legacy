using RopeSnake.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Mother3.Text
{
    public sealed class EnglishStringStreamReader : BinaryStream
    {
        private Queue<short> _buffer = new Queue<short>();
        private ScriptEncodingParameters _parameters;

        private bool AtStart { get; set; } = true;

        public EnglishStringStreamReader(Stream stream, ScriptEncodingParameters parameters) : base(stream)
        {
            _parameters = parameters;
        }

        internal byte DecodeByte()
        {
            int pos = Position + 0x8000000;
            byte raw = ReadByte();

            if (_parameters == null)
            {
                return raw;
            }

            bool even = (pos & 1) == 0;

            if (even)
            {
                int offset = ((pos >> 1) % _parameters.EvenPadModulus);
                byte pad = _parameters.EvenPad[offset];
                return (byte)(((raw + _parameters.EvenOffset1) ^ pad) + _parameters.EvenOffset2);
            }
            else
            {
                int offset = ((pos >> 1) % _parameters.OddPadModulus);
                byte pad = _parameters.OddPad[offset];
                return (byte)(((raw + _parameters.OddOffset1) ^ pad) + _parameters.OddOffset2);
            }
        }

        internal short DecodeShort()
        {
            byte lower = DecodeByte();
            byte upper = DecodeByte();

            return (short)(lower | (upper << 8));
        }

        internal short ReadShort(bool peek)
        {
            if (_buffer.Count > 0)
            {
                if (peek)
                {
                    return _buffer.Peek();
                }
                else
                {
                    return _buffer.Dequeue();
                }
            }

            byte value = DecodeByte();
            bool first = AtStart;

            if (!peek)
            {
                AtStart = false;
            }

            if (value == 0xEF)
            {
                // Custom article code
                return (short)(0xEF00 | DecodeByte());
            }

            else if (first && value == 0xFE)
            {
                // Custom hotspot code
                if (!peek)
                {
                    _buffer.Enqueue(DecodeShort());
                }

                return -257; // 0xFEFF
            }

            // Check for other control codes
            else if (value >= 0xF0)
            {
                if (value == 0xFF)
                {
                    return -1; // FFFF
                }

                else
                {
                    int length = (value & 0xF);
                    byte code = DecodeByte();

                    if (!peek)
                    {
                        for (int i = 0; i < length; i++)
                        {
                            _buffer.Enqueue(DecodeShort());
                        }
                    }

                    return (short)(0xFF00 | code);
                }
            }

            else
            {
                return value;
            }
        }

        public override short ReadShort() => ReadShort(false);
        public override ushort ReadUShort() => (ushort)ReadShort();

        public override short PeekShort()
        {
            int pos = Position;
            short value = ReadShort(true);
            Position = pos;
            return value;
        }
        public override ushort PeekUShort() => (ushort)PeekShort();
    }
}
