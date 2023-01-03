using PropertiesDotNet.Core;
using PropertiesDotNet.Core.Events;

using System;

namespace PropertiesDotNet.ObjectModel
{
    /// <summary>
    /// Represents an informational node inside a ".properties" document.
    /// </summary>
    public abstract class PropertiesNode : IEventStreamable, IEquatable<PropertiesNode>, IEquatable<PropertiesNodeType>
    {
        /// <summary>
        /// The type of this node, used for simpler comparisons.
        /// </summary>
        public abstract PropertiesNodeType NodeType { get; }

        /// <summary>
        /// The position in the stream where the node starts, if the event was parsed.
        /// </summary>
        public virtual StreamMark? Start { get; protected set; }

        /// <summary>
        /// The position in the stream where the node ends, if the event was parsed.
        /// </summary>
        public virtual StreamMark? End { get; protected set; }

        /// <summary>
        /// Deserializes a <see cref="PropertiesNode"/> from the given <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">The reader with the node data.</param>
        public PropertiesNode(IPropertiesReader reader)
        {

        }

        /// <summary>
        /// Creates a new <see cref="PropertiesNode"/> with the given data.
        /// </summary>
        /// <param name="start">The starting position of this node in the text stream. Null if this
        /// event was created for writing.</param>
        /// <param name="end">The ending position of this node in the text stream. Null if this
        /// event was created for writing.</param>
        protected PropertiesNode(StreamMark? start, StreamMark? end)
        {
            Start = start;
            End = end;
        }

        /// <inheritdoc/>
        public abstract IEventStream ToEventStream();

        /// <inheritdoc/>
        public abstract bool Equals(PropertiesNode? node);

        /// <inheritdoc/>
        public virtual bool Equals(PropertiesNodeType type) => NodeType == type;

        /// <summary>
        /// Returns the hash code for this <see cref="PropertiesNode"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="PropertiesNode"/>.</returns>
        public abstract override int GetHashCode();
    }
}