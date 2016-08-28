using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFileSystem;

namespace RopeSnake.Core
{
    public sealed class FileEventArgs
    {
        public FileSystemPath Path { get; private set; }
        public IndexTotal Index { get; private set; }

        public FileEventArgs(FileSystemPath path, IndexTotal index)
        {
            Path = path;
            Index = index;
        }
    }

    public struct IndexTotal : IEquatable<IndexTotal>
    {
        public static readonly IndexTotal Single = new IndexTotal(1, 1);

        public readonly int Index;
        public readonly int Total;

        public IndexTotal(int index, int total)
        {
            if (index < 1)
                throw new ArgumentException($"{nameof(index)} must be at least 1");

            if (index > total)
                throw new ArgumentException($"{nameof(index)} cannot exceed {nameof(total)}");

            Index = index;
            Total = total;
        }

        public bool Equals(IndexTotal other)
        {
            return (Index == other.Index) && (Total == other.Total);
        }

        public static bool operator ==(IndexTotal left, IndexTotal right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IndexTotal left, IndexTotal right)
        {
            return !(left == right);
        }

        public float ToPercent()
        {
            return Index * 100f / Total;
        }

        public override bool Equals(object obj)
        {
            return (obj is IndexTotal) && ((IndexTotal)obj == this);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode() ^ Total.GetHashCode();
        }

        public override string ToString()
        {
            if (Index == 1 && Total == 1)
            {
                return "";
            }
            else
            {
                return $"[{Index}/{Total}]";
            }
        }
    }

    public delegate void FileEventDelegate(object sender, FileEventArgs e);
}
