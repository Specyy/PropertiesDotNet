using System;
using System.Collections;
using System.Collections.Generic;

namespace PropertiesDotNet.Utils
{
    /// <summary>
    /// Represents an adaptation for <see cref="ICollection{T}" />s that dynamically adds
    /// values since not all generic collections implement <see cref="IList" />.
    /// </summary>
    internal sealed class DynamicGenericList<T> : IList
    {
        private readonly ICollection<T> _genericCollection;

        /// <summary>
        /// Creates a new <see cref="DynamicGenericList{T}"/>.
        /// </summary>
        public DynamicGenericList() : this(new List<T>())
        {
        }

        /// <summary>
        /// Creates a new <see cref="DynamicGenericList{T}"/>.
        /// </summary>
        /// <param name="innerCollection">The inner collection.</param>
        public DynamicGenericList(ICollection<T> innerCollection)
        {
            _genericCollection = innerCollection ?? throw new ArgumentNullException(nameof(innerCollection));
        }

        /// <inheritdoc/>
        public int Add(object? value)
        {
            var index = _genericCollection.Count;
            _genericCollection.Add((T)value!);
            return index;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _genericCollection.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(object? value) => _genericCollection.Contains((T)value);

        /// <inheritdoc/>
        public int IndexOf(object? value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public void Insert(int index, object? value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public bool IsFixedSize
        {
            get { throw new NotSupportedException(); }
        }

        /// <inheritdoc/>
        public bool IsReadOnly => _genericCollection.IsReadOnly;

        /// <inheritdoc/>
        public void Remove(object? value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public object? this[int index]
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                ((IList<T>)_genericCollection)[index] = (T)value!;
            }
        }

        /// <inheritdoc/>
        public void CopyTo(Array array, int index)
        {
            _genericCollection.CopyTo((T[])array, index);
        }

        /// <inheritdoc/>
        public int Count => _genericCollection.Count;

        /// <inheritdoc/>
        public bool IsSynchronized
        {
            get { throw new NotSupportedException(); }
        }

        /// <inheritdoc/>
        public object SyncRoot
        {
            get { throw new NotSupportedException(); }
        }

        /// <inheritdoc/>
        public IEnumerator GetEnumerator()
        {
            return _genericCollection.GetEnumerator();
        }
    }

    internal sealed class ArrayList : IList
    {
        private object?[] data;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Initialized inside Clear()
        public ArrayList()
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
        {
            Clear();
        }

        public int Add(object? value)
        {
            if (Count == data.Length)
            {
                Array.Resize(ref data, data.Length * 2);
            }
            data[Count] = value;
            return Count++;
        }

        public void Clear()
        {
            data = new object[10];
            Count = 0;
        }

        bool IList.Contains(object? value) => throw new NotSupportedException();
        int IList.IndexOf(object? value) => throw new NotSupportedException();
        void IList.Insert(int index, object? value) => throw new NotSupportedException();
        void IList.Remove(object? value) => throw new NotSupportedException();
        void IList.RemoveAt(int index) => throw new NotSupportedException();

        public bool IsFixedSize => false;

        public bool IsReadOnly => false;

        public object? this[int index]
        {
            get
            {
                return data[index];
            }
            set
            {
                data[index] = value;
            }
        }

        public void CopyTo(Array array, int index)
        {
            Array.Copy(data, 0, array, index, Count);
        }

        public int Count { get; private set; }

        public bool IsSynchronized => false;
        public object SyncRoot => data;

        public IEnumerator GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
            {
                yield return data[i];
            }
        }
    }
}
