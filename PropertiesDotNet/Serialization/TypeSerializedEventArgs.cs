using System;

using PropertiesDotNet.Core;

namespace PropertiesDotNet.Serialization
{
	/// <summary>
	/// Provides event arguments for type serialization.
	/// </summary>
	public sealed class TypeSerializedEventArgs : TypeSerializationEventArgsTemplate
	{
		/// <summary>
		/// Returns the writer where the type was serialized.
		/// </summary>
		public IPropertiesWriter Writer { get; }

		/// <summary>
		/// Creates a <see cref="TypeSerializedEventArgs"/>.
		/// </summary>
		/// <param name="writer">The writer where the type was serialized</param>
		/// <param name="type">The type that was serialized</param>
		/// <param name="value">The value of the serialized type.</param>
		public TypeSerializedEventArgs(IPropertiesWriter writer, Type type, object? value)
			: base(type, value)
		{
			Writer = writer;
		}
	}
}
