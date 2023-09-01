using System.Reflection;

namespace PropertiesDotNet.Serialization.ValueProviders
{
    /// <summary>
    /// Represents an <see cref="IValueProvider"/> that assigns values using reflection.
    /// </summary>
    public sealed class ReflectionValueProvider : IValueProvider
    {
        /// <summary>
        /// Creates a new <see cref="ReflectionValueProvider"/>.
        /// </summary>
        public ReflectionValueProvider() { }

        /// <inheritdoc/>
        public object? GetValue(object? target, FieldInfo field)
        {
            return field.GetValue(target);
        }

        /// <inheritdoc/>
        public object? GetValue(object? target, PropertyInfo property)
        {
#if NET35 || NET40
            return property.GetValue(target, null);
#else
            return property.GetValue(target);
#endif
        }

        /// <inheritdoc/>
        public void SetValue(object? target, FieldInfo field, object? value)
        {
            field.SetValue(target, value);
        }

        /// <inheritdoc/>
        public void SetValue(object? target, PropertyInfo property, object? value)
        {
#if NET35 || NET40
            property.SetValue(target, value, null);
#else
            property.SetValue(target, value);
#endif
        }
    }
}
