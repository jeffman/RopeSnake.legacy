using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using RopeSnake.Gba;

namespace RopeSnake.Mother3
{
    public abstract class Mother3Module : IModule
    {
        private Dictionary<string, HashSet<int>> _references;

        protected Mother3Module()
        {
            _references = new Dictionary<string, HashSet<int>>();
        }

        protected void AddReferences(string key, IEnumerable<int> references)
        {
            HashSet<int> referenceSet;
            if (!_references.TryGetValue(key, out referenceSet))
            {
                referenceSet = new HashSet<int>();
                _references.Add(key, referenceSet);
            }

            foreach (int reference in references)
                referenceSet.Add(reference);
        }

        protected void UpdateRomReferences(Block romData, string key, int value)
        {
            var stream = romData.ToBinaryStream();

            foreach (int reference in _references[key])
            {
                stream.Position = reference;
                stream.WriteGbaPointer(value);
            }
        }

        #region IModule implementation

        public abstract string Name { get; }
        public abstract void ReadFromRom(Block romData);
        public abstract void WriteToRom(Block romData, AllocatedBlockCollection allocatedBlocks);
        public abstract void ReadFromFiles(IFileSystem manager);
        public abstract void WriteToFiles(IFileSystem manager);
        public abstract BlockCollection Serialize();
        public abstract void UpdateReferences(AllocatedBlockCollection allocatedBlocks);

        #endregion
    }
}
