using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using System.Globalization;

namespace RopeSnake.Mother3.Text
{
    public sealed class EnglishStringCodec : StringCodec
    {
        private Block _writeStringBuffer = new Block(64 * 1024);
        private BinaryStream _writeStringBufferReader;
        private BinaryStream _writeStringBufferWriter;

        public EnglishStringCodec(Mother3RomConfig romConfig)
            : base(romConfig)
        {
            _writeStringBufferReader = _writeStringBuffer.ToBinaryStream();
            _writeStringBufferWriter = _writeStringBuffer.ToBinaryStream();
        }

        public override string ReadCodedString(BinaryStream stream)
            => ReadCodedStringInternal(stream, null);

        public override string ReadCodedString(BinaryStream stream, int maxLength)
            => ReadCodedStringInternal(stream, maxLength);

        public override string ReadScriptString(BinaryStream stream)
        {
            var reader = new EnglishStringStreamReader(stream.BaseStream, RomConfig.ScriptEncodingParameters);
            return ReadCodedString(reader);
        }

        public override void WriteCodedString(BinaryStream stream, string str)
            => WriteCodedStringInternal(stream, null, str);

        public override void WriteCodedString(BinaryStream stream, int maxLength, string str)
            => WriteCodedStringInternal(stream, maxLength, str);

        public override void WriteScriptString(BinaryStream stream, string str)
        {
            _writeStringBufferWriter.Position = 0;
            WriteCodedString(_writeStringBufferWriter, str);

            _writeStringBufferReader.Position = 0;

            while (_writeStringBufferReader.Position < _writeStringBufferWriter.Position)
            {
                short ch = _writeStringBufferReader.ReadShort();

                ControlCode code = ControlCodes.FirstOrDefault(c => c.Code == ch);

                if (code != null)
                {
                    if (code.Code == -257)
                    {
                        // Hotsprings code is coded specially
                        stream.WriteByte(0xFE);
                        stream.WriteShort(_writeStringBufferReader.ReadShort());
                    }
                    else if (code.Code == -1)
                    {
                        // End code is coded specially
                        stream.WriteByte(0xFF);
                    }
                    else
                    {
                        byte compact = (byte)(0xF0 | code.Arguments);
                        stream.WriteByte(compact);
                        stream.WriteByte((byte)(ch & 0xFF));

                        for (int i = 0; i < code.Arguments; i++)
                        {
                            stream.WriteShort(_writeStringBufferReader.ReadShort());
                        }
                    }
                }
                else
                {
                    stream.WriteByte((byte)(ch & 0xFF));
                }
            }
        }

        private string ReadCodedStringInternal(BinaryStream stream, int? maxLength)
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;
            bool saturnMode = false;

            for (count = 0; (!maxLength.HasValue) ||
                (maxLength.HasValue && count < maxLength);)
            {
                // Check for custom codes
                ushort peek = stream.PeekUShort();
                byte upper = (byte)((peek >> 8) & 0xFF);

                if (upper == 0xEF)
                {
                    // Custom code with one-byte argument inline
                    byte arg = (byte)(peek & 0xFF);
                    stream.ReadShort();
                    count++;

                    sb.Append($"[EF{arg:X2}]");
                }

                else
                {
                    ContextString contextString;
                    int currentIndex = sb.Length;
                    ControlCode code = ReadChar(stream, sb, false, ref count, out contextString);

                    if (code != null && code.Flags.HasFlag(ControlCodeFlags.Terminate))
                    {
                        break;
                    }

                    // Check for Saturn/normal mode switch
                    if (contextString != null)
                    {
                        if (saturnMode && contextString.Context == Context.Normal)
                        {
                            saturnMode = false;
                            sb.Insert(currentIndex, "[NORMALMODE]");
                        }
                        else if (!saturnMode && contextString.Context == Context.Saturn)
                        {
                            saturnMode = true;
                            sb.Insert(currentIndex, "[SATURNMODE]");
                        }
                    }
                }
            }

            // Advance the position past the end of the string if applicable
            while (maxLength.HasValue && count++ < maxLength)
            {
                stream.ReadShort();
            }

            return sb.ToString();
        }

        private void WriteCodedStringInternal(BinaryStream stream, int? maxLength, string str)
        {
            int currentIndex = 0;
            int currentWrittenLength = 0;
            Context context = Context.Normal;

            while (currentIndex < str.Length)
            {
                // Check for context switches
                if (ContainsAt(str, currentIndex, "[SATURNMODE]"))
                {
                    context = Context.Saturn;
                    currentIndex += 12;
                }
                else if (ContainsAt(str, currentIndex, "[NORMALMODE]"))
                {
                    context = Context.Normal;
                    currentIndex += 12;
                }
                else
                {
                    // Check for custom codes
                    bool foundCustomCode = false;

                    if (str[currentIndex] == '[')
                    {
                        int closingIndex = str.IndexOf(']', currentIndex + 1);
                        int openingIndex = str.IndexOf('[', currentIndex + 1);

                        if ((closingIndex < 0) || (openingIndex >= 0 && openingIndex < closingIndex))
                        {
                            throw new Exception("Opening bracket without subsequent closing bracket");
                        }

                        string[] tokens = str.Substring(currentIndex + 1, closingIndex - currentIndex - 1).Split(' ');
                        short hexCode;

                        if (tokens.Length == 1 && short.TryParse(tokens[0], NumberStyles.HexNumber, null, out hexCode))
                        {
                            // Custom [EFxx] code
                            if ((hexCode & 0xFF00) == 0xEF00)
                            {
                                // Write
                                if (maxLength.HasValue && currentWrittenLength >= maxLength)
                                {
                                    throw new Exception("Max length exceeded");
                                }

                                stream.WriteShort(hexCode);

                                foundCustomCode = true;
                                currentIndex = closingIndex + 1;
                                currentWrittenLength++;
                            }
                        }
                    }

                    if (!foundCustomCode)
                    {
                        ControlCode code = WriteChar(stream, str, context, ref currentWrittenLength,
                            maxLength, ref currentIndex, ReverseCharLookup);

                        if (code != null && code.Flags.HasFlag(ControlCodeFlags.Terminate))
                        {
                            break;
                        }
                    }
                }
            }

            if (maxLength == null)
            {
                // Write terminating character
                stream.WriteShort(-1);
            }
            else
            {
                // Fill the remaining characters will nullspace
                for (int i = currentWrittenLength; i < maxLength.Value; i++)
                {
                    stream.WriteShort(-1);
                }
            }
        }

        private static bool ContainsAt(string str, int startingIndex, string check)
        {
            if (str.Length - startingIndex < check.Length)
            {
                return false;
            }

            for (int i = 0; i < check.Length; i++)
            {
                if (str[startingIndex++] != check[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static void EncodeBlock(Block romData, int start, int size, ScriptEncodingParameters encodingParameters)
        {
            if (encodingParameters == null)
            {
                return;
            }

            int romPos = start | 0x8000000;
            bool even = (romPos & 1) == 0;
            byte value;

            for (int i = 0; i < size; i++)
            {
                value = romData[start];

                if (even)
                {
                    int delta = ((romPos >> 1) % encodingParameters.EvenPadModulus);
                    byte pad = encodingParameters.EvenPad[delta];
                    byte encoded = (byte)(((value - encodingParameters.EvenOffset2) ^ pad) - encodingParameters.EvenOffset1);
                    romData[start] = encoded;
                }
                else
                {
                    int delta = ((romPos >> 1) % encodingParameters.OddPadModulus);
                    byte pad = encodingParameters.OddPad[delta];
                    byte encoded = (byte)(((value - encodingParameters.OddOffset2) ^ pad) - encodingParameters.OddOffset1);
                    romData[start] = encoded;
                }

                romPos++;
                start++;
                even = !even;
            }
        }
    }
}
