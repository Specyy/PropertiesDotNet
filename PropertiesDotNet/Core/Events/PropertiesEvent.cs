using System;

namespace PropertiesDotNet.Core.Events
{
    /// <summary>
    /// Represents the smallest meaningful unit of document information. Each event describes
    /// one document function, node, or token.
    /// </summary>
    public abstract class PropertiesEvent : IEquatable<PropertiesEvent>
    {
        /// <summary>
        /// Returns whether this event is canonical to a ".properties" document.
        /// </summary>
        public abstract bool Canonical { get; }

        /// <summary>
        /// Returns the number of depth or nesting levels that this event causes.
        /// </summary>
        public abstract int DepthIncrease { get; }

        /// <summary>
        /// The position in the stream where the event starts, if the event was parsed.
        /// </summary>
        public virtual StreamMark? Start { get; }

        /// <summary>
        /// The position in the stream where the event ends, if the event was parsed.
        /// </summary>
        public virtual StreamMark? End { get; }

        /// <summary>
        /// Creates a new <see cref="PropertiesEvent"/>.
        /// </summary>
        /// <param name="start">The starting position of this event in the text stream. Null if this
        /// event was created for writing.</param>
        /// <param name="end">The ending position of this event in the text stream. Null if this
        /// event was created for writing.</param>
        internal PropertiesEvent(StreamMark? start, StreamMark? end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
		/// Accepts an <see cref="IEventVisitor"/>.
		/// </summary>
		/// <param name="visitor">The visitor to accept.</param>
		public virtual void AcceptVisitor(IEventVisitor visitor) => visitor.Visit(this);

        /// <inheritdoc/>
        public abstract bool Equals(PropertiesEvent? other);
    }
}
