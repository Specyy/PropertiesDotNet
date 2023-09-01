using System;
using System.Collections.Generic;

using PropertiesDotNet.Utils;

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
        /// Adds a comment to this node.
        /// </summary>
        /// <param name="comment">The text value of the comment.</param>
        public virtual void AddComment(string comment)
        {
            Comments ??= new List<string>();
            Comments.Add(comment);
        }

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
        public override bool Equals(object? obj) => Equals(obj as PropertiesTreeNode);

        /// <summary>
        /// Returns the <see cref="Name"/> of this node.
        /// </summary>
        /// <returns>The <see cref="Name"/> of this node</returns>
        public override string ToString() => Name;

        /// <summary>
        /// Returns the hash code for this tree node.
        /// </summary>
        /// <returns>The hash of this tree node.</returns>
        public override int GetHashCode() => HashCodeHelper.GenerateHashCode(GetHashCode(), HashCodeHelper.GenerateHashCode<string>(Comments));
    }
}
