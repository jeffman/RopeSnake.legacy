using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public interface IFilesystem
    {
        Stream CreateFile(string path);
        Stream OpenFile(string path);
        int GetFileSize(string path);
        bool FileExists(string path);
    }
}
