﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace RopeSnake.Core
{
    public sealed class Compiler
    {
        private sealed class AllocationRequest
        {
            public int Size { get; }
            private IList<string> _contiguousBlocks;
            private int?[] _offsets;

            public AllocationRequest(BlockCollection blockCollection, IList<string> contiguousKeys, int contiguousAlignment = 1)
            {
                _contiguousBlocks = contiguousKeys;
                _offsets = new int?[contiguousKeys.Count];

                int currentOffset = 0;

                for (int i = 0; i < contiguousKeys.Count; i++)
                {
                    string key = contiguousKeys[i];
                    var block = blockCollection[key];

                    if (block != null)
                    {
                        currentOffset = currentOffset.Align(contiguousAlignment);
                        _offsets[i] = currentOffset;
                        currentOffset += block.Size;
                    }
                    else
                    {
                        _offsets[i] = null;
                    }
                }

                Size = currentOffset;
            }

            public Dictionary<string, int> GenerateAllocatedPointers(int contiguousPointer)
            {
                var allocatedPointers = new Dictionary<string, int>();

                for (int i = 0; i < _contiguousBlocks.Count; i++)
                {
                    string key = _contiguousBlocks[i];
                    int? offset = _offsets[i];

                    if (offset != null)
                    {
                        allocatedPointers.Add(key, contiguousPointer + offset.Value);
                    }
                    else
                    {
                        allocatedPointers.Add(key, 0);
                    }
                }

                return allocatedPointers;
            }
        }

        private sealed class ResolveSerializedBlocksResult
        {
            public BlockCollection Blocks { get; }
            public IEnumerable<string> UpdatedKeys { get; }

            public ResolveSerializedBlocksResult(BlockCollection blocks, IEnumerable<string> updatedKeys)
            {
                Blocks = blocks;
                UpdatedKeys = updatedKeys;
            }
        }

        public class CompilationResult
        {
            public BlockCollection WrittenBlocks { get; }
            public IEnumerable<string> UpdatedKeys { get; }
            public IReadOnlyDictionary<string, int> AllocationResult { get; }

            public CompilationResult(BlockCollection writtenBlocks, IEnumerable<string> updatedKeys,
                IDictionary<string, int> allocationResult)
            {
                WrittenBlocks = writtenBlocks;
                UpdatedKeys = updatedKeys;
                AllocationResult = new ReadOnlyDictionary<string, int>(allocationResult);
            }
        }

        private Block _romData;
        private IAllocator _allocator;
        private IEnumerable<IModule> _modules;
        private BlockCollection _blockCache;
        private ParallelOptions _parallelOptions;

        public int AllocationAlignment { get; set; } = 1;

        private Compiler() { }

        public static Compiler Create(Block romData, IAllocator allocator, IEnumerable<IModule> modules,
            BlockCollection blockCache, int maxThreads = 1)
        {
            if (maxThreads < 1)
                throw new ArgumentException(nameof(maxThreads));

            Compiler compiler = new Compiler();
            compiler._romData = romData;
            compiler._allocator = allocator;
            compiler._modules = modules;
            compiler._blockCache = new BlockCollection(blockCache);
            compiler._parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxThreads };
            return compiler;
        }

        public CompilationResult Compile(IProgress<ProgressPercent> progress = null)
        {
            var writtenBlocks = new BlockCollection();
            var updatedKeys = new HashSet<string>();

            var allSerializationResults = new Dictionary<IModule, ModuleSerializationResult>();
            var allResolvedBlocks = new Dictionary<IModule, ResolveSerializedBlocksResult>();
            var allocationRequests = new Dictionary<IModule, IEnumerable<AllocationRequest>>();

            foreach (var module in _modules)
            {
                var serializationResult = module.Serialize();
                var resolvedBlocks = ResolveSerializedBlocks(serializationResult.Blocks, _blockCache, progress);
                var allFragments = GetAllFragments(resolvedBlocks.Blocks, serializationResult.ContiguousKeys);
                allocationRequests.Add(module, GetAllocationRequests(resolvedBlocks.Blocks, allFragments));

                allSerializationResults.Add(module, serializationResult);
                allResolvedBlocks.Add(module, resolvedBlocks);
                updatedKeys.AddRange(resolvedBlocks.UpdatedKeys);
            }

            var allocationResults = ProcessAllocationRequests(allocationRequests.Values
                .SelectMany(r => r)
                .OrderByDescending(r => r.Size));

            foreach (var module in _modules)
            {
                var blocks = allResolvedBlocks[module].Blocks;
                var allocatedPointers = blocks.Keys.ToDictionary(k => k, k => allocationResults[k]);

                module.WriteToRom(_romData, new AllocatedBlockCollection(blocks, allocatedPointers));

                writtenBlocks.AddRange(blocks);
            }

            return new CompilationResult(writtenBlocks, updatedKeys, allocationResults);
        }

        private ResolveSerializedBlocksResult ResolveSerializedBlocks(LazyBlockCollection lazyBlocks, BlockCollection blockCache,
            IProgress<ProgressPercent> progress = null)
        {
            var resolvedBlocks = new BlockCollection();
            var updatedKeys = new List<string>();
            var dict = new ConcurrentDictionary<string, Block>();
            int total = lazyBlocks.Count;
            int currentIndex = 1;

            Parallel.ForEach(lazyBlocks.Keys, _parallelOptions, key =>
            {
                if (blockCache != null && blockCache.ContainsKey(key))
                {
                    progress?.Report(new ProgressPercent($"Retrieving {key} from cache", currentIndex * 100f / total));

                    dict.TryAdd(key, blockCache[key]);
                }
                else
                {
                    progress?.Report(new ProgressPercent($"Serializing {key}", currentIndex * 100f / total));

                    var updatedBlock = lazyBlocks[key]();
                    dict.TryAdd(key, updatedBlock);
                    updatedKeys.Add(key);
                }

                Interlocked.Increment(ref currentIndex);
            });

            resolvedBlocks.AddRange(dict.OrderBy(kv => kv.Key));
            return new ResolveSerializedBlocksResult(resolvedBlocks, updatedKeys);
        }

        private IEnumerable<IList<string>> GetAllFragments(BlockCollection blocks, IEnumerable<IList<string>> contiguousKeys)
        {
            var allFragments = new List<IList<string>>();
            var contiguousKeySet = new HashSet<string>();

            if (contiguousKeys != null)
            {
                allFragments.AddRange(contiguousKeys);
                foreach (var key in contiguousKeys.SelectMany(k => k))
                {
                    contiguousKeySet.Add(key);
                }
            }

            foreach (var fragment in blocks.Keys.Where(k => !contiguousKeySet.Contains(k)))
            {
                allFragments.Add(new List<string> { fragment });
            }

            return allFragments;
        }

        private IEnumerable<AllocationRequest> GetAllocationRequests(BlockCollection blocks, IEnumerable<IList<string>> contiguousKeys)
        {
            var requests = new List<AllocationRequest>();

            foreach (var keyList in contiguousKeys)
            {
                requests.Add(new AllocationRequest(blocks, keyList, AllocationAlignment));
            }

            return requests;
        }

        private Dictionary<string, int> ProcessAllocationRequests(
            IOrderedEnumerable<AllocationRequest> requests)
        {
            var allAllocatedPointers = new Dictionary<string, int>();

            foreach (var request in requests)
            {
                int pointer;
                if (request.Size > 0)
                {
                    pointer = _allocator.Allocate(request.Size, AllocationAlignment, AllocationMode.Smallest);
                }
                else
                {
                    pointer = 0;
                }

                var allocatedPointers = request.GenerateAllocatedPointers(pointer);
                foreach (var kv in allocatedPointers)
                {
                    allAllocatedPointers.Add(kv.Key, kv.Value);
                }
            }

            return allAllocatedPointers;
        }
    }
}
