using System;

namespace PropertiesDotNet.Serialization.Converters
{
    /// <summary>
    /// A converter for <see cref="TimeSpan"/>.
    /// </summary>
    public sealed class TimeSpanConverter : IPropertiesPrimitiveConverter
    {
        /// <summary>
        /// Creates a new <see cref="TimeSpanConverter"/>.
        /// </summary>
        public TimeSpanConverter()
        {
        }

        /// <inheritdoc />
        public bool Accepts(Type type) => type == typeof(TimeSpan);

        /// <inheritdoc/>
        public object Deserialize(PropertiesSerializer serializer, Type type, string? input)
        {
            if (input?.Contains(":") ?? false)
                return TimeSpan.Parse(input.Trim());

            return TimeSpan.FromTicks(long.Parse(input?.Trim()));
        }

        /// <inheritdoc/>
        public string Serialize(PropertiesSerializer serializer, Type type, object? input)
        {
            return ((TimeSpan)input!).Ticks.ToString();
        }
    }
}