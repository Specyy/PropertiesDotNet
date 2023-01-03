using System;

namespace PropertiesDotNet.Core.Events
{
    /// <summary>
    /// Represents the start of a ".properties" document stream.
    /// </summary>
    public sealed class DocumentStart : PropertiesEvent, IEquatable<DocumentStart>
    {
        /// <inheritdoc/>
        public override bool Canonical => false;

        /// <inheritdoc/>
        public override int DepthIncrease => 1;

        /// <summary>
        /// Writes a <see cref="DocumentStart"/>.
        /// </summary>
        public DocumentStart() : this(null, null)
        {

        }

        /// <summary>
        /// Creates a new <see cref="DocumentStart"/>.
        /// </summary>
        /// <param name="start">The starting position of this event in the text stream. Null if this
        /// event was created for writing.</param>
        /// <param name="end">The ending position of this event in the text stream. Null if this
        /// event was created for writing.</param>
        public DocumentStart(StreamMark? start, StreamMark? end) : base(start, end)
        {

        }

        /// <summary>
		/// Accepts an <see cref="IEventVisitor"/>.
		/// </summary>
		/// <param name="visitor">The visitor to accept.</param>
		public override void AcceptVisitor(IEventVisitor visitor) => visitor.Visit(this);

        /// <inheritdoc/>
        public override bool Equals(PropertiesEvent? other) => other is DocumentStart start && Equals(start);

        /// <inheritdoc/>
        public bool Equals(DocumentStart? other) => !(other is null);
    }
}
