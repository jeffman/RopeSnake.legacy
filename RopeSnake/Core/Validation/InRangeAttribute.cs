using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core.Validation
{
    public class InRangeAttribute : ComparableAttribute
    {
        private object _lower;
        private object _upper;

        public InRangeAttribute(object lowerInclusive, object upperInclusive)
            : base(GetCompareType(lowerInclusive, upperInclusive))
        {
            _lower = lowerInclusive;
            _upper = upperInclusive;
        }

        private static Type GetCompareType(params object[] values)
        {
            if (values == null || values.Length == 0 || values.Any(v => v == null))
                throw new ArgumentException(nameof(values));

            var firstType = values[0].GetType();

            if (values.Skip(1).Any(v => v.GetType() != firstType))
                throw new ArgumentException(nameof(values));

            return firstType;
        }

        protected override bool Compare(IComparable value)
        {
            return value.CompareTo(_lower) >= 0 && value.CompareTo(_upper) <= 0;
        }

        protected override string GetFailMessage(object value)
        {
            return $"Value of {value} was not in range [{_lower}, {_upper}]";
        }
    }
}
