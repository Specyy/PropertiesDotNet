using System;

#if !NETSTANDARD1_3
using System.Runtime.Serialization;
#endif

namespace PropertiesDotNet.Core.Events
{
	/// <summary>
	/// Represents a value in a ".properties" document.
	/// </summary>
#if !NETSTANDARD1_3
	[Serializable]
#endif
	public class Value : Text, IEquatable<Value>
	{
		/// <inheritdoc/>
		public override bool Canonical => true;

#if !NETSTANDARD1_3
		/// <summary>
		/// Creates a new <see cref="Value"/>.
		/// </summary>
		/// <param name="info">The serialization info.</param>
		/// <param name="context">The context.</param>
		public Value(SerializationInfo info, StreamingContext context) : base(info, context)
		{

		}
#endif
		/// <summary>
		/// Writes a <see cref="Value"/>.
		/// </summary>
		/// <param name="value">The text representing this value.</param>
		public Value(string? value) : this(null, null, value)
		{
		}

		/// <summary>
		/// Creates a new <see cref="Value"/>.
		/// </summary>
		/// <param name="start">The starting position of this event in the text stream. Null if this
		/// event was created for writing.</param>
		/// <param name="end">The ending position of this event in the text stream. Null if this
		/// event was created for writing.</param>
		/// <param name="value">The text representing this value.</param>
		public Value(StreamMark? start, StreamMark? end, string? value) : base(start, end, value)
		{
			
		}

		/// <summary>
		/// Accepts an <see cref="IEventVisitor"/>.
		/// </summary>
		/// <param name="visitor">The visitor to accept.</param>
		public override void AcceptVisitor(IEventVisitor visitor) => visitor.Visit(this);

		/// <summary>
		/// Writes a <see cref="Value"/>.
		/// </summary>
		/// <param name="value">The value for this <see cref="Value"/>.</param>
		public static implicit operator Value(string? value)
		{
			return new Value(value);
		}

		/// <inheritdoc/>
		public bool Equals(Value? other) => base.Equals(other);
	}
}
