using System;

namespace PropertiesDotNet.Serialization
{
    /// <summary>
    /// Represents a (de)serializer for primitive ".properties" types. These converters register specific types as
    /// primitive types and take precedence over object serialization.
    /// </summary>
    /// <remarks>
    /// A primitive value is a textual value that can be written
    /// as either a key or a value and does not need to be (de)composed into a tree of properties.
    /// </remarks>
    public interface IPropertiesPrimitiveConverter
    {
        /// <summary>
        /// Returns whether this converter can convert the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>true if the specified <paramref name="type"/> can be converted by the converter; false otherwise.</returns>
        bool Accepts(Type type);

        /// <summary>
        /// Deserializes the given primitive <paramref name="input"/> as the <paramref name="type"/>. 
        /// </summary>
        /// <param name="serializer">The serializer which is requesting deserialization.</param>
        /// <param name="type">The type to intepret the <paramref name="input"/> as.</param>
        /// <param name="input">The textual value to deserialize. This represents the key or value from the primitive property.</param>
        /// <returns>The deserialized value.</returns>
        object? Deserialize(PropertiesSerializer serializer, Type type, string? input);

        /// <summary>
        /// Serializes the given primitive <paramref name="input"/> as the <paramref name="type"/>.
        /// </summary>
        /// <param name="serializer">The serializer which is requesting serialization.</param>
        /// <param name="type">The type to intepret the <paramref name="input"/> as.</param>
        /// <param name="input">The input object to serialize.</param>
        /// <returns>The <paramref name="input"/> as a text value. This translates to the key or value for the primitive property.</returns>
        string? Serialize(PropertiesSerializer serializer, Type type, object? input);
    }
}
