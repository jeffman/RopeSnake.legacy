using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace RopeSnake.Core.Validation
{
    public class KeysMatchEnum : ValidateRuleBaseAttribute
    {
        private Type _enumType;
        private HashSet<object> _keys;

        public KeysMatchEnum(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException(nameof(enumType));

            _keys = new HashSet<object>(Enum.GetValues(enumType).Cast<object>());
        }

        protected override bool ValidateInternal(object value, LazyString path, Logger log)
        {
            var dictionary = value as IDictionary;
            if (dictionary == null)
                throw new Exception("Value must implement IDictionary");

            bool success = true;

            foreach (var key in dictionary.Keys)
            {
                if (!_keys.Contains(key))
                {
                    success &= Fail($"Invalid key {key}", path, log);
                }
            }

            foreach (var key in _keys)
            {
                if (!dictionary.Contains(key))
                {
                    success &= Fail($"Missing key {key}", path, log);
                }
            }

            return success;
        }
    }
}
