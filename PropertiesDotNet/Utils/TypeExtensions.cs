using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Globalization;

//
// hello.example.world = 5
// second."example.world" = 5
// hello.example.world" = zook

// type, reader
// key = reader.Read();
// reader.Skip();
// value = reader.Read();
//
// 
// type, key, value
// if(!(type is dictionary))
//      return;
//
// if(key.contains('.') && type.value is dictionary){
//      Dictionary<type> t();
//      t.Add(primitive_parser(type.key, key.substring('first .')), nested(type.value, key.substring()));
//      return t;
//  } else {
//      Dictionary<type> t(); // retrieve already made dictionary
//      t.Add(primitive_parser(type.key, key), primitive_parser(type.value, value));
//  }
//
// primitive_parser(type, text)
//


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

            for(var i = 0; i < ctors.Length; i++)
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

        //public static object? ConvertType(object? value, Type type, IObjectProvider objectProvider)
        //{
        //    // Handle null and DBNull
        //    if (value == null || value.IsDbNull())
        //    {
        //        return type.IsValueType() ? Activator.CreateInstance(type) : null;
        //    }

        //    Type sourceType = value.GetType();

        //    // If the source type is compatible with the destination type, no conversion is needed
        //    if (type == sourceType || type.IsAssignableFrom(sourceType))
        //    {
        //        return value;
        //    }

        //    // Nullable types get a special treatment
        //    if (type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        //    {
        //        Type innerType = type.GetGenericArguments()[0];
        //        Type convertedValue = ConvertType(value, innerType, objectProvider);
        //        return objectProvider.Construct(type, new object?[] { convertedValue });
        //    }

        //    // Enums also require special handling
        //    if (type.IsEnum())
        //    {
        //        return Enum.Parse(type, value.ToString(), true);
        //    }

        //    // Special case for booleans to support parsing "1" and "0". This is
        //    // necessary for compatibility with XML Schema.
        //    if (type == typeof(bool))
        //    {
        //        if ("0".Equals(value))
        //        {
        //            return false;
        //        }

        //        if ("1".Equals(value))
        //        {
        //            return true;
        //        }
        //    }

        //    // Try with the source type's converter
        //    TypeConverter sourceConverter = TypeDescriptor.GetConverter(sourceType);
        //    if (!(sourceConverter is null) && sourceConverter.CanConvertTo(type))
        //    {
        //        return sourceConverter.ConvertTo(null, CultureInfo.InvariantCulture, value, type);
        //    }

        //    // Try with the destination type's converter
        //    TypeConverter destinationConverter = TypeDescriptor.GetConverter(type);
        //    if (!(destinationConverter is null) && destinationConverter.CanConvertFrom(sourceType))
        //    {
        //        return destinationConverter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
        //    }

        //    // Try to find a casting operator in the source or destination type
        //    Type[] types = new[] { sourceType, type };
        //    foreach (Type currentType in types)
        //    {
        //        MethodInfo[] publicStaticMethods = currentType.GetPublicStaticMethods();
        //        foreach (MethodInfo method in publicStaticMethods)
        //        {
        //            bool isCastingOperator = method.IsSpecialName &&
        //                (method.Name == "op_Implicit" || method.Name == "op_Explicit") &&
        //                type.IsAssignableFrom(method.ReturnParameter.ParameterType);

        //            if (isCastingOperator)
        //            {
        //                ParameterInfo[] parameters = method.GetParameters();

        //                bool isCompatible =
        //                    parameters.Length == 1 &&
        //                    parameters[0].ParameterType.IsAssignableFrom(sourceType);

        //                if (isCompatible)
        //                {
        //                    try
        //                    {
        //                        return method.Invoke(null, new[] { value });
        //                    }
        //                    catch (TargetInvocationException ex)
        //                    {
        //                        throw ex.Unwrap();
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    // If source type is string, try to find a Parse or TryParse method
        //    if (sourceType == typeof(string))
        //    {
        //        try
        //        {
        //            // Try with - public static T Parse(string, IFormatProvider)
        //            var parseMethod = type.GetPublicStaticMethod("Parse", typeof(string), typeof(IFormatProvider));
        //            if (parseMethod != null)
        //            {
        //                return parseMethod.Invoke(null, new object[] { value, culture });
        //            }

        //            // Try with - public static T Parse(string)
        //            parseMethod = type.GetPublicStaticMethod("Parse", typeof(string));
        //            if (parseMethod != null)
        //            {
        //                return parseMethod.Invoke(null, new object[] { value });
        //            }
        //        }
        //        catch (TargetInvocationException ex)
        //        {
        //            throw ex.Unwrap();
        //        }
        //    }

        //    // Handle TimeSpan
        //    if (type == typeof(TimeSpan))
        //    {
        //        return TimeSpan.Parse((string)ConvertType(value, typeof(string), objectProvider)!);
        //    }

        //    // Default to the Convert class
        //    return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);


        //    // https://github.com/aaubry/YamlDotNet/blob/0bf02fd092a97f49069945177ac8bd16efac84ce/YamlDotNet/Serialization/Utilities/TypeConverter.cs#L129
        //}
    }
}
