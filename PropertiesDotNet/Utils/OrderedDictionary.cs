using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#if !NETSTANDARD1_3
using System.Runtime.Serialization;
#endif

namespace PropertiesDotNet.Utils
{
    /// <summary>
    /// A generic implementation of <see cref="IDictionary{TKey, TValue}"/> that preserves key insertion order.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
#if !NETSTANDARD1_3
    [Serializable]
#endif
    internal class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
    {
        // Do not serialize the dictionary, only the list and load the
        // data from the list
#if !NETSTANDARD1_3
        [NonSerialized]
#endif
        private Dictionary<TKey, TValue> _dictionary;
        private readonly List<KeyValuePair<TKey, TValue>> _list;
        private readonly IEqualityComparer<TKey> _comparer;

        /// <summary>
        /// Gets the keys.
        /// </summary>
        public ICollection<TKey> Keys => new KeyCollection(this);

        /// <summary>
        /// Gets the values.
        /// </summary>
        public ICollection<TValue> Values => new ValueCollection(this);

        /// <summary>
        /// Gets the count.
        /// </summary>
        public int Count => _dictionary.Count;

        /// <summary>
        /// Gets a value indicating whether read is only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Creates a new <see cref="OrderedDictionary{TKey, TValue}"/>.
        /// </summary>
        public OrderedDictionary() : this(EqualityComparer<TKey>.Default)
        {
        }

        /// <summary>
        /// Creates a new <see cref="OrderedDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="comparer">The comparer for the dictionary.</param>
        public OrderedDictionary(IEqualityComparer<TKey> comparer)
        {
            _list = new List<KeyValuePair<TKey, TValue>>();
            _dictionary = new Dictionary<TKey, TValue>(comparer);
            _comparer = comparer;
        }

        /// <summary>
        /// Adds the <paramref name="key"/> and <paramref name="value"/>.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(TKey key, TValue value) => Add(new KeyValuePair<TKey, TValue>(key, value));

        /// <summary>
        /// Adds the <paramref name="item"/>.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _dictionary.Add(item.Key, item.Value);
            _list.Add(item);
        }

        /// <summary>
        /// Clears this dictionary.
        /// </summary>
        public void Clear()
        {
            _dictionary.Clear();
            _list.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => _dictionary.ContainsKey(item.Key) &&
            _dictionary.ContainsValue(item.Value);

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
            _list.CopyTo(array, arrayIndex);

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>An IEnumerator.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _list.GetEnumerator();

        public void Insert(int index, TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            _list.Insert(index, new KeyValuePair<TKey, TValue>(key, value));
        }

        public bool Remove(TKey key)
        {
            if (_dictionary.ContainsKey(key))
            {
                var index = _list.FindIndex(listKey => _comparer.Equals(listKey.Key, key));
                _list.RemoveAt(index);
                if (!_dictionary.Remove(key))
                {
                    throw new InvalidOperationException();
                }
                return true;
            }

            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

        public void RemoveAt(int index)
        {
            var key = _list[index].Key;
            _dictionary.Remove(key);
            _list.RemoveAt(index);
        }

        /// <inheritdoc/>
        public bool TryGetValue(TKey key, out TValue value) =>
            _dictionary.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

#if !NETSTANDARD1_3
        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            // Initialize dictionary from serialized list

            _dictionary = new Dictionary<TKey, TValue>();
            foreach (var listKey in _list)
            {
                _dictionary[listKey.Key] = listKey.Value;
            }
        }
#endif

        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set
            {
                if (_dictionary.ContainsKey(key))
                {
                    int index = _list.FindIndex(listKey => _comparer.Equals(listKey.Key, key));
                    _dictionary[key] = value;
                    _list[index] = new KeyValuePair<TKey, TValue>(key, value);
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public KeyValuePair<TKey, TValue> this[int index]
        {
            get => _list[index];
            set
            {
                _dictionary.Remove(_list[index].Key);
                _dictionary.Add(value.Key, value.Value);
                _list[index] = value;
            }
        }

        private class KeyCollection : ICollection<TKey>
        {
            private readonly OrderedDictionary<TKey, TValue> _orderedDictionary;

            public int Count => _orderedDictionary._list.Count;

            public bool IsReadOnly => true;

            public void Add(TKey item) => throw new NotSupportedException();

            public void Clear() => throw new NotSupportedException();

            public bool Contains(TKey item) => _orderedDictionary._dictionary.ContainsKey(item);

            public KeyCollection(OrderedDictionary<TKey, TValue> orderedDictionary)
            {
                _orderedDictionary = orderedDictionary;
            }

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                for (var i = 0; i < _orderedDictionary._list.Count; i++)
                    array[i] = _orderedDictionary._list[i + arrayIndex].Key;
            }

            public IEnumerator<TKey> GetEnumerator() =>
                _orderedDictionary._list.Select(x => x.Key).GetEnumerator();

            public bool Remove(TKey item) => throw new NotSupportedException();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private class ValueCollection : ICollection<TValue>
        {
            private readonly OrderedDictionary<TKey, TValue> _orderedDictionary;

            public int Count => _orderedDictionary._list.Count;

            public bool IsReadOnly => true;

            public void Add(TValue item) => throw new NotSupportedException();

            public void Clear() => throw new NotSupportedException();

            public bool Contains(TValue item) => _orderedDictionary._dictionary.ContainsValue(item);

            public ValueCollection(OrderedDictionary<TKey, TValue> orderedDictionary)
            {
                _orderedDictionary = orderedDictionary;
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                for (var i = 0; i < _orderedDictionary._list.Count; i++)
                    array[i] = _orderedDictionary._list[i + arrayIndex].Value;
            }

            public IEnumerator<TValue> GetEnumerator() =>
                _orderedDictionary._list.Select(x => x.Value).GetEnumerator();

            public bool Remove(TValue item) => throw new NotSupportedException();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
