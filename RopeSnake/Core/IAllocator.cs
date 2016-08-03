using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public interface IAllocator
    {
        int Allocate(int size, int alignment, AllocationMode mode);
        void Deallocate(Range range);
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

    public sealed class AllocationException : Exception
    {
        public int RequestedSize { get; }
        public int RequestedAlignment { get; set; }
        public AllocationMode RequestedMode { get; set; }

        public AllocationException(int requestedSize, int requestedAlignment, AllocationMode requestedMode)
            : this($"Could not allocate the requested size of {requestedSize}. Alignment: {requestedAlignment}, mode: {requestedMode}",
                  requestedSize,
                  requestedAlignment,
                  requestedMode)
        { }

        public AllocationException(string message, int requestedSize, int requestedAlignment, AllocationMode requestedMode)
            : base(message)
        {
            RequestedSize = requestedSize;
            RequestedAlignment = requestedAlignment;
            RequestedMode = requestedMode;
        }
    }
}
