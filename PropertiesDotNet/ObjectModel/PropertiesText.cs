using PropertiesDotNet.Core;

using System;

namespace PropertiesDotNet.ObjectModel
{
    /// <summary>
    /// Represents a textual node in a ".properties" document.
    /// </summary>
    public abstract class PropertiesText : PropertiesNode, IEquatable<PropertiesText>, IEquatable<string>
    {
        /// <summary>
        /// Deserializes a <see cref="PropertiesText"/> from the given <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">The reader with the node data.</param>
        protected PropertiesText(IPropertiesReader reader) : base(reader)
        {

        }

        /// <summary>
        /// Creates a new <see cref="PropertiesText"/>.
        /// </summary>
        /// <param name="start">The starting position of this node in the text stream. Null if this
        /// event was created for writing.</param>
        /// <param name="end">The ending position of this node in the text stream. Null if this
        /// event was created for writing.</param>
        public PropertiesText(StreamMark? start, StreamMark? end) : base(start, end)
        {
        }

        /// <inheritdoc/>
        public override bool Equals(PropertiesNode? other) => other is PropertiesText text && Equals(text);

        /// <inheritdoc/>
        public virtual bool Equals(PropertiesText? other) => Equals(other?.ToString());

        /// <inheritdoc/>
        public virtual bool Equals(string? other) => ToString()?.Equals(other) ?? other is null;

        /// <summary>
        /// Returns a string representation of this <see cref="PropertiesText"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="PropertiesText"/>.</returns>
        public abstract override string? ToString();
    }
}
