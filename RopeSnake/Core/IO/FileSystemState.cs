using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public sealed class FileSystemState
    {
        public List<FileSystemProperties> FileProperties { get; }

        public FileSystemState(IEnumerable<FileSystemProperties> fileProperties)
        {
            FileProperties = new List<FileSystemProperties>(fileProperties);
        }

        public Dictionary<string, FileChangedState> Compare(FileSystemState before)
        {
            var results = new Dictionary<string, FileChangedState>();
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
