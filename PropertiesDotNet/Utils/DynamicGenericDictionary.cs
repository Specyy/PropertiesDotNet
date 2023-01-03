using System;
using System.Collections;
using System.Collections.Generic;

namespace PropertiesDotNet.Utils
{
    /// <summary>
    /// Represents an <see cref="IDictionary{TKey, TValue}"/> that dynamically adds <see cref="object"/>
    /// items. These items must of the key and value types.
    /// </summary>
    internal sealed class DynamicGenericDictionary<TKey, TValue> : IDictionary where TKey : notnull
    {
        private readonly IDictionary<TKey, TValue> _innerDictionary;

        /// <summary>
        /// Returns the inner dictionary.
        /// </summary>
        public IDictionary<TKey, TValue> InnerDictionary => _innerDictionary;

        /// <summary>
        /// Creates a new <see cref="DynamicGenericDictionary{TKey, TValue}"/>.
        /// </summary>
        public DynamicGenericDictionary() : this(new Dictionary<TKey, TValue>())
        {

        }

        /// <summary>
        /// Creates a new <see cref="DynamicGenericDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="inner">The inner dictionary.</param>
        public DynamicGenericDictionary(IDictionary<TKey, TValue> inner)
        {
            _innerDictionary = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        /// <inheritdoc/>
        public object? this[object key]
        {
            get => _innerDictionary[(TKey)key];
            set => _innerDictionary[(TKey)key] = (TValue)value;
        }

        /// <inheritdoc/>
        public bool IsFixedSize => throw new NotSupportedException();

        /// <inheritdoc/>
        public bool IsReadOnly => _innerDictionary.IsReadOnly;

        /// <inheritdoc/>
        public ICollection Keys => throw new NotSupportedException();

        /// <inheritdoc/>
        public ICollection Values => throw new NotSupportedException();

        /// <inheritdoc/>
        public int Count => _innerDictionary.Count;

        /// <inheritdoc/>
        public bool IsSynchronized => throw new NotSupportedException();

        /// <inheritdoc/>
        public object SyncRoot => throw new NotSupportedException();

        /// <inheritdoc/>
        public void Add(object key, object? value)
        {
            _innerDictionary.Add((TKey)key, (TValue)value);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _innerDictionary.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(object key)
        {
            return key is TKey casted && _innerDictionary.ContainsKey(casted);
        }

        /// <inheritdoc/>
        public void CopyTo(Array array, int index)
        {
            _innerDictionary.CopyTo((KeyValuePair<TKey, TValue>[])array, index);
        }

        /// <inheritdoc/>
        public IDictionaryEnumerator GetEnumerator() => new DynamicDictionaryEnumerator(_innerDictionary.GetEnumerator());

        /// <inheritdoc/>
        public void Remove(object key)
        {
            _innerDictionary.Remove((TKey)key);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class DynamicDictionaryEnumerator : IDictionaryEnumerator
        {
            private readonly IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;

            /// <summary>
            /// Creates a new <see cref="DynamicDictionaryEnumerator"/>.
            /// </summary>
            /// <param name="enumerator">The inner enumerator.</param>
            public DynamicDictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
            {
                _enumerator = enumerator;
            }

            /// <inheritdoc/>
            public DictionaryEntry Entry => new DictionaryEntry(Key, Value);

            /// <inheritdoc/>
            public object Key => _enumerator.Current.Key;

            /// <inheritdoc/>
            public object? Value => _enumerator.Current.Value;

            /// <inheritdoc/>
            public object Current => Entry;

            /// <inheritdoc/>
            public bool MoveNext() => _enumerator.MoveNext();

            /// <inheritdoc/>
            public void Reset() => _enumerator.Reset();
        }
    }
}
