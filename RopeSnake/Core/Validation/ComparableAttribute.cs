using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace RopeSnake.Core.Validation
{
    public abstract class ComparableAttribute : ValidateRuleBaseAttribute
    {
        protected Type CompareType { get; private set; }

        protected ComparableAttribute(Type compareType)
        {
            CompareType = compareType;
        }

        protected abstract bool Compare(IComparable value);
        protected abstract string GetFailMessage(object value);

        protected override bool ValidateInternal(object value, LazyString path, Logger log)
        {
            var comparable = Convert.ChangeType(value, CompareType) as IComparable;
            if (comparable == null)
            {
                throw new Exception("Value is not comparable");
            }

            if (!Compare(comparable))
            {
                return Fail(GetFailMessage(value), path, log);
            }

            return true;
        }
    }
}
