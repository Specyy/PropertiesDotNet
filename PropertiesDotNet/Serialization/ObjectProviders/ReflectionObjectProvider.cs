using System;
using System.Collections.Generic;
using System.Reflection;

using PropertiesDotNet.Core;
using PropertiesDotNet.Utils;

namespace PropertiesDotNet.Serialization.ObjectProviders
{
    /// <summary>
    /// Represents an <see cref="IObjectProvider"/> that creates objects using reflection.
    /// </summary>
    public sealed class ReflectionObjectProvider : IObjectProvider
    {
        private readonly IEqualityComparer<Type[]> _equalityComparer = new TypeCacheEqualityComparer();
        private readonly Dictionary<Type, Dictionary<Type[], ObjectConstructor>> _ctorCache;

        /// <summary>
        /// Creates a new <see cref="ReflectionObjectProvider"/>.
        /// </summary>
        public ReflectionObjectProvider()
        {
            _ctorCache = new Dictionary<Type, Dictionary<Type[], ObjectConstructor>>((IEqualityComparer<Type>)_equalityComparer);
        }

        /// <inheritdoc/>
        public object Construct(Type type, Type[]? argTypes, object?[]? args)
        {
            if (type.IsAbstract())
                throw new ArgumentException($"Cannot create instance of type: {type?.FullName ?? "null"}");

            if (argTypes is null || argTypes.Length == 0)
            {
                argTypes = (args is null || args.Length == 0) ?
                    Type.EmptyTypes : throw new ArgumentException("No argument types for provided arguments!");
            }

            try
            {
                if (!_ctorCache.TryGetValue(type, out Dictionary<Type[], ObjectConstructor> available) ||
                    !available.TryGetValue(argTypes, out var ctor))
                {
                    ctor = CreateConstructor(type, argTypes, available);
                }

                return ctor(args);
            }
            catch (PropertiesException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PropertiesException($"Could not create instance of type \"{type.FullName}\": {ex.Message}", ex);
            }
        }

        private ObjectConstructor CreateConstructor(Type type, Type[] argTypes, Dictionary<Type[], ObjectConstructor>? available)
        {
            if (available is null)
                _ctorCache.Add(type, available = new Dictionary<Type[], ObjectConstructor>(_equalityComparer));

            ObjectConstructor ctor;

            if (type.IsValueType() && argTypes.Length == 0)
            {
                ctor = (args) => Activator.CreateInstance(type);
            }
            else
            {
                const BindingFlags visibilityFlags = BindingFlags.Public | BindingFlags.Instance;

                ConstructorInfo info = type.GetConstructor(visibilityFlags, argTypes) ??
                    throw new PropertiesException("Could not find constructor with the given argument types!");

                ctor = (args) => info.Invoke(args);
            }

            available.Add(argTypes, ctor);
            return ctor;
        }

        /// <summary>
        /// Clears the cached constructors for the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type constructor to clear.</param>
        public void ClearCache(Type type)
        {
            _ctorCache.Remove(type);
        }

        /// <summary>
        /// Clears all the cached constructors.
        /// </summary>
        public void ClearCache()
        {
            _ctorCache.Clear();
        }
    }
}
