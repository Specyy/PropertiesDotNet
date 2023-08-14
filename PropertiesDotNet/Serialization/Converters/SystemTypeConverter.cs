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
            return TypeExtensions.GetTypeCode(type) switch
            {
                TypeCode.Boolean => input?.GetType() == typeof(string) ? serializer.DeserializePrimitive<bool>(input.ToString()).ToString() : ((bool?)input)?.ToString(),
                TypeCode.Char => input?.GetType() == typeof(string) ? serializer.DeserializePrimitive<char>(input.ToString()).ToString() : ((char?)input)?.ToString(),
                TypeCode.SByte => input?.GetType() == typeof(string) ? serializer.DeserializePrimitive<sbyte>(input.ToString()).ToString() : ((sbyte?)input)?.ToString(),
                TypeCode.Byte => input?.GetType() == typeof(string) ? serializer.DeserializePrimitive<byte>(input.ToString()).ToString() : ((byte?)input)?.ToString(),
                TypeCode.Int16 => input?.GetType() == typeof(string) ? serializer.DeserializePrimitive<short>(input.ToString()).ToString() : ((short?)input)?.ToString(),
                TypeCode.UInt16 => input?.GetType() == typeof(string) ? serializer.DeserializePrimitive<ushort>(input.ToString()).ToString() : ((ushort?)input)?.ToString(),
                TypeCode.Int32 => input?.GetType() == typeof(string) ? serializer.DeserializePrimitive<int>(input.ToString()).ToString() : ((int?)input)?.ToString(),
                TypeCode.UInt32 => input?.GetType() == typeof(string) ? serializer.DeserializePrimitive<uint>(input.ToString()).ToString() : ((uint?)input)?.ToString(),
                TypeCode.Int64 => input?.GetType() == typeof(string) ? serializer.DeserializePrimitive<long>(input.ToString()).ToString() : ((long?)input)?.ToString(),
                TypeCode.UInt64 => input?.GetType() == typeof(string) ? serializer.DeserializePrimitive<ulong>(input.ToString()).ToString() : ((ulong?)input)?.ToString(),
                TypeCode.Single => input?.GetType() == typeof(string) ? serializer.DeserializePrimitive<float>(input.ToString()).ToString() : ((float?)input)?.ToString(),
                TypeCode.Double => input?.GetType() == typeof(string) ? serializer.DeserializePrimitive<double>(input.ToString()).ToString() : ((double?)input)?.ToString(),
                TypeCode.Decimal => input?.GetType() == typeof(string) ? serializer.DeserializePrimitive<decimal>(input.ToString()).ToString() : ((decimal?)input)?.ToString(),
                TypeCode.String => input?.ToString(),
                TypeCode.DateTime => input?.GetType() == typeof(string) ? serializer.DeserializePrimitive<DateTime>(input.ToString()).ToString() : ((DateTime?)input)?.ToString(),
                _ => type == typeof(Guid) ? ((Guid?)input)?.ToString() : throw new PropertiesException($"Cannot serialize primitive type: {type.FullName}"),
            };
        }
    }
}
