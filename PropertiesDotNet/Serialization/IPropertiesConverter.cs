using System;

using PropertiesDotNet.Serialization.PropertiesTree;

namespace PropertiesDotNet.Serialization
{
    /// <summary>
    /// Represents a (de)serializer for non-primitive ".properties" types. These converters serialize objects as a
    /// delimeter-separated tree and only emit primitive values as actual properties.
    /// </summary>
    public interface IPropertiesConverter
    {
        /// <summary>
        /// Returns whether this converter can convert the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>true if the specified <paramref name="type"/> can be converted by the converter; false otherwise.</returns>
        bool Accepts(Type type);

        /// <summary>
        /// Deserializes the property <paramref name="obj"/> as the <paramref name="type"/>. 
        /// </summary>
        /// <param name="serializer">The serializer which is requesting deserialization.</param>
        /// <param name="type">The type to deserialize.</param>
        /// <param name="obj">A tree-like representation of the ".properties" object to deserialize.</param>
        /// <returns>The deserialized value.</returns>
        object? Deserialize(PropertiesSerializer serializer, Type type, PropertiesObject obj);

        /// <summary>
        /// Serializes the object <paramref name="value"/> as the <paramref name="type"/> into
        /// the <paramref name="obj"/>. 
        /// </summary>
        /// <param name="serializer">The serializer which is requesting serialization.</param>
        /// <param name="type">The type to intepret the <paramref name="value"/> as.</param>
        /// <param name="obj">A tree-like representation of the <paramref name="value"/>.</param>
        /// <param name="value">The value to serialize.</param>
        void Serialize(PropertiesSerializer serializer, Type type, object? value, PropertiesObject obj);
    }
}
