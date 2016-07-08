using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public sealed class Compiler
    {
        private Block _romData;
        private IAllocator _allocator;
        private IEnumerable<IModule> _modules;

        public int AllocationAlignment { get; set; } = 1;

        private Compiler() { }

        public static Compiler Create(Block romData, IAllocator allocator, IEnumerable<IModule> modules)
        {
            Compiler compiler = new Compiler();
            compiler._romData = romData;
            compiler._allocator = allocator;
            compiler._modules = modules;
            return compiler;
        }

        public void Compile()
        {
            int alignment = AllocationAlignment;
            var allocatedPointers = new Dictionary<string, int>();

            foreach (var module in _modules)
            {
                var blocks = module.Serialize();

                foreach (var blockPair in blocks)
                {
                    int pointer = _allocator.Allocate(blockPair.Value.Size, alignment);
                    allocatedPointers.Add(blockPair.Key, pointer);
                }

                var allocatedBlocks = new AllocatedBlockCollection(blocks, allocatedPointers);
                module.WriteToRom(_romData, allocatedBlocks);
            }
        }
    }
}
