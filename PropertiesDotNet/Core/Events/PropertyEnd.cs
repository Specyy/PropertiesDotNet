using System;

namespace PropertiesDotNet.Core.Events
{
    /// <summary>
    /// Represents the end of a property in a ".properties" document.
    /// </summary>
    public sealed class PropertyEnd : PropertiesEvent, IEquatable<PropertyEnd>
    {
        /// <inheritdoc/>
        public override bool Canonical => false;

        /// <inheritdoc/>
        public override int DepthIncrease => -1;

        /// <summary>
        /// Writes a <see cref="PropertyEnd"/>.
        /// </summary>
        public PropertyEnd() : this(null, null)
        {

        }

        /// <summary>
        /// Creates a new <see cref="PropertyEnd"/>.
        /// </summary>
        /// <param name="start">The starting position of this event in the text stream. Null if this
        /// event was created for writing.</param>
        /// <param name="end">The ending position of this event in the text stream. Null if this
        /// event was created for writing.</param>
        public PropertyEnd(StreamMark? start, StreamMark? end) : base(start, end)
        {

        }

        /// <summary>
		/// Accepts an <see cref="IEventVisitor"/>.
		/// </summary>
		/// <param name="visitor">The visitor to accept.</param>
		public override void AcceptVisitor(IEventVisitor visitor) => visitor.Visit(this);

        /// <inheritdoc/>
        public override bool Equals(PropertiesEvent? other) => other is PropertyEnd end && Equals(end);

        /// <inheritdoc/>
        public bool Equals(PropertyEnd? other) => !(other is null);
    }
}
