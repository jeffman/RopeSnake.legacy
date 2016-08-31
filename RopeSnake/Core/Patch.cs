using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RopeSnake.Core
{
    public class Patch
    {
        public int Position { get; set; }
        public byte[] Data { get; set; }

        public void Apply(Block block) => Apply(block.ToBinaryStream());

        public void Apply(BinaryStream stream)
        {
            int position = stream.Position;
            stream.Position = Position;
            stream.WriteBytes(Data, 0, Data.Length);
            stream.Position = position;
        }
    }

    [JsonObject]
    public class PatchCollection : ICollection<Patch>
    {
        [JsonProperty(PropertyName = "Chunks")]
        private List<Patch> _patches;

        public string Description { get; set; }

        #region ICollection implementation

        public int Count
        {
            get
            {
                return _patches.Count;
            }
        }

        public bool IsReadOnly => false;

        public void Add(Patch item)
        {
            _patches.Add(item);
        }

        public void Clear()
        {
            _patches.Clear();
        }

        public bool Contains(Patch item)
        {
            return _patches.Contains(item);
        }

        public void CopyTo(Patch[] array, int arrayIndex)
        {
            _patches.CopyTo(array, arrayIndex);
        }

        public IEnumerator<Patch> GetEnumerator()
        {
            return _patches.GetEnumerator();
        }

        public bool Remove(Patch item)
        {
            return _patches.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _patches.GetEnumerator();
        }

        #endregion
    }
}
