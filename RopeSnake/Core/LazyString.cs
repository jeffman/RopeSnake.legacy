using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public sealed class LazyString
    {
        private Func<string> _stringFunc;

        public LazyString(string str) : this(() => str)
        {

        }

        public LazyString(Func<string> strFunc)
        {
            _stringFunc = strFunc;
        }

        public LazyString Append(string str)
        {
            return new LazyString(() => string.Concat(_stringFunc(), str));
        }

        public string Evaluate()
        {
            return _stringFunc();
        }
    }
}
