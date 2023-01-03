using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace PropertiesDotNet.Serialization
{
    /// <summary>
    /// Represents a set of ordered, doubly-linked, type-based components.
    /// </summary>
    public sealed class ComponentList<T> : ICollection, ICollection<T> where T : notnull
    {
        private readonly LinkedList<T> _components;
        private readonly Dictionary<Type, LinkedListNode<T>> _componentTypes;

        /// <inheritdoc/>
        public int Count => _components.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc/>
        public bool IsSynchronized => false;

        /// <inheritdoc/>
        public object SyncRoot { get; }

        /// <summary>
        /// Creates a new <see cref="ComponentList{T}"/>
        /// </summary>
        public ComponentList()
        {
            _components = new LinkedList<T>();
            _componentTypes = new Dictionary<Type, LinkedListNode<T>>();
            SyncRoot = new object();
        }

        /// <summary>
        /// Adds the component at the top of this list.
        /// </summary>
        /// <param name="component">The component to add.</param>
        public void Add(T component)
        {
            AddType(_components.AddFirst(component));
        }

        /// <summary>
        /// Attempts to add the component at the top of this list.
        /// </summary>
        /// <param name="component">The component to add.</param>
        /// <returns>Whether this list already contains the given component.</returns>
        public bool TryAdd(T component)
        {
            if (Contains(component))
                return false;

            Add(component);
            return true;
        }

        /// <summary>
        /// Adds the component at the bottom of this list.
        /// </summary>
        /// <param name="component">The component to add.</param>
        public void AddBottom(T component)
        {
            AddType(_components.AddLast(component));
        }

        /// <summary>
        /// Attempts to add the component at the bottom of this list.
        /// </summary>
        /// <param name="component">The component to add.</param>
        /// <returns>Whether this list already contains the given component.</returns>
        public bool TryAddBottom(T component)
        {
            if (Contains(component))
                return false;

            AddBottom(component);
            return true;
        }

        /// <summary>
        /// Adds the <paramref name="component"/> before the <paramref name="beforeType"/> in this list.
        /// </summary>
        /// <param name="component">The component to add.</param>
        /// <param name="beforeType">The type of the component to add the <paramref name="component"/> before.</param>
        public void AddBefore(T component, Type beforeType)
        {
            AddType(_components.AddBefore(_componentTypes[beforeType], component));
        }

        /// <summary>
        /// Attempts to add the <paramref name="component"/> before the <paramref name="beforeType"/> in this list.
        /// </summary>
        /// <param name="component">The component to add.</param>
        /// <returns>Whether this list already contains the given component.</returns>
        /// <param name="beforeType">The type of the component to add the <paramref name="component"/> before.</param>
        public bool TryAddBefore(T component, Type beforeType)
        {
            if (Contains(component))
                return false;

            AddBefore(component, beforeType);
            return true;
        }

        /// <summary>
        /// Adds the <paramref name="component"/> after the <paramref name="afterType"/> in this list.
        /// </summary>
        /// <param name="component">The component to add.</param>
        /// <param name="afterType">The type of the component to add the <paramref name="component"/> after.</param>
        public void AddAfter(T component, Type afterType)
        {
            AddType(_components.AddAfter(_componentTypes[afterType], component));
        }

        /// <summary>
        /// Attempts to add the <paramref name="component"/> after the <paramref name="beforeType"/> in this list.
        /// </summary>
        /// <param name="component">The component to add.</param>
        /// <returns>Whether this list already contains the given component.</returns>
        /// <param name="beforeType">The type of the component to add the <paramref name="component"/> after.</param>
        public bool TryAddAfter(T component, Type beforeType)
        {
            if (Contains(component))
                return false;

            AddAfter(component, beforeType);
            return true;
        }

        /// <summary>
        /// Removes the component type from this list.
        /// </summary>
        /// <param name="componentType">The component type to remove.</param>
        /// <returns>Whether this list contained the specified <paramref name="componentType"/>.</returns>
        public bool Remove(Type componentType)
        {
            if (!ContainsType(componentType))
                return false;

            _components.Remove(_componentTypes[componentType]);
            _componentTypes.Remove(componentType);
            return true;
        }

        /// <summary>
        /// Removes the component from this list.
        /// </summary>
        /// <param name="component">The component to remove.</param>
        /// <returns>Whether this list contained the specified <paramref name="component"/>.</returns>
        public bool Remove(T component) => Remove(component.GetType());

        /// <summary>
        /// Removes all components from this list.
        /// </summary>
        public void Clear()
        {
            _components.Clear();
            _componentTypes.Clear();
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void AddType(LinkedListNode<T> component) => _componentTypes.Add(component.Value.GetType(), component);

        /// <summary>
        /// Returns whether this ist contains the specified component.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <returns>Whether this ist contains the specified component.</returns>
        public bool Contains(T component) => ContainsType(component.GetType());

        /// <summary>
        /// Returns whether this list contains a component with the given type.
        /// </summary>
        /// <param name="type">The component type.</param>
        /// <returns>Whether this list contains a component with the given type.</returns>
        public bool ContainsType(Type type) => _componentTypes.ContainsKey(type);

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() => _components.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex) => _components.CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        public void CopyTo(Array array, int index) => CopyTo((T[])array, index);
    }
}