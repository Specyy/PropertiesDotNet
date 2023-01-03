namespace PropertiesDotNet.Core.Events
{
	/// <summary>
	/// Represents a class or struct that visits events.
	/// </summary>
	public interface IEventVisitor
	{
		/// <summary>
		/// Visits an unknown event type.
		/// </summary>
		/// <param name="event">The event to visit.</param>
		void Visit(PropertiesEvent @event);

		/// <summary>
		/// Visits a <see cref="DocumentStart"/>.
		/// </summary>
		/// <param name="start">The event to visit.</param>
		void Visit(DocumentStart start);

		/// <summary>
		/// Visits a <see cref="Comment"/>.
		/// </summary>
		/// <param name="comment">The event to visit.</param>
		void Visit(Comment comment);

		/// <summary>
		/// Visits a <see cref="PropertyStart"/>.
		/// </summary>
		/// <param name="start">The event to visit.</param>
		void Visit(PropertyStart start);

		/// <summary>
		/// Visits a <see cref="Key"/>.
		/// </summary>
		/// <param name="key">The event to visit.</param>
		void Visit(Key key);

		/// <summary>
		/// Visits a <see cref="ValueAssigner"/>.
		/// </summary>
		/// <param name="assigner">The event to visit.</param>
		void Visit(ValueAssigner assigner);

		/// <summary>
		/// Visits a <see cref="Value"/>.
		/// </summary>
		/// <param name="value">The event to visit.</param>
		void Visit(Value value);

		/// <summary>
		/// Visits a <see cref="PropertyEnd"/>.
		/// </summary>
		/// <param name="end">The event to visit.</param>
		void Visit(PropertyEnd end);

		/// <summary>
		/// Visits an <see cref="Error"/>.
		/// </summary>
		/// <param name="error">The event to visit.</param>
		void Visit(Error error);

		/// <summary>
		/// Visits a <see cref="DocumentEnd"/>.
		/// </summary>
		/// <param name="end">The event to visit.</param>
		void Visit(DocumentEnd end);
	}
}
