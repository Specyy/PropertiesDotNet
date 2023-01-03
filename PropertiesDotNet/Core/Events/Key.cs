using System;
using System.Diagnostics.CodeAnalysis;

#if !NETSTANDARD1_3
using System.Runtime.Serialization;
#endif

namespace PropertiesDotNet.Core.Events
{
    /// <summary>
    /// Represents a key in a ".properties" document.
    /// </summary>
#if !NETSTANDARD1_3
    [Serializable]
#endif
    public class Key : Text, IEquatable<Key>
    {
        /// <inheritdoc/>
        public override bool Canonical => true;

        /// <summary>
        /// Returns the string value of this <see cref="Key"/>.
        /// </summary>
        [NotNull]
        public override string Value => base.Value;

#if !NETSTANDARD1_3
        /// <summary>
        /// Creates a new <see cref="Key"/>.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The context.</param>
        public Key(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            
        }
#endif
        /// <summary>
        /// Writes a <see cref="Key"/>.
        /// </summary>
        /// <param name="value">The key's string value.</param>
        public Key(string value) : this(null, null, value)
        {

        }

        /// <summary>
        /// Creates a new <see cref="Key"/>.
        /// </summary>
        /// <param name="start">The starting position of this event in the text stream. Null if this
        /// event was created for writing.</param>
        /// <param name="end">The ending position of this event in the text stream. Null if this
        /// event was created for writing.</param>
        /// <param name="value">The key's string value.</param>
        public Key(StreamMark? start, StreamMark? end, string value) : base(start, end, value ?? throw new ArgumentNullException(nameof(value)))
        {

        }

        /// <summary>
		/// Accepts an <see cref="IEventVisitor"/>.
		/// </summary>
		/// <param name="visitor">The visitor to accept.</param>
		public override void AcceptVisitor(IEventVisitor visitor) => visitor.Visit(this);

        /// <summary>
        /// Writes a <see cref="Key"/>.
        /// </summary>
        /// <param name="value">The value for this <see cref="Key"/>.</param>
        public static implicit operator Key(string value)
        {
            return new Key(value);
        }

        /// <inheritdoc/>
        public bool Equals(Key? other) => base.Equals(other);
    }
}
