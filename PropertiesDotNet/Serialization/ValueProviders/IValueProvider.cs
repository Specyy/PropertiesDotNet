using System;
using System.Reflection;

namespace PropertiesDotNet.Serialization.ValueProviders
{
    /// <summary>
    /// Represents the function that resolves the given name according to the naming strategy.
    /// </summary>
    /// <param name="name">The property or field name.</param>
    /// <returns>The resolved property name.</returns>
    public delegate string NamingStrategyResolver(string? name);

    /// <summary>
    /// Represents a provider where field and property values of objects can be changed.
    /// </summary>
    public interface IValueProvider
    {
        /// <summary>
        /// The naming strategy function used to successfully search for a value name. 
        /// </summary>
        NamingStrategyResolver NamingStrategyResolver { get; }

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

        /// <summary>
        /// Returns the field with the given <paramref name="fieldName"/>.
        /// </summary>
        /// <param name="type">The type to retrieve the field from.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The field associated with the <paramref name="fieldName"/> from the <paramref name="type"/>.</returns>
        FieldInfo? GetField(Type type, string fieldName);

        /// <summary>
        /// Returns the property with the given <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="type">The type to retrieve the property from.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The property associated with the <paramref name="propertyName"/> from the <paramref name="type"/>.</returns>
        PropertyInfo? GetProperty(Type type, string propertyName);
    }
}
