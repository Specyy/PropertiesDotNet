using PropertiesDotNet.Core;

namespace PropertiesDotNet.Serialization
{
    /// <summary>
    /// Represents a type that controls its own serialization/deserialization into a <see cref="IPropertiesWriter"/> or
    /// <see cref="IPropertiesReader"/>. A type that is <see cref="IPropertiesSerializable"/> must implement a constructor
    /// with a <see cref="IPropertiesReader"/> as the only argument.
    /// </summary>
    public interface IPropertiesSerializable
    {
        /// <summary>
        /// Serializes this instance into the <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The writer to write the document information.</param>
        void Serialize(IPropertiesWriter writer);
    }
}
