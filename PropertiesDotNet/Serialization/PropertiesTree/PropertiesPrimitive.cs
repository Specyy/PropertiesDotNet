using System;
using System.Collections.Generic;

using PropertiesDotNet.Utils;

namespace PropertiesDotNet.Serialization.PropertiesTree
{
    /// <summary>
    /// Represents a leaf element of a ".properties" object tree.
    /// </summary>
    public sealed class PropertiesPrimitive : PropertiesTreeNode, IEquatable<PropertiesPrimitive>
    {
        /// <summary>
        /// The value of this primitive property node. The <see cref="PropertiesTreeNode.Name"/> acts as the key.
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Creates a new primitive property node.
        /// </summary>
        /// <param name="key">The key for this property node.</param>
        /// <param name="value">The value for this property node.</param>
        public PropertiesPrimitive(string key, string? value) : base(key)
        {
            Value = value;
        }

        /// <inheritdoc/>
        public bool Equals(PropertiesPrimitive? other) => !(other is null) && other.Name == Name && other.Value == Value;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as PropertiesPrimitive);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCodeHelper.GenerateHashCode(base.GetHashCode(), Value.GetHashCode());
    }
}
