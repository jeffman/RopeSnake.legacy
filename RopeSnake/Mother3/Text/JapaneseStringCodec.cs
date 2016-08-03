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

        private string ReadCodedStringInternal(BinaryStream stream, int? maxLength)
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;
            bool saturnMode = false;

            for (count = 0; (!maxLength.HasValue) ||
                (maxLength.HasValue && count < maxLength);)
            {
                ContextString contextString;
                ControlCode code = ProcessChar(stream, sb, saturnMode, ref count, out contextString);

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
    }
}
