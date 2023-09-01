using System;
using System.Reflection;

namespace PropertiesDotNet.Serialization.ValueProviders
{
    /// <summary>
    /// Represents a provider where field and property values of objects can be changed.
    /// </summary>
    public interface IValueProvider
    {
        /// <summary>
        /// Sets the value of the given <paramref name="field"/> in the <paramref name="target"/>.
        /// </summary>
        /// <param name="target">The target object to assign the value into. Null for static fields.</param>
        /// <param name="field">The field value to assign.</param>
        /// <param name="value">The value to assign to the <paramref name="field"/>.</param>
        void SetValue(object? target, FieldInfo field, object? value);

        /// <summary>
        /// Sets the value of the given <paramref name="property"/> in the <paramref name="target"/>.
        /// </summary>
        /// <param name="target">The target object to assign the value into. Null for static properties.</param>
        /// <param name="property">The property value to assign.</param>
        /// <param name="value">The value to assign to the <paramref name="property"/>.</param>
        void SetValue(object? target, PropertyInfo property, object? value);

        /// <summary>
        /// Returns the value of the given <paramref name="field"/> from the <paramref name="target"/>.
        /// </summary>
        /// <param name="target">The target object to retrieve the value. Null for static fields.</param>
        /// <param name="field">The field value to retrieve.</param>
        /// <returns>The value of the given <paramref name="field"/> from the <paramref name="target"/>.</returns>
        object? GetValue(object? target, FieldInfo field);

        /// <summary>
        /// Returns the value of the given <paramref name="property"/> from the <paramref name="target"/>.
        /// </summary>
        /// <param name="target">The target object to retrieve the value. Null for static properties.</param>
        /// <param name="property">The property value to retrieve.</param>
        /// <returns>The value of the given <paramref name="property"/> from the <paramref name="target"/>.</returns>
        object? GetValue(object? target, PropertyInfo property);
    }
}
