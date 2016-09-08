using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3.IO
{
    public abstract class OffsetTableReader
    {
        public BinaryStream Stream { get; }
        protected int[] Pointers { get; set; }

        public virtual int Count => Pointers.Length;
        public virtual int CurrentIndex { get; protected set; }
        public virtual bool EndOfTable => CurrentIndex >= Count;

        protected OffsetTableReader(BinaryStream stream)
        {
            Stream = stream;
        }

        public virtual bool Next()
        {
            int pointer = NextPointer();
            if (pointer == 0)
                return false;

            Stream.Position = pointer;
            return true;
        }

        public virtual int NextPointer()
        {
            if (CurrentIndex >= Count)
                throw new Exception("Exceeded the end of the table");

            return Pointers[CurrentIndex++];
        }

        public virtual void Skip(int count)
        {
            while (count-- > 0)
            {
                NextPointer();
            }
        }
    }
}
