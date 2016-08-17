using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private IDictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();
        private List<TKey> _orderedKeys = new List<TKey>();

        public virtual TValue this[TKey key]
        {
            get { return _dict[key]; }
            set
            {
                if (IsReadOnly)
                    throw new InvalidOperationException("The dictionary is read-only");

                if (!_orderedKeys.Contains(key))
                    _orderedKeys.Add(key);

                _dict[key] = value;
            }
        }

        public int Count => _dict.Count;

        public virtual bool IsReadOnly => _dict.IsReadOnly;

        public virtual ICollection<TKey> Keys => _orderedKeys;

        public virtual ICollection<TValue> Values => _orderedKeys.Select(k => _dict[k]).ToArray();

        public OrderedDictionary() { }

        public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> copyFrom)
        {
            AddRange(copyFrom);
        }

        public virtual void Add(TKey key, TValue value)
        {
            if (IsReadOnly)
                throw new InvalidOperationException("The dictionary is read-only");

            ForceAdd(key, value);
        }

        public virtual void Add(KeyValuePair<TKey, TValue> item)
            => Add(item.Key, item.Value);

        protected virtual void ForceAdd(TKey key, TValue value)
        {
            _dict.Add(key, value);
            _orderedKeys.Add(key);
        }

        public virtual void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> range)
        {
            foreach (var kv in range)
            {
                Add(kv.Key, kv.Value);
            }
        }

        public virtual void Clear()
        {
            if (IsReadOnly)
                throw new InvalidOperationException("The dictionary is read-only");

            _dict.Clear();
            _orderedKeys.Clear();
        }

        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dict.Contains(item);
        }

        public virtual bool ContainsKey(TKey key)
        {
            return _dict.ContainsKey(key);
        }

        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _dict.CopyTo(array, arrayIndex);
        }

        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (TKey key in Keys)
            {
                yield return new KeyValuePair<TKey, TValue>(key, _dict[key]);
            }
        }
    
        public virtual bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (IsReadOnly)
                throw new InvalidOperationException("The dictionary is read-only");

            _orderedKeys.Remove(item.Key);
            return _dict.Remove(item);
        }

        public virtual bool Remove(TKey key)
        {
            if (IsReadOnly)
                throw new InvalidOperationException("The dictionary is read-only");

            _orderedKeys.Remove(key);
            return _dict.Remove(key);
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            return _dict.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dict.GetEnumerator();
        }
    }
}
