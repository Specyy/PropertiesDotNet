using System;
using System.IO;
using System.Text;

using PropertiesDotNet.Core;

namespace PropertiesDotNet.Serialization
{
    /// <summary>
    /// Provides extensions methods for <see cref="IPropertiesSerializer"/> and <see cref="IPropertiesDeserializer"/>.
    /// </summary>
    public static class PropertiesSerializationExtensions
    {
        // ///////////////////////// SERIALIZATION ///////////////////////// //

        /// <summary>
        /// Serializes the object value.
        /// </summary>
        /// <param name="serializer">The underlying serializer.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="output">The stream to output to.</param>
        /// <param name="type">The type to serialize the value as. A null value indicates that the <paramref name="value"/>'s
        /// type will be used.</param>
        public static void Serialize(this IPropertiesSerializer serializer, object? value, TextWriter output, Type? type = null)
        {
            serializer.Serialize(new PropertiesWriter(output), value, type);
        }

        /// <summary>
        /// Serializes the object value.
        /// </summary>
        /// <param name="serializer">The underlying serializer.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="output">The stream to output to.</param>
        /// <param name="type">The type to serialize the value as. A null value indicates that the <paramref name="value"/>'s
        /// type will be used.</param>
        public static void Serialize(this IPropertiesSerializer serializer, object? value, StringBuilder output, Type? type = null)
        {
            Serialize(serializer, value, new StringWriter(output), type);
        }

        /// <summary>
        /// Serializes the object value.
        /// </summary>
        /// <param name="serializer">The underlying serializer.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="type">The type to serialize the value as. A null value indicates that the <paramref name="value"/>'s
        /// type will be used.</param>
        /// <returns>The serialized data, as a string.</returns>
        public static string Serialize(this IPropertiesSerializer serializer, object? value, Type? type = null)
        {
            StringBuilder output = new StringBuilder();
            Serialize(serializer, value, output, type);
            return output.ToString();
        }

        // ///////////////////////// SERIALIZATION ///////////////////////// //

        // ///////////////////////// DESERIALIZATION ///////////////////////// //

        /// <summary>
        /// Deserializes the value from the given <paramref name="input"/>.
        /// </summary>
        /// <param name="serializer">The underlying serializer.</param>
        /// <param name="input">The stream to read from.</param>
        /// <param name="type">The type to deserialize the input as. A null value indicates that <see cref="object"/>'s
        /// type will be used.</param>
        /// <returns>The deserialized instance.</returns>
        public static object? Deserialize(this IPropertiesSerializer serializer, TextReader input, Type? type = null)
        {
            return serializer.Deserialize(new PropertiesReader(input), type);
        }

        /// <summary>
        /// Deserializes the value from the given <paramref name="input"/>.
        /// </summary>
        /// <param name="serializer">The underlying serializer.</param>
        /// <param name="input">The stream to read from.</param>
        /// <param name="type">The type to deserialize the input as. A null value indicates that <see cref="object"/>'s
        /// type will be used.</param>
        /// <returns>The deserialized instance.</returns>
        public static object? Deserialize(this IPropertiesSerializer serializer, string input, Type? type = null)
        {
            return Deserialize(serializer, new StringReader(input), type);
        }

        /// <summary>
        /// Deserializes a value from the given <paramref name="input"/>.
        /// </summary>
        /// <param name="serializer">The underlying serializer.</param>
        /// <param name="input">The reader to read the content from.</param>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <returns>The deserialized instance.</returns>
        public static T Deserialize<T>(this IPropertiesSerializer serializer, IPropertiesReader input)
        {
            return (T)serializer.Deserialize(input, typeof(T))!;
        }

        /// <summary>
        /// Deserializes the value from the given <paramref name="input"/>.
        /// </summary>
        /// <param name="serializer">The underlying serializer.</param>
        /// <param name="input">The stream to read from.</param>
        /// <returns>The deserialized instance.</returns>
        public static T Deserialize<T>(this IPropertiesSerializer serializer, TextReader input)
        {
            return Deserialize<T>(serializer, new PropertiesReader(input));
        }

        /// <summary>
        /// Deserializes the value from the given <paramref name="input"/>.
        /// </summary>
        /// <param name="serializer">The underlying serializer.</param>
        /// <param name="input">The stream to read from.</param>
        /// <returns>The deserialized instance.</returns>
        public static T Deserialize<T>(this IPropertiesSerializer serializer, string input)
        {
            return Deserialize<T>(serializer, new StringReader(input));
        }
        
        // ///////////////////////// DESERIALIZATION ///////////////////////// //
    }
}
