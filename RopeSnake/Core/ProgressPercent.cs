using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public struct ProgressPercent
    {
        public readonly string Message;
        public readonly float Percent;

        public ProgressPercent(string message, float percent)
        {
            Message = message;
            Percent = percent;
        }
    }
}
