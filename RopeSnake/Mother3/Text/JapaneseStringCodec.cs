using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3.Text
{
    public sealed class JapaneseStringCodec : StringCodec
    {
        public JapaneseStringCodec(Mother3RomConfig romConfig)
            : base(romConfig)
        {
        }

        public override string ReadCodedString(BinaryStream stream)
            => ReadCodedStringInternal(stream, null);

        public override string ReadCodedString(BinaryStream stream, int maxLength)
            => ReadCodedStringInternal(stream, maxLength);

        public override string ReadScriptString(BinaryStream stream)
            => ReadCodedString(stream);

        public override void WriteCodedString(BinaryStream stream, string str)
            => WriteCodedStringInternal(stream, null, str);

        public override void WriteCodedString(BinaryStream stream, int maxLength, string str)
            => WriteCodedStringInternal(stream, maxLength, str);

        public override void WriteScriptString(BinaryStream stream, string str)
            => WriteCodedString(stream, str);

        private string ReadCodedStringInternal(BinaryStream stream, int? maxLength)
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;
            bool saturnMode = false;

            for (count = 0; (!maxLength.HasValue) ||
                (maxLength.HasValue && count < maxLength);)
            {
                ContextString contextString;
                ControlCode code = ReadChar(stream, sb, saturnMode, ref count, out contextString);

                if (code != null)
                {
                    if (code.Flags.HasFlag(ControlCodeFlags.AlternateFont))
                    {
                        saturnMode = !saturnMode;
                    }

                    if (code.Flags.HasFlag(ControlCodeFlags.Terminate))
                    {
                        break;
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
            Dictionary<short, ContextString> currentLookup = CharLookup;
            ReverseLookup currentReverseLookup = ReverseCharLookup;
            bool saturnMode = false;

            while (currentIndex < str.Length)
            {
                ControlCode code = WriteChar(stream, str, Context.None, ref currentWrittenLength, maxLength,
                    ref currentIndex, currentReverseLookup);

                if (code != null)
                {
                    if (code.Flags.HasFlag(ControlCodeFlags.Terminate))
                    {
                        break;
                    }

                    if (code.Flags.HasFlag(ControlCodeFlags.AlternateFont))
                    {
                        saturnMode = !saturnMode;

                        currentLookup = saturnMode ? SaturnLookup : CharLookup;
                        currentReverseLookup = saturnMode ? ReverseSaturnLookup : ReverseCharLookup;
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
    }
}
