using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace RopeSnake.Core.Validation
{
    public class NotNullAttribute : ValidateRuleBaseAttribute
    {
        public override bool Validate(object value, LazyString path, Logger log)
        {
            if (value == null)
            {
                return Fail("Value was null", path, log);
            }
            return true;
        }
    }
}
