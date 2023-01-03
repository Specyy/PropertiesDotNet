using System;

namespace PropertiesDotNet.Core.Events
{
	/// <summary>
	/// Represents the start of a property in a ".properties" document.
	/// </summary>
	public sealed class PropertyStart : PropertiesEvent, IEquatable<PropertyStart>
	{
		/// <inheritdoc/>
		public override bool Canonical => false;

		/// <inheritdoc/>
		public override int DepthIncrease => 1;

		/// <summary>
		/// Writes a <see cref="PropertyStart"/>.
		/// </summary>
		public PropertyStart() : this(null, null)
		{
			
		}

		/// <summary>
		/// Creates a new <see cref="PropertyStart"/>.
		/// </summary>
		/// <param name="start">The starting position of this event in the text stream. Null if this
		/// event was created for writing.</param>
		/// <param name="end">The ending position of this event in the text stream. Null if this
		/// event was created for writing.</param>
		public PropertyStart(StreamMark? start, StreamMark? end) : base(start, end)
		{
			
		}

		/// <summary>
		/// Accepts an <see cref="IEventVisitor"/>.
		/// </summary>
		/// <param name="visitor">The visitor to accept.</param>
		public override void AcceptVisitor(IEventVisitor visitor) => visitor.Visit(this);

		/// <inheritdoc/>
		public override bool Equals(PropertiesEvent? other) => other is PropertyStart start && Equals(start);

		/// <inheritdoc/>
		public bool Equals(PropertyStart? other) => !(other is null);
	}
}
