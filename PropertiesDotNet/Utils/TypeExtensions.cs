using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Globalization;
using PropertiesDotNet.Serialization.ObjectProviders;

namespace PropertiesDotNet.Utils
{
    /// <summary>
    /// Provides extension methods for <see cref="System.Type"/> compatibility.
    /// </summary>
    internal static class TypeExtensions
    {
        public static ConstructorInfo? GetConstructor(this Type type, BindingFlags flags, Type[] args)
        {
#if NETSTANDARD1_3
            ConstructorInfo[] ctors = type.GetConstructors(flags);

            for (var i = 0; i < ctors.Length; i++)
            {
                ConstructorInfo ctor = ctors[i];
                ParameterInfo[] ctorParams = ctor.GetParameters();

                if (ctorParams.Length != args.Length)
                    continue;

                for (var j = 0; j < ctorParams.Length; j++)
                {
                    if (!args[i].Equals(ctorParams[i]))
                        break;

                    if (j == ctorParams.Length - 1)
                        return ctor;
                }
            }

            return null;
#else
            return type.GetConstructor(flags, null, args, null);
#endif
        }

        public static Type? GetGenericInterface(Type type, Type genericInterfaceType)
        {
            foreach (var interfacetype in type.GetInterfaces())
            {
                if (interfacetype.IsGenericType() && interfacetype.GetGenericTypeDefinition() == genericInterfaceType)
                {
                    return interfacetype;
                }
            }
            return null;
        }

        public static bool IsPrimitive(this Type type)
        {
#if NETSTANDARD1_3
            return type.GetTypeInfo().IsPrimitive;
#else
            return type.IsPrimitive;
#endif
        }

        public static IEnumerable<Type> GetInterfaces(this Type type)
        {
#if NETSTANDARD1_3
            return type.GetTypeInfo().ImplementedInterfaces;
#else
            return type.GetInterfaces();
#endif
        }

        public static Type[] GetGenericArguments(this Type type)
        {
#if NETSTANDARD1_3
            return type.GetTypeInfo().GenericTypeArguments;
#else
            return type.GetGenericArguments();
#endif
        }

        public static IEnumerable<T> GetCustomAttributes<T>(MemberInfo member) where T : Attribute
        {
            var atts = member.GetCustomAttributes(typeof(T), true);

            foreach (var att in atts)
                yield return (T)att;
        }

        public static T? GetCustomAttribute<T>(MemberInfo member) where T : Attribute
        {
#if NETSTANDARD1_3
            return (T?)member.GetCustomAttribute(typeof(T));
#else
            return (T?)Attribute.GetCustomAttribute(member, typeof(T));
#endif
        }

        public static bool HasDefaultConstuctor(this Type type)
        {
#if NETSTANDARD1_3
            return type.GetTypeInfo().IsValueType ||
                type.GetTypeInfo().DeclaredConstructors.Any(c => c.IsPublic && !c.IsStatic && c.GetParameters().Length == 0);
#else
            return type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null;
#endif
        }

        public static bool IsAbstract(this Type type)
        {
#if NETSTANDARD1_3
            return type.GetTypeInfo().IsAbstract;
#else
            return type.IsAbstract;
#endif
        }

        public static bool IsValueType(this Type type)
        {
#if NETSTANDARD1_3
            return type.GetTypeInfo().IsValueType;
#else
            return type.IsValueType;
#endif
        }

        public static bool IsGenericType(this Type type)
        {
#if NETSTANDARD1_3
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }

        public static bool IsClass(this Type type)
        {
#if NETSTANDARD1_3
            return type.GetTypeInfo().IsClass;
#else
            return type.IsClass;
#endif
        }

        public static bool IsInterface(this Type type)
        {
#if NETSTANDARD1_3
            return type.GetTypeInfo().IsInterface;
#else
            return type.IsInterface;
#endif
        }

        public static bool IsEnum(this Type type)
        {
#if NETSTANDARD1_3
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        public static TypeCode GetTypeCode(Type? type)
        {
#if !NETSTANDARD1_3

            return Type.GetTypeCode(type);
#else
            if (type.IsEnum())
            {
                type = Enum.GetUnderlyingType(type);
            }

            if (type is null)
            {
                return TypeCode.Empty;
            }

            if (type == typeof(string))
            {
                return TypeCode.String;
            }
            if (type == typeof(char))
            {
                return TypeCode.Char;
            }
            if (type == typeof(bool))
            {
                return TypeCode.Boolean;
            }

            if (type == typeof(sbyte))
            {
                return TypeCode.SByte;
            }
            if (type == typeof(byte))
            {
                return TypeCode.Byte;
            }
            if (type == typeof(short))
            {
                return TypeCode.Int16;
            }
            if (type == typeof(ushort))
            {
                return TypeCode.UInt16;
            }
            if (type == typeof(int))
            {
                return TypeCode.Int32;
            }
            if (type == typeof(uint))
            {
                return TypeCode.UInt32;
            }
            if (type == typeof(long))
            {
                return TypeCode.Int64;
            }
            if (type == typeof(ulong))
            {
                return TypeCode.UInt64;
            }
            if (type == typeof(float))
            {
                return TypeCode.Single;
            }
            if (type == typeof(double))
            {
                return TypeCode.Double;
            }
            if (type == typeof(decimal))
            {
                return TypeCode.Decimal;
            }
            if (type == typeof(DateTime))
            {
                return TypeCode.DateTime;
            }
            return TypeCode.Object;
#endif
        }

        public static bool IsAssignableFrom(this Type type, Type other)
        {
#if NETSTANDARD1_3
            return type.GetTypeInfo().IsAssignableFrom(other.GetTypeInfo());
#else
            return type.IsAssignableFrom(other);
#endif
        }

        public static MethodInfo? GetMethod(this Type type, string name, BindingFlags flags, Type[] argTypes)
        {
#if NETSTANDARD1_3
            var method = type.GetMethod(name, flags);

            if (method != null)
            {
                var methodParams = method.GetParameters();

                if (methodParams.Length == argTypes.Length)
                {
                    for (int i = 0; i < argTypes.Length; i++)
                    {
                        if (!methodParams[i].ParameterType.Equals(argTypes[i]))
                            return null;
                    }

                    return method;
                }
            }

            return null;
#else
            return type.GetMethod(name, flags, null, argTypes, null);
#endif
        }

        public static object? ConvertType(object? value, Type type, IObjectProvider objectProvider)
        {
            // Handle null and DBNull
            if (value is null || value?.GetType()?.FullName == "System.DBNull")
            {
                return type.IsValueType() ? objectProvider.Construct(type) : null;
            }

            Type sourceType = value.GetType();

            // If the source type is compatible with the destination type, no conversion is needed
            if (type == sourceType || type.IsAssignableFrom(sourceType))
            {
                return value;
            }

            // Nullable types get a special treatment
            if (type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                Type innerType = type.GetGenericArguments()[0];
                object convertedValue = ConvertType(value, innerType, objectProvider);
                return objectProvider.Construct(type, new object?[] { convertedValue });
            }

            // Enums also require special handling
            if (type.IsEnum())
            {
                return Enum.Parse(type, value.ToString(), true);
            }

            // Special case for booleans to support parsing "1" and "0".
            if (type == typeof(bool))
            {
                if ("0".Equals(value))
                {
                    return false;
                }

                if ("1".Equals(value))
                {
                    return true;
                }
            }

#if !NETSTANDARD1_3
            // Try with the source type's converter
            TypeConverter sourceConverter = TypeDescriptor.GetConverter(sourceType);
            if (!(sourceConverter is null) && sourceConverter.CanConvertTo(type))
            {
                return sourceConverter.ConvertTo(null, CultureInfo.InvariantCulture, value, type);
            }

            // Try with the destination type's converter
            TypeConverter destinationConverter = TypeDescriptor.GetConverter(type);
            if (!(destinationConverter is null) && destinationConverter.CanConvertFrom(sourceType))
            {
                return destinationConverter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
            }
#endif

            // Try to find a casting operator in the source or destination type
            Type[] types = new[] { sourceType, type };
            foreach (Type currentType in types)
            {
                MethodInfo[] publicStaticMethods = currentType.GetMethods(BindingFlags.Public | BindingFlags.Static);
                foreach (MethodInfo method in publicStaticMethods)
                {
                    bool isCastingOperator = method.IsSpecialName &&
                        (method.Name == "op_Implicit" || method.Name == "op_Explicit") &&
                        type.IsAssignableFrom(method.ReturnParameter.ParameterType);

                    if (isCastingOperator)
                    {
                        ParameterInfo[] parameters = method.GetParameters();

                        bool isCompatible =
                            parameters.Length == 1 &&
                            parameters[0].ParameterType.IsAssignableFrom(sourceType);

                        if (isCompatible)
                        {
                            return method.Invoke(null, new[] { value });
                        }
                    }
                }
            }

            // If source type is string, try to find a Parse or TryParse method
            if (sourceType == typeof(string))
            {

                // Try with - public static T Parse(string, IFormatProvider)
                var parseFunction = type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, new[] { typeof(string), typeof(IFormatProvider) });
                if (parseFunction != null)
                {
                    return parseFunction.Invoke(null, new object[] { value, CultureInfo.InvariantCulture });
                }

                // Try with - public static T Parse(string)
                parseFunction = type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, new[] { typeof(string) });
                if (parseFunction != null)
                {
                    return parseFunction.Invoke(null, new object[] { value });
                }
            }

            // Handle TimeSpan
            if (type == typeof(TimeSpan))
            {
                return TimeSpan.Parse((string)ConvertType(value, typeof(string), objectProvider)!);
            }

            // Default to the Convert class
            return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }
    }
}
