using System;

namespace PropertiesDotNet.Core.Events
{
	/// <summary>
	/// Represents an error in a ".properties" document.
	/// </summary>
	public sealed class Error : PropertiesEvent, IEquatable<Error>, IEquatable<string>
	{
		/// <inheritdoc/>
		public override bool Canonical => false;

		/// <inheritdoc/>
		public override int DepthIncrease => 0;

		/// <summary>
		///	The message for the error.
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// Creates a new <see cref="Error"/>.
		/// </summary>
		/// <param name="start">The starting position of this event in the text stream. Null if this
		/// event was created for writing.</param>
		/// <param name="end">The ending position of this event in the text stream. Null if this
		/// event was created for writing.</param>
		/// <param name="message">The error message.</param>
		public Error(StreamMark? start, StreamMark? end, string message) : base(start, end)
		{
			Message = message;
		}

		/// <summary>
		/// Accepts an <see cref="IEventVisitor"/>.
		/// </summary>
		/// <param name="visitor">The visitor to accept.</param>
		public override void AcceptVisitor(IEventVisitor visitor) => visitor.Visit(this);

		/// <inheritdoc/>
		public override bool Equals(PropertiesEvent? other) => other is Error err && Equals(err);

		/// <inheritdoc/>
		public bool Equals(Error? other) => Equals(other?.Message);

		/// <inheritdoc/>
		public bool Equals(string? other) => Message.Equals(other);
	}
}
