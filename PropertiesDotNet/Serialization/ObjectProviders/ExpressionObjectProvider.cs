﻿using PropertiesDotNet.Core;
using PropertiesDotNet.Utils;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace PropertiesDotNet.Serialization.ObjectProviders
{
    /// <summary>
    /// Represents an <see cref="IObjectProvider"/> that creates objects using expressions.
    /// </summary>
    public sealed class ExpressionObjectProvider : IObjectProvider
    {
        private readonly IEqualityComparer<Type[]> _equalityComparer = new TypeCacheEqualityComparer();
        private readonly Dictionary<Type, Dictionary<Type[], ObjectConstructor>> _ctorCache;

        /// <inheritdoc/>
        public BindingFlags ConstructorFlags { get; set; }

        /// <summary>
        /// Creates a new <see cref="ExpressionObjectProvider"/>.
        /// </summary>
        public ExpressionObjectProvider()
        {
            ConstructorFlags = BindingFlags.Instance | BindingFlags.Public;
            _ctorCache = new Dictionary<Type, Dictionary<Type[], ObjectConstructor>>((IEqualityComparer<Type>)_equalityComparer);
        }

        /// <inheritdoc/>
        public object Construct(Type type, Type[]? argTypes, object?[]? args)
        {
            if (type.IsAbstract() || type.IsInterface())
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
                Expression boxed = Expression.Convert(Expression.New(type), typeof(object));
                Func<object> innerCtor = Expression.Lambda<Func<object>>(boxed).Compile();
                ctor = (args) => innerCtor();
            }
            else
            {
                ConstructorInfo info = type.GetConstructor(ConstructorFlags, argTypes) ??
                    throw new PropertiesException($"Could not find constructor for type {type.FullName} with the given argument types!");

                Expression[] expressionArgs = new Expression[argTypes.Length];
                ParameterExpression constantArgs = Expression.Parameter(typeof(object?[]), "args");

                for (int i = 0; i < expressionArgs.Length; i++)
                {
                    Type argType = argTypes[i] ?? throw new ArgumentNullException(nameof(argTypes));
                    Expression providedArg = Expression.ArrayIndex(constantArgs, Expression.Constant(i));
#if !NET35
                    expressionArgs[i] = argType.IsValueType() ?
                        Expression.Unbox(providedArg, argType) :
                        Expression.Convert(providedArg, argType);
#else
                    expressionArgs[i] = Expression.Convert(providedArg, argType);
#endif
                }

                Expression rawNew = Expression.New(info, expressionArgs);
                Expression newExpression = type.IsValueType() ? Expression.Convert(rawNew, typeof(object)) : rawNew;

                ctor = Expression.Lambda<ObjectConstructor>(newExpression, constantArgs).Compile();
            }

            available.Add(argTypes, ctor);
            return ctor;
        }

        /// <summary>
        /// Clears the cached constructors for the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type constructor to clear.</param>
        public void ClearCache(Type type) => _ctorCache.Remove(type);

        /// <summary>
        /// Clears all the cached constructors.
        /// </summary>
        public void ClearCache() => _ctorCache.Clear();
    }
}
