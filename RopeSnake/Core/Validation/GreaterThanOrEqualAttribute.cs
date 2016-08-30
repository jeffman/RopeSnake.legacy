using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core.Validation
{
    public class GreaterThanOrEqualAttribute : ComparableAttribute
    {
        private object _compareTo;

        public GreaterThanOrEqualAttribute(object compareTo) : base(compareTo.GetType())
        {
            _compareTo = compareTo;
        }

        protected override bool Compare(IComparable value)
        {
            return value.CompareTo(_compareTo) >= 0;
        }

        protected override string GetFailMessage(object value)
        {
            return $"Value of {value} was less than {_compareTo}";
        }
    }
}
