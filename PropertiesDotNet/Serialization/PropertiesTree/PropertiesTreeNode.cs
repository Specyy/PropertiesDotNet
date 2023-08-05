using System;
using System.Collections.Generic;

namespace PropertiesDotNet.Serialization.PropertiesTree
{
    /// <summary>
    /// Represents an element of a ".properties" document tree.
    /// </summary>
    public abstract class PropertiesTreeNode : IEquatable<PropertiesTreeNode>, IEquatable<string>
    {
        /// <summary>
        /// A list of the comments that will be emitted above this property when it is serialized.
        /// This is <see cref="Nullable{T}"/> in order to save memory.
        /// </summary>
        public List<string>? Comments { get; set; }

        /// <summary>
        /// The name of this particular node.
        /// </summary>
        public virtual string Name { get; protected set; }

        /// <summary>
        /// Creates a new <see cref="PropertiesTreeNode"/>
        /// </summary>
        /// <param name="name">The name of this particular node.</param>
        public PropertiesTreeNode(string name) => Name = name;

        /// <summary>
        /// Checks whether this node is equal to the underlying node.
        /// </summary>
        /// <param name="other">The underlying node.</param>
        /// <returns>Whether this node is equal to the underlying node.</returns>
        public virtual bool Equals(PropertiesTreeNode? other) => Equals(other?.Name);

        /// <summary>
        /// Checks whether the name for this node is equal to the given string.
        /// </summary>
        /// <param name="other">The string to compare to.</param>
        /// <returns>Whether the name for this node is equal to the given string.</returns>
        public virtual bool Equals(string? other) => Name.Equals(other);

        /// <inheritdoc/>
        public override string ToString() => Name;
    }
}
