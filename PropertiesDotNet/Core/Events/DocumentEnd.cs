using System;

namespace PropertiesDotNet.Core.Events
{
	/// <summary>
	/// Represents the end of a ".properties" document stream.
	/// </summary>
	public sealed class DocumentEnd : PropertiesEvent, IEquatable<DocumentEnd>
	{
		/// <inheritdoc/>
		public override bool Canonical => false;

		/// <inheritdoc/>
		public override int DepthIncrease => -1;

		/// <summary>
		/// Writes a <see cref="DocumentEnd"/>.
		/// </summary>
		public DocumentEnd() : this(null, null)
		{
			
		}

		/// <summary>
		/// Creates a new <see cref="DocumentEnd"/>.
		/// </summary>
		/// <param name="start">The starting position of this event in the text stream. Null if this
		/// event was created for writing.</param>
		/// <param name="end">The ending position of this event in the text stream. Null if this
		/// event was created for writing.</param>
		public DocumentEnd(StreamMark? start, StreamMark? end) : base(start, end)
		{
			
		}

		/// <summary>
		/// Accepts an <see cref="IEventVisitor"/>.
		/// </summary>
		/// <param name="visitor">The visitor to accept.</param>
		public override void AcceptVisitor(IEventVisitor visitor) => visitor.Visit(this);

		/// <inheritdoc/>
		public override bool Equals(PropertiesEvent? other) => other is DocumentEnd start && Equals(start);

		/// <inheritdoc/>
		public bool Equals(DocumentEnd? other) => !(other is null);
	}
}
