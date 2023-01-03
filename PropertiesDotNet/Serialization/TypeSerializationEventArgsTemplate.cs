using System;

namespace PropertiesDotNet.Serialization
{
    /// <summary>
    /// Provides event arguments for type serialization/deserialization.
    /// </summary>
    public abstract class TypeSerializationEventArgsTemplate : EventArgs
    {
        /// <summary>
        /// Returns the type that was serialized or deserialized.
        /// </summary>
        public virtual Type Type { get; }

        /// <summary>
        /// Returns the value of the serialized or deserialized type.
        /// </summary>
        public virtual object? Value { get; }

        /// <summary>
        /// Creates a <see cref="TypeSerializationEventArgsTemplate"/>.
        /// </summary>
        /// <param name="type">The type that was serialized or deserialized</param>
        /// <param name="value">The value of the serialized or deserialized type.</param>
        public TypeSerializationEventArgsTemplate(Type type, object? value)
        {
            Type = type;
            Value = value;
        }
    }
}