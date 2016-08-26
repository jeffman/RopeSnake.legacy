using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFileSystem;

namespace RopeSnake.Core
{
    public sealed class FileSystemState
    {
        public IEnumerable<FileMetaData> FileProperties { get; }

        public FileSystemState(IEnumerable<FileMetaData> fileProperties)
        {
            FileProperties = fileProperties.ToArray();
        }

        public Dictionary<FileSystemPath, FileChangedState> Compare(FileSystemState before)
        {
            var results = new Dictionary<FileSystemPath, FileChangedState>();
            var beforeLookup = before.FileProperties.ToDictionary(p => p.Path, p => p);
            var afterLookup = FileProperties.ToDictionary(p => p.Path, p => p);

            // Check for added files
            foreach (var addedPath in afterLookup.Keys.Except(beforeLookup.Keys))
            {
                results.Add(addedPath, FileChangedState.Added);
            }

            // Check for removed files
            foreach (var removedPath in beforeLookup.Keys.Except(afterLookup.Keys))
            {
                results.Add(removedPath, FileChangedState.Removed);
            }

            // Check for modified files
            foreach (var commonPath in beforeLookup.Keys.Intersect(afterLookup.Keys))
            {
                var beforeProperties = beforeLookup[commonPath];
                var afterProperties = afterLookup[commonPath];

                if (beforeProperties.Size != afterProperties.Size ||
                    beforeProperties.LastModified != afterProperties.LastModified)
                {
                    results.Add(commonPath, FileChangedState.Modified);
                }
            }

            return results;
        }
    }

    public enum FileChangedState
    {
        Added,
        Removed,
        Modified
    }
}
