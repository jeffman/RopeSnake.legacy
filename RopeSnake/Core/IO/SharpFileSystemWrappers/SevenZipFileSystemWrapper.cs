using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFileSystem;
using SharpFileSystem.SevenZip;
using SevenZip;
using System.IO;
using System.Reflection;

namespace RopeSnake.Core
{
    public class SevenZipFileSystemWrapper : SevenZipFileSystem, IFileSystemWrapper
    {
        protected SevenZipExtractor _reflectedExtractor;

        public SevenZipFileSystemWrapper(Stream stream)
            : base(stream)
        {
            _reflectedExtractor = GetReflectedExtractor();
        }

        public SevenZipFileSystemWrapper(string physicalPath)
            : base(physicalPath)
        {
            _reflectedExtractor = GetReflectedExtractor();
        }

        protected SevenZipExtractor GetReflectedExtractor()
        {
            // This is a hack until SevenZipFileSystem._extractor becomes protected instead of private
            var type = typeof(SevenZipFileSystem);
            var fieldInfo = type.GetField("_extractor", BindingFlags.Instance | BindingFlags.NonPublic);
            return (SevenZipExtractor)fieldInfo.GetValue(this);
        }

        public FileMetaData GetMetaData(FileSystemPath path)
        {
            if (!Exists(path))
                throw new FileNotFoundException("File or directory not found", path.Path);

            var sevenZipPath = GetSevenZipPath(path);
            var info = _reflectedExtractor.ArchiveFileData.First(a => a.FileName == sevenZipPath);

            return new FileMetaData(path, info.LastWriteTime.ToUniversalTime(), (long)info.Size);
        }
    }
}
