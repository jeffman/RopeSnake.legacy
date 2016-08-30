using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace RopeSnake.Core.Validation
{
    public class CountEqualsAttribute : ValidateRuleBaseAttribute
    {
        private int _count;

        public CountEqualsAttribute(int count)
        {
            _count = count;
        }

        protected override bool ValidateInternal(object value, LazyString path, Logger log)
        {
            var collection = value as ICollection;
            if (collection == null)
                throw new Exception("Value must implement ICollection");

            if (collection.Count != _count)
            {
                return Fail($"Count was {collection.Count}, but expected {_count}", path, log);
            }
            return true;
        }
    }
}
