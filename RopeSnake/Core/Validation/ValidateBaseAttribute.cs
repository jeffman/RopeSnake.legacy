using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core.Validation
{
    public abstract class ValidateBaseAttribute : Attribute
    {
        public virtual ValidateFlags Flags { get; set; } = ValidateFlags.None;
    }

    [Flags]
    public enum ValidateFlags
    {
        None = 0,
        Instance = 1,
        Collection = 2,
        DictionaryValues = 4,
        DictionaryKeys = 8
    }
}
