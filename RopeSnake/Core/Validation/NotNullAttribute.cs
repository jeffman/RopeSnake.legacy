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
        public override NullHandling NullHandling { get; set; } = NullHandling.Check;

        public override bool Validate(object value, LazyString path, Logger log)
        {
            if (NullHandling != NullHandling.Check)
                throw new Exception("What are you doing");

            return base.Validate(value, path, log);
        }

        protected override bool ValidateInternal(object value, LazyString path, Logger log)
        {
            if (value == null)
            {
                return Fail("Value was null", path, log);
            }
            return true;
        }
    }
}
