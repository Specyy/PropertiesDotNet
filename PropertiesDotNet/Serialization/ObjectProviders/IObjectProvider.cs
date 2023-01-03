using System;

namespace PropertiesDotNet.Serialization.ObjectProviders
{
    /// <summary>
    /// Represents an object constructor.
    /// </summary>
    /// <param name="args">The constructor arguments.</param>
    /// <returns>The constructed object.</returns>
    internal delegate object ObjectConstructor(object?[]? args);

    /// <summary>
    /// Represents a provider where specific object types can be created. Object providers are only
    /// responsible for the creation of non-serializable types.
    /// </summary>
    public interface IObjectProvider
    {
        /// <summary>
        /// Creates a new instance of the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to construct.</param>
        /// <param name="argTypes">The argument types.</param>
        /// <param name="args">The constructor arguments.</param>
        /// <returns>A new instance of the specified <paramref name="type"/>. This instance is empty.</returns>
        object Construct(Type type, Type[]? argTypes, object?[]? args);
    }
}
