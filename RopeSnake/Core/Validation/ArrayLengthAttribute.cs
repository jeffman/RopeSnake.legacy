using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace RopeSnake.Core.Validation
{
    public class ArrayLengthAttribute : ValidateRuleBaseAttribute
    {
        private int[] _lengths;

        public ArrayLengthAttribute(int[] lengths)
        {
            if (lengths == null)
                throw new ArgumentNullException(nameof(lengths));

            _lengths = lengths;
        }

        protected override bool ValidateInternal(object value, LazyString path, Logger log)
        {
            var array = value as Array;
            if (array == null)
                throw new Exception("Value must be an array");

            if (array.Rank != _lengths.Length)
                throw new Exception($"Array has a rank of {array.Rank} but expected {_lengths}");

            for (int i = 0; i < array.Rank; i++)
            {
                int length = array.GetLength(i);
                if (length != _lengths[i])
                {
                    return Fail($"Array length (rank {i}) was {length}, but expected {_lengths[i]}", path, log);
                }
            }

            return true;
        }
    }
}
