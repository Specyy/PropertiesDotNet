﻿using System;
using System.Collections.Generic;
using System.Reflection;

using PropertiesDotNet.Utils;

namespace PropertiesDotNet.Serialization.ObjectProviders
{
    /// <summary>
    /// Represents an object constructor.
    /// </summary>
    /// <param name="args">The constructor arguments.</param>
    /// <returns>The constructed object.</returns>
    internal delegate object ObjectConstructor(object?[]? args);

    /// <summary>
    /// Represents a factory or provider where specific object types can be created.
    /// </summary>
    /// <remarks>Object providers are only responsible for the creation of non-serializable types.</remarks>
    public interface IObjectProvider
    {
        /// <summary>
        /// The binding flags for constructors.
        /// </summary>
        BindingFlags ConstructorFlags { get; set; }

        /// <summary>
        /// Creates a new instance of the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to construct.</param>
        /// <param name="argTypes">The argument types.</param>
        /// <param name="args">The constructor arguments.</param>
        /// <returns>A new instance of the specified <paramref name="type"/>. This instance is empty.</returns>
        object Construct(Type type, Type[]? argTypes, object?[]? args);
    }

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

            for (int i = 0; i < argTypes.Length; i++)
                argTypes[i] = args[i]?.GetType() ?? typeof(object);

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
        public static T Construct<T>(this IObjectProvider provider, params object?[]? args) where T : notnull => (T)provider.Construct(typeof(T), args);

        /// <summary>
        /// Creates a new instance of the specified <typeparamref name="T"/> using the default constructor.
        /// </summary>
        /// <param name="provider">The object provider.</param>
        /// <typeparam name="T">The type to construct.</typeparam>
        /// <returns>A new instance of the specified <typeparamref name="T"/>. This instance is empty.</returns>
        public static T Construct<T>(this IObjectProvider provider) where T : notnull => (T)provider.Construct(typeof(T), null, null);
    }

    /// <summary>
    /// Comparer for registered types and constructors in <see cref="IObjectProvider"/>s.
    /// </summary>
    internal sealed class TypeCacheEqualityComparer : IEqualityComparer<Type>, IEqualityComparer<Type[]>
    {
        /// <inheritdoc/>
        public bool Equals(Type? x, Type? y) => x?.Equals(y) ?? y is null;

        /// <inheritdoc/>
        public bool Equals(Type[]? x, Type[]? y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            if (x.Length != y.Length)
                return false;

            for (int i = 0; i < x.Length; i++)
            {
                if (!((x[i]?.Equals(y[i])) ?? y[i] is null))
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public int GetHashCode(Type obj)
        {
#if NETSTANDARD1_3
            return obj.TypeHandle.GetHashCode();
#else
            return obj.GetHashCode();
#endif
        }

        /// <inheritdoc/>
        public int GetHashCode(Type[] obj) => HashCodeHelper.GenerateHashCode(obj);
    }
}