using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace RopeSnake.Core.Validation
{
    public class CountEquals : ValidateRuleBaseAttribute
    {
        private int _count;

        public CountEquals(int count)
        {
            _count = count;
        }

        public override bool Validate(object value, LazyString path, Logger log)
        {
            if (value == null)
                return true;

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
