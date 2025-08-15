using System;
using System.Globalization;

namespace PropertiesDotNet.Serialization.Converters
{
    /// <summary>
    /// A converter for <see cref="DateTimeOffset"/>.
    /// </summary>
    public sealed class DateTimeOffsetConverter : IPropertiesPrimitiveConverter
    {
        /// <summary>
        /// Creates a new <see cref="DateTimeOffsetConverter"/>.
        /// </summary>
        public DateTimeOffsetConverter()
        {
        }

        /// <inheritdoc />
        public bool Accepts(Type type) => type == typeof(DateTimeOffset);

        /// <inheritdoc/>
        public object Deserialize(PropertiesSerializer serializer, Type type, string? input)
        {
            return DateTimeOffset.ParseExact(input, "o", null, DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite);
        }

        /// <inheritdoc/>
        public string Serialize(PropertiesSerializer serializer, Type type, object? input)
        {
            return ((DateTimeOffset)input!).ToString("o");
        }
    }
}