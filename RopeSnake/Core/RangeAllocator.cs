using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    /// <summary>
    /// The default implementation of <see cref="IAllocator"/>.
    /// </summary>
    public sealed class RangeAllocator : IAllocator
    {
        private object _lockObj;

        #region Private members

        private class RangeComparer : IComparer<Range>
        {
            public int Compare(Range x, Range y)
            {
                int startCompare = x.Start.CompareTo(y.Start);
                if (startCompare == 0)
                {
                    return x.End.CompareTo(y.End);
                }
                else
                {
                    return startCompare;
                }
            }
        }

        private static IComparer<Range> rangeComparer = new RangeComparer();

        private LinkedList<Range> _rangeList;

        #endregion

        /// <summary>
        /// Gets or sets the allocation mode.
        /// </summary>
        public AllocationMode AllocationMode { get; set; } = AllocationMode.Smallest;

        /// <summary>
        /// Gets or sets a flag indicating whether unaligned chunks should be discarded during allocation.
        /// 
        /// If <c>true</c>, unaligned chunks will be discarded during allocation. This results in lower
        /// fragmentation and consequently faster future allocations, but with memory being lost.
        /// 
        /// If <c>false</c>, unaligned chunks will not be discarded, resulting in fragmentation
        /// with no lost memory.
        /// </summary>
        public bool DiscardUnalignedChunks { get; set; } = true;

        // TODO: store "lost" chunks somewhere so that they can be restored/exported later

        /// <summary>
        /// Creates an empty instance.
        /// </summary>
        public RangeAllocator()
        {
            _rangeList = new LinkedList<Range>();
        }

        /// <summary>
        /// Creates an instance with the given default ranges.
        /// </summary>
        /// <param name="ranges">default ranges</param>
        public RangeAllocator(IEnumerable<Range> ranges)
            : this()
        {
            foreach (var range in ranges.Distinct().OrderBy(r => r, rangeComparer))
            {
                _rangeList.AddLast(range);
            }

            Consolidate();
        }

        /// <summary>
        /// Creates an instance with the given default ranges.
        /// </summary>
        /// <param name="ranges">default ranges</param>
        public RangeAllocator(params Range[] ranges)
            : this((IEnumerable<Range>)ranges)
        { }

        /// <inheritdoc/>
        public int Allocate(int size, int alignment)
        {
            lock (_lockObj)
            {
                var query = _rangeList.EnumerateNodes().Where(n => n.Value.Size >= size);

                switch (AllocationMode)
                {
                    case AllocationMode.Earliest:
                        break;

                    case AllocationMode.Smallest:
                        query = query.OrderBy(n => n.Value.Size);
                        break;

                    case AllocationMode.Largest:
                        query = query.OrderByDescending(n => n.Value.Size);
                        break;

                    default:
                        throw new NotSupportedException();
                }

                var match = query.FirstOrDefault(n => n.Value.GetAlignedSize(alignment) >= size);

                if (match == null)
                {
                    throw new AllocationException(
                        $"Could not allocate the requested block: size = {size}, alignment = {alignment}. Available ranges: {ToString()}",
                        size, alignment);
                }

                Range range = match.Value;
                int location = range.Start.Align(alignment);

                if (!DiscardUnalignedChunks && location > range.Start)
                {
                    // Add a small chunk for the unaligned portion of the matched range
                    Range before = Range.StartEnd(range.Start, location - 1);
                    _rangeList.AddBefore(match, before);
                }

                if (location + size - 1 < range.End)
                {
                    // Add a chunk for the remaining range
                    Range after = Range.StartEnd(location + size, range.End);
                    _rangeList.AddAfter(match, after);
                }

                _rangeList.Remove(match);

                return location;
            }
        }

        /// <inheritdoc/>
        public void Deallocate(Range range)
        {
            lock (_lockObj)
            {
                LinkedListNode<Range> current = _rangeList.First;
                int? compareResult = null;

                while (current != null && (compareResult = rangeComparer.Compare(range, current.Value)) >= 0)
                {
                    current = current.Next;
                }

                if (compareResult == 0)
                {
                    // Ignore duplicates
                    return;
                }

                // Insert
                if (current == null)
                {
                    current = _rangeList.AddLast(range);
                }
                else
                {
                    current = _rangeList.AddBefore(current, range);
                }

                // Consolidate backwards
                Consolidate(current, n => n.Previous);

                // Consolidate forwards
                Consolidate(current, n => n.Next);
            }
        }

        private void Consolidate()
        {
            if (_rangeList.Count < 1)
                return;

            LinkedListNode<Range> current = _rangeList.First;

            while (current != null)
            {
                current = Consolidate(current, n => n.Next);
            }
        }

        private LinkedListNode<Range> Consolidate(LinkedListNode<Range> current, Func<LinkedListNode<Range>, LinkedListNode<Range>> nextSelector)
        {
            LinkedListNode<Range> next = nextSelector(current);
            while (next != null && current.Value.CanCombineWith(next.Value))
            {
                Range combined = current.Value.CombineWith(next.Value);
                current.Value = combined;
                _rangeList.Remove(next);
                next = nextSelector(current);
            }
            return next;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Join(", ", _rangeList);
        }
    }

    /// <summary>
    /// Indicates how free ranges are prioritized during allocation.
    /// </summary>
    public enum AllocationMode
    {
        /// <summary>
        /// The earliest (by start location) candidate range is prioritized.
        /// </summary>
        Earliest,

        /// <summary>
        /// The smallest candidate range is prioritized.
        /// </summary>
        Smallest,

        /// <summary>
        /// The largest candidate range is prioritized.
        /// </summary>
        Largest
    }
}
