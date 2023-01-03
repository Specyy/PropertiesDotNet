using System;
using System.Collections.Generic;
using System.Text;

namespace PropertiesDotNet.Serialization.ObjectProviders
{
    /// <summary>
    /// Provides extension methods for an <see cref="IObjectProvider"/>.
    /// </summary>
    public static class ObjectProviderExtensions
    {
        /// <summary>
        /// Creates a new instance of the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="provider">The object provider.</param>
        /// <param name="type">The type to construct.</param>
        /// <param name="args">The constructor arguments.</param>
        /// <returns>A new instance of the specified <paramref name="type"/>. This instance is empty.</returns>
        public static object Construct(this IObjectProvider provider, Type type, object?[]? args)
        {
            Type[] argTypes = new Type[args?.Length ?? 0];

            for (var i = 0; i < argTypes.Length; i++)
            {
                argTypes[i] = args[i]?.GetType() ?? typeof(object);
            }

            return provider.Construct(type, argTypes, args);
        }

        /// <summary>
        /// Creates a new instance of the specified <paramref name="type"/> using the default constructor.
        /// </summary>
        /// <param name="provider">The object provider.</param>
        /// <param name="type">The type to construct.</param>
        /// <returns>A new instance of the specified <paramref name="type"/>. This instance is empty.</returns>
        public static object Construct(this IObjectProvider provider, Type type) => provider.Construct(type, null, null);

        /// <summary>
        /// Creates a new instance of the specified <typeparamref name="T"/>.
        /// </summary>
        /// <param name="provider">The object provider.</param>
        /// <param name="argTypes">The argument types.</param>
        /// <param name="args">The constructor arguments.</param>
        /// <typeparam name="T">The type to construct.</typeparam>
        /// <returns>A new instance of the specified <typeparamref name="T"/>. This instance is empty.</returns>
        public static T Construct<T>(this IObjectProvider provider, Type[] argTypes, object?[]? args) where T : notnull => (T)provider.Construct(typeof(T), argTypes, args);

        /// <summary>
        /// Creates a new instance of the specified <typeparamref name="T"/>.
        /// </summary>
        /// <param name="provider">The object provider.</param>
        /// <param name="args">The constructor arguments.</param>
        /// <typeparam name="T">The type to construct.</typeparam>
        /// <returns>A new instance of the specified <typeparamref name="T"/>. This instance is empty.</returns>
        public static T Construct<T>(this IObjectProvider provider, object?[]? args) where T : notnull => (T)provider.Construct(typeof(T), args);

        /// <summary>
        /// Creates a new instance of the specified <typeparamref name="T"/> using the default constructor.
        /// </summary>
        /// <param name="provider">The object provider.</param>
        /// <typeparam name="T">The type to construct.</typeparam>
        /// <returns>A new instance of the specified <typeparamref name="T"/>. This instance is empty.</returns>
        public static T Construct<T>(this IObjectProvider provider) where T : notnull => (T)provider.Construct(typeof(T), null, null); 
    }
}
