using System;
using System.Globalization;

using PropertiesDotNet.Core;
using PropertiesDotNet.Utils;

namespace PropertiesDotNet.Serialization.Converters
{
    /// <summary>
    /// A primitive converter for system types.
    /// </summary>
    public sealed class SystemTypeConverter : IPropertiesPrimitiveConverter
    {
        /// <inheritdoc/>
        public bool Accepts(Type type)
        {
            if (type.IsEnum())
                return true;

            switch (TypeExtensions.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.String:
                case TypeCode.DateTime:
                    return true;
                default:
                    return type == typeof(Guid);
            }
        }

        /// <inheritdoc/>
        public object? Deserialize(PropertiesSerializer serializer, Type type, string? input)
        {
            if (type.IsEnum())
            {
                return Enum.Parse(Enum.GetUnderlyingType(type), input);
            }

            return TypeExtensions.GetTypeCode(type) switch
            {
                TypeCode.Boolean => bool.Parse(input),
                TypeCode.Char => char.Parse(input),
                TypeCode.SByte => sbyte.Parse(input),
                TypeCode.Byte => byte.Parse(input),
                TypeCode.Int16 => short.Parse(input),
                TypeCode.UInt16 => ushort.Parse(input),
                TypeCode.Int32 => int.Parse(input, NumberStyles.Integer | NumberStyles.AllowExponent),
                TypeCode.UInt32 => uint.Parse(input, NumberStyles.Integer | NumberStyles.AllowExponent),
                TypeCode.Int64 => long.Parse(input, NumberStyles.Integer | NumberStyles.AllowExponent),
                TypeCode.UInt64 => ulong.Parse(input, NumberStyles.Integer | NumberStyles.AllowExponent),
                TypeCode.Single => float.Parse(char.ToUpperInvariant(input[input.Length - 1]) == char.ToUpperInvariant('f') ? input.Substring(0, input.Length - 1) : input, NumberStyles.Float),
                TypeCode.Double => double.Parse(char.ToUpperInvariant(input[input.Length - 1]) == char.ToUpperInvariant('d') ? input.Substring(0, input.Length - 1) : input, NumberStyles.Float),
                TypeCode.Decimal => decimal.Parse(input, NumberStyles.Float | NumberStyles.AllowCurrencySymbol),
                TypeCode.String => input,
                TypeCode.DateTime => DateTime.Parse(input),
                _ => type == typeof(Guid) ?
#if NET35
                new Guid(input)
#else
                Guid.Parse(input)
#endif
                 : throw new PropertiesException($"Cannot deserialize primitive type: {type.FullName} (\"{input}\")"),
            };
        }

        /// <inheritdoc/>
        public string? Serialize(PropertiesSerializer serializer, Type type, object? input)
        {
            switch (TypeExtensions.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.String:
                case TypeCode.DateTime:
                    return input?.ToString() ?? null;
                default:
                    if (type == typeof(Guid))
                        return input?.ToString();

                    throw new PropertiesException($"Cannot serialize primitive type: {type.FullName}");
            }
        }
    }
}
