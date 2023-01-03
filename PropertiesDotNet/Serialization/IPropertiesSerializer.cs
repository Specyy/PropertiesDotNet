using System;

using PropertiesDotNet.Core;

namespace PropertiesDotNet.Serialization
{
	/// <summary>
	/// Delegate called when a type is serialized.
	/// </summary>
	/// <param name="serializer">The serializer that serialized the type.</param>
	/// <param name="args">The event arguments.</param>
	public delegate void TypeSerialized(IPropertiesSerializer serializer, TypeSerializedEventArgs args);

	/// <summary>
	/// Delegate called when a type is deserialized.
	/// </summary>
	/// <param name="serializer">The serializer that deserialized the type.</param>
	/// <param name="args">The event arguments.</param>
	public delegate void TypeDeserialized(IPropertiesSerializer serializer, TypeDeserializedEventArgs args);

	/// <summary>
	/// Represents an interface that serializes and deserializes objects to and from a ".properties" document.
	/// </summary>
	public interface IPropertiesSerializer
	{
		/// <summary>
		/// Event raised when a type serialized inside a document.
		/// </summary>
		event TypeSerialized? TypeSerialized;

		/// <summary>
		/// Event raised when a type is deserialized from a document.
		/// </summary>
		event TypeDeserialized? TypeDeserialized;

		/// <summary>
		/// Serializes the object value.
		/// </summary>
		/// <param name="output">The writer to output the value.</param>
		/// <param name="value">The value to serialize.</param>
		/// <param name="type">The type to serialize the value as. A null value indicates that the <paramref name="value"/>'s
		/// type will be used.</param>
		void Serialize(IPropertiesWriter output, object? value, Type? type = null);

		/// <summary>
		/// Deserializes a value from the given <paramref name="input"/>.
		/// </summary>
		/// <param name="input">The reader to read the content from.</param>
		/// <param name="type">The type to deserialize the input as. A null value indicates that <see cref="object"/>'s
		/// type will be used.</param>
		/// <returns>The deserialized instance.</returns>
		object? Deserialize(IPropertiesReader input, Type? type = null);
	}
}
