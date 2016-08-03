using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3.Text
{
    public abstract class StringCodec
    {
        protected Mother3RomConfig RomConfig { get; }
        protected Dictionary<short, ContextString> CharLookup => RomConfig.CharLookup;
        protected Dictionary<short, ContextString> SaturnLookup => RomConfig.SaturnLookup;
        protected List<ControlCode> ControlCodes => RomConfig.ControlCodes;

        protected StringCodec(Mother3RomConfig romConfig)
        {
            RomConfig = romConfig;
        }

        public abstract string ReadScriptString(BinaryStream stream);
        public abstract string ReadCodedString(BinaryStream stream);
        public abstract string ReadCodedString(BinaryStream stream, int maxLength);

        public static StringCodec Create(Mother3RomConfig romConfig)
        {
            if (romConfig.IsJapanese)
                return new JapaneseStringCodec(romConfig);
            else
                return null;
        }

        protected virtual ControlCode ProcessChar(BinaryStream stream, StringBuilder sb,
            bool isSaturn, ref int count, out ContextString contextString)
        {
            contextString = null;
            int chPosition = stream.Position;
            short ch = stream.ReadShort();
            count++;

            ControlCode code = ControlCodes.FirstOrDefault(cc => cc.Code == ch);
            var charLookup = isSaturn ? SaturnLookup : CharLookup;

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
    }
}
