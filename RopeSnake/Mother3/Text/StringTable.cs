using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RopeSnake.Mother3.Text
{
    [JsonObject]
    public sealed class StringTable : IList<string>
    {
        public ushort MaxLength { get; set; }

        [JsonProperty]
        public List<string> Strings { get; private set; } = new List<string>();

        #region IList implementation

        public string this[int index]
        {
            get
            {
                return Strings[index];
            }

            set
            {
                Strings[index] = value;
            }
        }

        [JsonIgnore]
        public int Count => Strings.Count;

        [JsonIgnore]
        public bool IsReadOnly => false;

        public void Add(string item)
        {
            Strings.Add(item);
        }

        public void Clear()
        {
            Strings.Clear();
        }

        public bool Contains(string item) => Strings.Contains(item);

        public void CopyTo(string[] array, int arrayIndex)
        {
            Strings.CopyTo(array, arrayIndex);
        }

        public IEnumerator<string> GetEnumerator() => Strings.GetEnumerator();

        public int IndexOf(string item) => Strings.IndexOf(item);

        public void Insert(int index, string item)
        {
            Strings.Insert(index, item);
        }

        public bool Remove(string item) => Strings.Remove(item);

        public void RemoveAt(int index)
        {
            Strings.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() => Strings.GetEnumerator();

        #endregion
    }
}
