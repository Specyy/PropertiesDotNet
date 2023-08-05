using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PropertiesDotNet.Serialization.PropertiesTree
{
    /// <summary>
    /// Represents a composite element of a ".properties" document tree. 
    /// </summary>
    public sealed class PropertiesObject : PropertiesTreeNode, IEnumerable<PropertiesTreeNode>, IEnumerable, IEquatable<PropertiesObject>
    {
        private readonly Dictionary<string, PropertiesTreeNode> _children;

        /// <summary>
        /// Returns the (direct) children of this ".properties" object.
        /// </summary>
        public IEnumerable<PropertiesTreeNode> Children => _children.Values;

        /// <summary>
        /// Returns the number of (direct) children within this ".properties" object.
        /// </summary>
        public int ChildCount => _children.Count;

        /// <summary>
        /// Creates a new object node for a ".properties" document tree.
        /// </summary>
        /// <param name="name">The name of this particular node.</param>
        /// <param name="children">The children of this object node.</param>
        public PropertiesObject(string name, IEnumerable<PropertiesTreeNode> children) : this(name)
        {
            foreach (var child in children)
                Add(child);
        }

        /// <summary>
        /// Creates a new object node for a ".properties" document tree.
        /// </summary>
        /// <param name="name">The name of this particular node.</param>
        public PropertiesObject(string name) : base(name)
        {
            _children = new Dictionary<string, PropertiesTreeNode>();
        }

        /// <summary>
        /// Adds the specified node as a child of this object node.
        /// </summary>
        /// <param name="child">The child node to add.</param>
        /// <returns>The child that was added.</returns>
        /// <exception cref="ArgumentException">A child node with the same name already exists.</exception>
        public PropertiesTreeNode Add(PropertiesTreeNode child)
        {
            _children.Add(child.Name, child);
            return child;
        }

        /// <summary>
        /// Adds a primitive property to this object.
        /// </summary>
        /// <param name="key">The key for the primitive property.</param>
        /// <param name="value">The value for the primitive property.</param>
        /// <returns>The property that was added.</returns>
        /// <exception cref="ArgumentException">A child node with the same name already exists.</exception>
        public PropertiesPrimitive AddProperty(string key, string value) => AddPrimitive(key, value);

        /// <summary>
        /// Adds a primitive property to this object.
        /// </summary>
        /// <param name="key">The key for the primitive property.</param>
        /// <param name="value">The value for the primitive property.</param>
        /// <returns>The property that was added.</returns>
        /// <exception cref="ArgumentException">A child node with the same name already exists.</exception>
        public PropertiesPrimitive AddPrimitive(string key, string value)
        {
            return Add(new PropertiesPrimitive(key, value)) as PropertiesPrimitive;
        }

        /// <summary>
        /// Adds an properties object to this object.
        /// </summary>
        /// <param name="name">The name of the object to add.</param>
        /// <returns>The object that was added.</returns>
        /// <exception cref="ArgumentException">A child node with the same name already exists.</exception>
        public PropertiesObject AddObject(string name)
        {
            return Add(new PropertiesObject(name)) as PropertiesObject;
        }

        /// <summary>
        /// Retrieves the child with the specified name, if available.
        /// </summary>
        /// <param name="name">THe name of the node</param>
        /// <param name="node">The child node with the specified name.</param>
        /// <returns>true if this object contains a child element with the given <paramref name="name"/>; false otherwise.</returns>
        public bool TryGetChild(string name, out PropertiesTreeNode? node) => _children.TryGetValue(name, out node);

        /// <summary>
        /// Removes the specified node from the list of children of this object node.
        /// </summary>
        /// <param name="child">The child node to add.</param>
        public bool Remove(PropertiesTreeNode child) => Contains(child) && _children.Remove(child.Name);

        /// <summary>
        /// Checks whether this object node directly contains a child node with the given name.
        /// </summary>
        /// <param name="name">The name of the node to check for.</param>
        /// <returns>true if it contains the node; false otherwise.</returns>
        public bool Contains(string name) => _children.ContainsKey(name);

        /// <summary>
        /// Checks whether this object node contains the underlying node as a direct child.
        /// </summary>
        /// <param name="node">The node to look for.</param>
        /// <returns>true if it contains the node; false otherwise.</returns>
        public bool Contains(PropertiesTreeNode node) => _children.TryGetValue(node.Name, out var child) && child.Equals(node);

        /// <summary>
        /// Clears the children within this composite node.
        /// </summary>
        public void Clear() => _children.Clear();

        /// <summary>
        /// Returns the child with the specified name.
        /// </summary>
        /// <param name="name">The name of the child.</param>
        /// <exception cref="KeyNotFoundException">No direct children with the given name exist within this object node.</exception>
        public PropertiesTreeNode this[string name]
        {
            get => _children[name];
        }

        /// <inheritdoc/>
        public IEnumerator<PropertiesTreeNode> GetEnumerator() => _children.Values.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public override bool Equals(PropertiesTreeNode? node) => node is PropertiesObject obj ? Equals(obj) : base.Equals(node);

        /// <inheritdoc/>
        public bool Equals(PropertiesObject? other)
        {
            return !(other is null) && _children.Count == other._children.Count && !_children.Except(other._children).Any();
        }
    }
}
