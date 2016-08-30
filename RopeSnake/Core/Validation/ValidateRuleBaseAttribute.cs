using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace RopeSnake.Core.Validation
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public abstract class ValidateRuleBaseAttribute : ValidateBaseAttribute
    {
        public virtual NullHandling NullHandling { get; set; } = NullHandling.Pass;
        public override ValidateFlags Flags { get; set; } = ValidateFlags.Instance;
        public bool Warn { get; set; } = false;

        public virtual bool Validate(object value, LazyString path, Logger log)
        {
            if (value == null && NullHandling == NullHandling.Pass)
                return true;
            return ValidateInternal(value, path, log);
        }

        protected abstract bool ValidateInternal(object value, LazyString path, Logger log);

        protected bool Fail(string message, LazyString path, Logger log)
        {
            if (Warn)
            {
                log?.Warn($"{message}: {path.Evaluate()}");
                return true;
            }
            else
            {
                log?.Error($"{message}: {path.Evaluate()}");
                return false;
            }
        }
    }

    public enum NullHandling
    {
        Pass,
        Check
    }
}
