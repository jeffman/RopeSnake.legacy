using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFileSystem;

namespace RopeSnake.Core
{
    /// <summary>
    /// General extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Aligns a value to the specified alignment.
        /// </summary>
        /// <param name="value">value to align. Must be non-negative.</param>
        /// <param name="alignment">desired alignment. Must be positive.</param>
        /// <returns>aligned value</returns>
        public static int Align(this int value, int alignment)
        {
            if (alignment < 1)
                throw new InvalidOperationException("Alignment must be positive");

            if (value < 0)
                throw new InvalidOperationException("Value must be non-negative");

            if (alignment == 1)
                return value;

            int mask = -alignment;
            value += alignment - 1;
            value &= mask;
            return value;
        }

        /// <summary>
        /// Checks whether a value is aligned to the specified alignment.
        /// </summary>
        /// <param name="value">value to check. Must be non-negative.</param>
        /// <param name="alignment">alignment to check. Must be positive.</param>
        /// <returns><c>true</c> if <paramref name="value"/> is aligned to <paramref name="alignment"/>, <c>false</c> otherwise</returns>
        public static bool IsAligned(this int value, int alignment)
        {
            if (alignment < 1)
                throw new InvalidOperationException("Alignment must be positive");

            if (value < 0)
                throw new InvalidOperationException("Value must be non-negative");

            if (alignment == 1)
                return true;

            int mask = -alignment;
            return value == (value & mask);
        }

        /// <summary>
        /// Enumerates the nodes of a <see cref="LinkedList{T}"/>.
        /// </summary>
        /// <param name="list">the linked list whose nodes to enumerate</param>
        public static IEnumerable<LinkedListNode<T>> EnumerateNodes<T>(this LinkedList<T> list)
        {
            var current = list.First;

            while (current != null)
            {
                yield return current;
                current = current.Next;
            }
        }

        public static void AddRange<T>(this ISet<T> set, IEnumerable<T> values)
        {
            foreach (T value in values)
                set.Add(value);
        }

        public static FileSystemPath ToPath(this string path)
        {
            return FileSystemPath.Parse(path);
        }

        public static FileSystemState GetState(this IFileSystemWrapper fileSystem, FileSystemPath path, params FileSystemPath[] ignorePaths)
        {
            return new FileSystemState(fileSystem.GetEntitiesRecursive(path)
                .Where(p => !ignorePaths.Any(i => p == i || p.IsChildOf(i)))
                .Select(p => fileSystem.GetMetaData(p)));
        }
    }
}
