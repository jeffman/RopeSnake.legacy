using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RopeSnake.Mother3.Text
{
    public sealed class ControlCode
    {
        public short Code { get; set; }
        public int Arguments { get; set; }
        public string Description { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Tag { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public ControlCodeFlags Flags { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('[');

            if (Tag != null)
            {
                sb.Append(Tag);
            }
            else
            {
                sb.Append(((ushort)Code).ToString("X4"));
            }

            for (int i = 0; i < Arguments; i++)
            {
                sb.Append(" xxxx");
            }

            sb.Append(']');

            return sb.ToString();
        }
    }

    [Flags]
    public enum ControlCodeFlags
    {
        None = 0,
        Terminate = 1,
        AlternateFont = 2
    }
}
