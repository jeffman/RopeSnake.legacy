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
            var allocatedPointers = new Dictionary<IModule, Dictionary<string, int>>();

            foreach (var module in _modules)
            {
                allocatedPointers.Add(module, new Dictionary<string, int>());
            }

            var serializedBlocks = _modules.ToDictionary(m => m, m => m.Serialize());
            var orderedBlocks = serializedBlocks
                .SelectMany(kv => kv.Value, (kv, b) => new { Module = kv.Key, Key = b.Key, Block = b.Value })
                .OrderByDescending(o => o.Block.Size)
                .ToList();

            foreach (var orderedBlock in orderedBlocks)
            {
                int pointer = _allocator.Allocate(orderedBlock.Block.Size, AllocationAlignment, AllocationMode.Smallest);
                allocatedPointers[orderedBlock.Module].Add(orderedBlock.Key, pointer);
            }

            foreach (var module in _modules)
            {
                var allocatedBlocks = new AllocatedBlockCollection(serializedBlocks[module], allocatedPointers[module]);
                module.WriteToRom(_romData, allocatedBlocks);
            }
        }
    }
}
