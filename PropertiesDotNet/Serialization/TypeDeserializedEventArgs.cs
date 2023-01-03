using System;

using PropertiesDotNet.Core;

namespace PropertiesDotNet.Serialization
{
	/// <summary>
	/// Provides event arguments for type deserialization.
	/// </summary>
	public sealed class TypeDeserializedEventArgs : TypeSerializationEventArgsTemplate
	{
		/// <summary>
		/// Returns the reader where the type was deserialized.
		/// </summary>
		public IPropertiesReader Reader { get; }

		/// <summary>
		/// Creates a new <see cref="TypeDeserializedEventArgs"/>.
		/// </summary>
		/// <param name="reader">The reader where the type was deserialized.</param>
		/// <param name="type">The type that was deserialized</param>
		/// <param name="value">The value of the deserialized type.</param>
		public TypeDeserializedEventArgs(IPropertiesReader reader, Type type, object? value)
			: base(type, value)
		{
			Reader = reader;
		}
	}
}
