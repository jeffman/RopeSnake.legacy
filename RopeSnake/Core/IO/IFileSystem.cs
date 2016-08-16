using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public interface IFileSystem
    {
        Stream CreateFile(string path);
        Stream OpenFile(string path);
        int GetFileSize(string path);
        bool FileExists(string path);
        void CreateDirectory(string path);
        bool DirectoryExists(string path);
        string[] GetDirectories(string path);
        string[] GetFiles(string path);
    }
}
