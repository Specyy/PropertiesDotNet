using PropertiesDotNet.Core;
using PropertiesDotNet.Utils;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace PropertiesDotNet.Serialization.ObjectProviders
{
    /// <summary>
    /// Represents an <see cref="IObjectProvider"/> that creates objects using IL instructions.
    /// </summary>
    public sealed class DynamicObjectProvider : IObjectProvider
    {
        private const string CONSTRUCTOR_NAME = ".dynamic.ctor";
        private readonly IEqualityComparer<Type[]> _equalityComparer = new TypeCacheEqualityComparer();
        private readonly Dictionary<Type, Dictionary<Type[], ObjectConstructor>> _ctorCache;

        /// <inheritdoc/>
        public BindingFlags ConstructorFlags { get; set; }

        /// <summary>
        /// Creates a new <see cref="DynamicObjectProvider"/>.
        /// </summary>
        public DynamicObjectProvider()
        {
            ConstructorFlags = BindingFlags.Instance | BindingFlags.Public;
            _ctorCache = new Dictionary<Type, Dictionary<Type[], ObjectConstructor>>((IEqualityComparer<Type>)_equalityComparer);
        }

        /// <inheritdoc/>
        public object Construct(Type type, Type[]? argTypes, object?[]? args)
        {
            if (type.IsAbstract())
                throw new ArgumentException($"Cannot create instance of type: {type?.FullName ?? "null"}");

            if ((argTypes?.Length ?? 0) != (args?.Length ?? 0))
                throw new ArgumentException("No argument types for provided arguments!");

            argTypes ??= Type.EmptyTypes;

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

            DynamicMethod method = new DynamicMethod(CONSTRUCTOR_NAME, typeof(object), new[] { typeof(object?[]) });

            EmitConstructorInstructions(method.GetILGenerator(), type, argTypes);

            ObjectConstructor ctor = (ObjectConstructor)method.CreateDelegate(typeof(ObjectConstructor));
            available.Add(argTypes, ctor);
            return ctor;
        }

        private void EmitConstructorInstructions(ILGenerator ilGen, Type type, Type[] argTypes)
        {
            // --- Method Structure ---
            //
            // T = Constructed/Desired object
            // P1_T = Type of parameter 1
            // P2_T = Type of parameter 2
            // P{n}_T = Type of parameter n
            //
            // public object .dynamic.ctor(object?[]? args)
            // {
            //     return new T((P1_T)args[0], (P2_T)args[1], ..., (P{n}_T)args[n-1]);
            // }
            //
            // --- Method Structure ---

            // There is no actual empty ctor for value types
            if (type.IsValueType() && argTypes.Length == 0)
            {
                ilGen.Emit(OpCodes.Ldloca, ilGen.DeclareLocal(type));
                ilGen.Emit(OpCodes.Initobj, type);
                ilGen.Emit(OpCodes.Ldloc_0);
            }
            else
            {
                ConstructorInfo info = type.GetConstructor(ConstructorFlags, argTypes) ??
                    throw new PropertiesException($"Could not find constructor for type {type.FullName} with the given argument types!");

                ParameterInfo[] paramInfo = info.GetParameters();

                // Load params
                for (int i = 0; i < argTypes.Length; i++)
                {
                    // Ldarg_0 = object?[]? args
                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Ldc_I4, i);
                    ilGen.Emit(OpCodes.Ldelem_Ref);

                    Type paramType = paramInfo[i].ParameterType;
                    ilGen.Emit(paramType.IsValueType() ? OpCodes.Unbox_Any : OpCodes.Castclass, paramType);
                }

                ilGen.Emit(OpCodes.Newobj, info);
            }

            if (type.IsValueType())
                ilGen.Emit(OpCodes.Box, type);

            ilGen.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Clears the cached constructors for the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of which the constructors should be cleared.</param>
        public void ClearCache(Type type)
        {
            _ctorCache.Remove(type);
        }

        /// <summary>
        /// Clears the cached constructor for the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of which the constructors should be cleared.</param>
        /// <param name="argTypes">The arguments of the constructor to clear.</param>
        public void ClearCache(Type type, Type[]? argTypes)
        {
            if (_ctorCache.TryGetValue(type, out Dictionary<Type[], ObjectConstructor> available))
            {
                available.Remove(argTypes ?? Type.EmptyTypes);
            }
        }

        /// <summary>
        /// Clears the cached constructors.
        /// </summary>
        public void ClearCache()
        {
            _ctorCache.Clear();
        }
    }
}
