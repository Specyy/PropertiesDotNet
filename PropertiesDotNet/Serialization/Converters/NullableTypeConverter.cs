using System;
using PropertiesDotNet.Utils;

using PropertiesDotNet.Serialization.PropertiesTree;

namespace PropertiesDotNet.Serialization.Converters
{
    /// <summary>
    /// A converter for <see cref="Nullable{T}"/> types.
    /// </summary>
    public sealed class NullableTypeConverter : IPropertiesConverter, IPropertiesPrimitiveConverter
    {
        /// <summary>
        /// Creates a new <see cref="NullableTypeConverter"/>.
        /// </summary>
        public NullableTypeConverter() { }

        /// <inheritdoc/>
        public bool Accepts(Type type) => !(Nullable.GetUnderlyingType(type) is null);

        /// <inheritdoc/>
        public object? Deserialize(PropertiesSerializer serializer, Type type, PropertiesObject obj)
        {
            return serializer.DeserializeObject(Nullable.GetUnderlyingType(type), obj);
        }

        /// <inheritdoc/>
        public object? Deserialize(PropertiesSerializer serializer, Type type, string? input)
        {
            return serializer.DeserializePrimitive(Nullable.GetUnderlyingType(type), input);
        }

        /// <inheritdoc/>
        public void Serialize(PropertiesSerializer serializer, Type type, object? value, PropertiesObject obj)
        {
            serializer.SerializeObject(Nullable.GetUnderlyingType(type), value, obj);
        }

        /// <inheritdoc/>
        public string? Serialize(PropertiesSerializer serializer, Type type, object? input)
        {
            return serializer.SerializePrimitive(Nullable.GetUnderlyingType(type), input);
        }
    }
}
