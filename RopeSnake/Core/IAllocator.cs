using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public interface IAllocator
    {
        int Allocate(int size, int alignment);
        void Deallocate(Range range);
    }

    public sealed class AllocationException : Exception
    {
        public int RequestedSize { get; }
        public int RequestedAlignment { get; set; }

        public AllocationException(int requestedSize, int requestedAlignment)
            : this($"Could not allocated the requested size, {requestedSize}, with the requested alignment, {requestedAlignment}",
                  requestedSize,
                  requestedAlignment)
        { }

        public AllocationException(string message, int requestedSize, int requestedAlignment)
            : base(message)
        {
            RequestedSize = requestedSize;
            RequestedAlignment = requestedAlignment;
        }
    }
}
