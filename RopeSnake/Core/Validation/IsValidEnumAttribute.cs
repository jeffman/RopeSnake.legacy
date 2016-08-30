using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace RopeSnake.Core.Validation
{
    public class IsValidEnumAttribute : ValidateRuleBaseAttribute
    {
        private Type _enumType;

        public IsValidEnumAttribute(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException(nameof(enumType));

            _enumType = enumType;
        }

        protected override bool ValidateInternal(object value, LazyString path, Logger log)
        {
            if (!Enum.IsDefined(_enumType, value))
            {
                return Fail($"Value of {value} was not a valid enum of type {_enumType.Name}", path, log);
            }
            return true;
        }
    }
}
