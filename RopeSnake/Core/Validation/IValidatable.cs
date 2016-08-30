using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core.Validation
{
    public interface IValidatable
    {
        bool Validate(LazyString path);
    }
}
