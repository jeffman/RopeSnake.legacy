using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core.Validation
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct |
        AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ValidateAttribute : ValidateBaseAttribute
    {

    }
}
