using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using System.Globalization;

namespace RopeSnake.Mother3.Text
{
    public abstract class StringCodec
    {
        protected Mother3RomConfig RomConfig { get; }
        protected Dictionary<short, ContextString> CharLookup => RomConfig.CharLookup;
        protected Dictionary<short, ContextString> SaturnLookup => RomConfig.SaturnLookup;
        protected ReverseLookup ReverseCharLookup => RomConfig.ReverseCharLookup;
        protected ReverseLookup ReverseSaturnLookup => RomConfig.ReverseSaturnLookup;
        protected List<ControlCode> ControlCodes => RomConfig.ControlCodes;

        protected StringCodec(Mother3RomConfig romConfig)
        {
            RomConfig = romConfig;
        }

        public abstract string ReadScriptString(BinaryStream stream);
        public abstract string ReadCodedString(BinaryStream stream);
        public abstract string ReadCodedString(BinaryStream stream, int maxLength);

        public abstract void WriteScriptString(BinaryStream stream, string str);
        public abstract void WriteCodedString(BinaryStream stream, string str);
        public abstract void WriteCodedString(BinaryStream stream, int maxLength, string str);

        public static StringCodec Create(Mother3RomConfig romConfig)
        {
            if (romConfig.IsJapanese)
                return new JapaneseStringCodec(romConfig);
            else
                return new EnglishStringCodec(romConfig);
        }

        protected ControlCode ReadChar(BinaryStream stream, StringBuilder sb,
            bool isSaturn, ref int count, out ContextString contextString)
        {
            contextString = null;
            int chPosition = stream.Position;
            short ch = stream.ReadShort();
            count++;

            ControlCode code = ControlCodes.FirstOrDefault(cc => cc.Code == ch);

            if (code != null)
            {
                if (code.Code != -1)
                {
                    sb.Append('[');

                    if (code.Tag != null)
                    {
                        sb.Append(code.Tag);
                    }
                    else
                    {
                        sb.Append(((ushort)ch).ToString("X4"));
                    }

                    for (int i = 0; i < code.Arguments; i++)
                    {
                        ch = stream.ReadShort();
                        count++;

                        sb.Append(' ');
                        sb.Append(((ushort)ch).ToString("X4"));
                    }

                    sb.Append(']');
                }
            }
            else
            {
                var charLookup = isSaturn ? SaturnLookup : CharLookup;

                if (charLookup.ContainsKey(ch))
                {
                    contextString = charLookup[ch];
                    sb.Append(contextString.String);
                }
                else
                {
                    sb.Append('[');
                    sb.Append(((ushort)ch).ToString("X4"));
                    sb.Append(']');

                    //logger.Debug($"Unrecognized character: [{ch:X4}] at 0x{chPosition:X}");
                }
            }

            return code;
        }

        protected ControlCode WriteChar(BinaryStream stream, string str, Context context,
            ref int currentWrittenLength, int? maxLength, ref int startingIndex,
            ReverseLookup reverseCharLookup)
        {
            // Check for opening bracket
            if (str[startingIndex] == '[')
            {
                // Look for the closing bracket
                int closingIndex = str.IndexOf(']', startingIndex + 1);

                // Look for invalid opening bracket
                int openingIndex = str.IndexOf('[', startingIndex + 1);

                if ((closingIndex < 0) || (openingIndex >= 0 && openingIndex < closingIndex))
                {
                    throw new Exception("Opening bracket without subsequent closing bracket");
                }

                // Get the tokens inside the brackets
                string[] tokens = str.Substring(startingIndex + 1, closingIndex - startingIndex - 1).Split(' ');

                short hexCode = 0;
                bool validHexCode = false;
                bool didParseHexCode = false;

                if (tokens.Length == 0)
                {
                    throw new Exception("Empty brackets");
                }

                // Check for a matching control code by tag
                ControlCode code = ControlCodes.FirstOrDefault(c => c.Tag == tokens[0]);

                if (code == null)
                {
                    // Check for a matching control code by value
                    validHexCode = short.TryParse(tokens[0], NumberStyles.HexNumber, null, out hexCode);
                    didParseHexCode = true;

                    if (validHexCode)
                    {
                        code = ControlCodes.FirstOrDefault(c => c.Code == hexCode);
                    }
                }

                if (code != null)
                {
                    // Found a control code; parse it

                    // Check for correct number of arguments
                    if (tokens.Length != (code.Arguments + 1))
                    {
                        throw new Exception("Wrong number of arguments");
                    }

                    // Parse the arguments as hex
                    short[] args = new short[code.Arguments];

                    for (int i = 0; i < code.Arguments; i++)
                    {
                        short arg;

                        if (!short.TryParse(tokens[i + 1], NumberStyles.HexNumber, null, out arg))
                        {
                            throw new Exception("Could not parse argument as a hex number");
                        }

                        args[i] = arg;
                    }

                    // Write
                    if (maxLength.HasValue && (currentWrittenLength + code.Arguments + 1) > maxLength)
                    {
                        throw new Exception("Max length exceeded");
                    }

                    stream.WriteShort(code.Code);

                    for (int i = 0; i < code.Arguments; i++)
                    {
                        stream.WriteShort(args[i]);
                    }

                    startingIndex = closingIndex + 1;
                    currentWrittenLength += code.Arguments + 1;

                    return code;
                }

                else
                {
                    bool foundHex = didParseHexCode;

                    // Treat the whole thing as hex
                    if (!didParseHexCode)
                    {
                        validHexCode = short.TryParse(tokens[0], NumberStyles.HexNumber, null, out hexCode);

                        if (validHexCode)
                        {
                            foundHex = true;
                        }
                    }

                    if (foundHex && validHexCode)
                    {
                        short[] args = new short[tokens.Length - 1];

                        for (int i = 0; i < args.Length; i++)
                        {
                            short arg;

                            if (!short.TryParse(tokens[i + 1], NumberStyles.HexNumber, null, out arg))
                            {
                                foundHex = false;
                                break;
                            }

                            args[i] = arg;
                        }

                        if (foundHex)
                        {
                            // Write
                            if (maxLength.HasValue && (currentWrittenLength + args.Length + 1) > maxLength)
                            {
                                throw new Exception("Max length exceeded");
                            }

                            stream.WriteShort(hexCode);

                            for (int i = 0; i < args.Length; i++)
                            {
                                stream.WriteShort(args[i]);
                            }

                            startingIndex = closingIndex + 1;
                            currentWrittenLength += args.Length + 1;

                            return null;
                        }
                    }
                }
            }

            // Find a string match
            int matchedLength;
            short? stringMatch = reverseCharLookup.Find(str, startingIndex, context, out matchedLength);

            if (stringMatch == null)
            {
                throw new Exception("Could not parse character");
            }

            // Write
            if (maxLength.HasValue && currentWrittenLength >= maxLength)
            {
                throw new Exception("Max length exceeded");
            }

            stream.WriteShort(stringMatch.Value);

            startingIndex += matchedLength;
            currentWrittenLength++;

            return null;
        }
    }
}
