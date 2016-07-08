using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public interface IBinarySerializable
    {
        void Serialize(Stream stream);
        void Deserialize(Stream stream, int size);
    }
}
