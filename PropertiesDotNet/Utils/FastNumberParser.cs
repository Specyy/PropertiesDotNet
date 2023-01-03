using System;
using System.Diagnostics.CodeAnalysis;

namespace PropertiesDotNet.Utils
{
    /// <summary>
    /// Provides functions for the quick conversion of string to number types.
    /// </summary>
    internal static class FastNumberParser
    {
        /// <summary>
        /// Denotes the default decimal separator.
        /// </summary>
        public const char DEFAULT_DECIMAL_SEPARATOR = '.';

        /// <summary>
        /// Attempts to parse the <paramref name="input"/> into a <see cref="byte"/>.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <param name="result">The byte result.</param>
        /// <returns>true if the number could be parsed, false otherwise.</returns>
        public static bool TryParseByte(string input, [MaybeNullWhen(false)] out byte? result)
        {
            var len = input.Length;

            if (len < 1)
            {
                result = null;
                return false;
            }

            result = 0;

            for (var i = 0; i < len; i++)
            {
                var currentChar = input[i];

                if (currentChar >= '0' && currentChar <= '9')
                {
                    var digit = (byte)(currentChar - '0');
                    var nextValue = (ulong)(result * 10 + digit);

                    if (nextValue > byte.MaxValue)
                    {
                        result = null;
                        return false;
                    }

                    result = (byte)nextValue;
                }
                else
                {
                    result = null;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Attempts to parse the <paramref name="input"/> into a <see cref="sbyte"/>.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <param name="result">The sbyte result.</param>
        /// <returns>true if the number could be parsed, false otherwise.</returns>
        public static bool TryParseSByte(string input, [MaybeNullWhen(false)] out sbyte? result)
        {
            var len = input.Length;

            if (len < 1)
            {
                result = null;
                return false;
            }

            result = 0;
            var negative = false;

            for (var i = 0; i < len; i++)
            {
                var currentChar = input[i];

                if (currentChar >= '0' && currentChar <= '9')
                {
                    var digit = (byte)(currentChar - '0');
                    var nextValue = (long)(result * 10 + digit);

                    if (nextValue > sbyte.MaxValue || (negative && -nextValue < sbyte.MinValue))
                    {
                        result = null;
                        return false;
                    }

                    result = (sbyte)nextValue;
                }
                else if (currentChar == '-')
                {
                    if (i != 0)
                    {
                        result = null;
                        return false;
                    }

                    negative = true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }

            result = negative ? (sbyte)-result : result;

            return true;
        }

        /// <summary>
        /// Attempts to parse the <paramref name="input"/> into a <see cref="ushort"/>.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <param name="result">The ushort result.</param>
        /// <returns>true if the number could be parsed, false otherwise.</returns>
        public static bool TryParseUInt16(string input, [MaybeNullWhen(false)] out ushort? result)
        {
            var len = input.Length;

            if (len < 1)
            {
                result = null;
                return false;
            }

            result = 0;

            for (var i = 0; i < len; i++)
            {
                var currentChar = input[i];

                if (currentChar >= '0' && currentChar <= '9')
                {
                    var digit = (byte)(currentChar - '0');
                    var nextValue = (ulong)(result * 10 + digit);

                    if (nextValue > ushort.MaxValue)
                    {
                        result = null;
                        return false;
                    }

                    result = (ushort)nextValue;
                }
                else
                {
                    result = null;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Attempts to parse the <paramref name="input"/> into a <see cref="short"/>.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <param name="result">The short result.</param>
        /// <returns>true if the number could be parsed, false otherwise.</returns>
        public static bool TryParseInt16(string input, [MaybeNullWhen(false)] out short? result)
        {
            var len = input.Length;

            if (len < 1)
            {
                result = null;
                return false;
            }

            result = 0;
            var negative = false;

            for (var i = 0; i < len; i++)
            {
                var currentChar = input[i];

                if (currentChar >= '0' && currentChar <= '9')
                {
                    var digit = (byte)(currentChar - '0');
                    var nextValue = (long)(result * 10 + digit);

                    if (nextValue > short.MaxValue || (negative && -nextValue < short.MinValue))
                    {
                        result = null;
                        return false;
                    }

                    result = (short)nextValue;
                }
                else if (currentChar == '-')
                {
                    if (i != 0)
                    {
                        result = null;
                        return false;
                    }

                    negative = true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }

            result = negative ? (short)-result : result;

            return true;
        }

        /// <summary>
        /// Attempts to parse the <paramref name="input"/> into a <see cref="uint"/>.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <param name="result">The uint result.</param>
        /// <returns>true if the number could be parsed, false otherwise.</returns>
        public static bool TryParseUInt32(string input, [MaybeNullWhen(false)] out uint? result)
        {
            var len = input.Length;

            if (len < 1)
            {
                result = null;
                return false;
            }

            result = 0;

            for (var i = 0; i < len; i++)
            {
                var currentChar = input[i];

                if (currentChar >= '0' && currentChar <= '9')
                {
                    var digit = (byte)(currentChar - '0');
                    var nextValue = (ulong)(result * 10 + digit);

                    if (nextValue > uint.MaxValue)
                    {
                        result = null;
                        return false;
                    }

                    result = (uint)nextValue;
                }
                else
                {
                    if (i == len - 1 && char.ToLowerInvariant(currentChar) == 'u')
                        return true;
                    result = null;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Attempts to parse the <paramref name="input"/> into a hexadecimal <see cref="uint"/>.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <param name="result">The hexadecimal uint result.</param>
        /// <returns>true if the number could be parsed, false otherwise.</returns>
        public static bool TryParseUInt32Hex(string input, [MaybeNullWhen(false)] out uint? result)
        {
            result = 0;
            for (var i = 0; i < input.Length; i++)
            {
                var ch = input[i];

                if (ch >= '0' && ch <= '9')
                {
                    result = (uint)((result << 4) + (ch - '0'));
                }
                else if ((ch >= 'A' && ch <= 'F'))
                {
                    result = (uint)((result << 4) + (ch - 'A') + 10);
                }
                else if ((ch >= 'a' && ch <= 'f'))
                {
                    result = (uint)((result << 4) + (ch - 'a') + 10);
                }
                else
                {
                    result = null;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Attempts to parse the <paramref name="input"/> into an <see cref="int"/>.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <param name="result">The int result.</param>
        /// <returns>true if the number could be parsed, false otherwise.</returns>
        public static bool TryParseInt32(string input, [MaybeNullWhen(false)] out int? result)
        {
            var len = input.Length;

            if (len < 1)
            {
                result = null;
                return false;
            }

            result = 0;
            var negative = false;
            for (var i = 0; i < len; i++)
            {
                var currentChar = input[i];

                if (currentChar >= '0' && currentChar <= '9')
                {
                    var digit = (byte)(currentChar - '0');
                    var nextValue = (long)(result * 10 + digit);

                    if (nextValue > int.MaxValue || (negative && -nextValue < int.MinValue))
                    {
                        result = null;
                        return false;
                    }

                    result = (int)nextValue;
                }
                else if (currentChar == '-')
                {
                    if (i != 0)
                    {
                        result = null;
                        return false;
                    }

                    negative = true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }

            result = negative ? (int)-result : result;

            return true;
        }

        /// <summary>
        /// Attempts to parse the <paramref name="input"/> into a hexadecimal <see cref="int"/>.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <param name="result">The hexadecimal int result.</param>
        /// <returns>true if the number could be parsed, false otherwise.</returns>
        public static bool TryParseInt32Hex(string input, [MaybeNullWhen(false)] out int? result)
        {
            result = 0;
            var negative = false;


            for (var i = 0; i < input.Length; i++)
            {
                var ch = input[i];

                if (ch >= '0' && ch <= '9')
                {
                    result = (result << 4) + (ch - '0');
                }
                else if ((ch >= 'A' && ch <= 'F'))
                {
                    result = (result << 4) + (ch - 'A') + 10;
                }
                else if ((ch >= 'a' && ch <= 'f'))
                {
                    result = (result << 4) + (ch - 'a') + 10;
                }
                else if (i == 0 && ch == '-')
                {
                    negative = true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }

            result = negative ? -result : result;
            return true;
        }

        /// <summary>
        /// Attempts to parse the <paramref name="input"/> into a <see cref="ulong"/>.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <param name="result">The ulong result.</param>
        /// <returns>true if the number could be parsed, false otherwise.</returns>
        public static bool TryParseUInt64(string input, [MaybeNullWhen(false)] out ulong? result)
        {
            var len = input.Length;

            if (len < 1)
            {
                result = null;
                return false;
            }

            result = 0;

            for (var i = 0; i < len; i++)
            {
                var currentChar = input[i];

                if (currentChar >= '0' && currentChar <= '9')
                {
                    var digit = (byte)(currentChar - '0');
                    result = (result * 10 + digit);
                }
                else
                {
                    if (i == len - 2 && char.ToLowerInvariant(currentChar) == 'u'
                        && char.ToLowerInvariant(input[i + 1]) == 'l')
                        return true;
                    result = null;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Attempts to parse the <paramref name="input"/> into a <see cref="long"/>.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <param name="result">The long result.</param>
        /// <returns>true if the number could be parsed, false otherwise.</returns>
        public static bool TryParseInt64(string input, [MaybeNullWhen(false)] out long? result)
        {
            var len = input.Length;

            if (len < 1)
            {
                result = null;
                return false;
            }

            result = 0;
            var negative = false;

            for (var i = 0; i < len; i++)
            {
                var currentChar = input[i];

                if (currentChar >= '0' && currentChar <= '9')
                {
                    var digit = (byte)(currentChar - '0');
                    result = (result * 10 + digit);
                }
                else if (currentChar == '-')
                {
                    if (i != 0)
                    {
                        result = null;
                        return false;
                    }

                    negative = true;
                }
                else
                {
                    if (i == len - 1 && char.ToLowerInvariant(currentChar) == 'l')
                        return true;

                    result = null;
                    return false;
                }
            }

            result = negative ? (long)-result : result;

            return true;
        }

        /// <summary>
        /// Attempts to parse the <paramref name="input"/> into a <see cref="double"/>.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <param name="decimalChar">The character that denotes the start of the decimal.</param>
        /// <param name="result">The double result.</param>
        /// <returns>true if the number could be parsed, false otherwise.</returns>
        public static bool TryParseDouble(string input, char decimalChar, [MaybeNullWhen(false)] out double? result)
        {
            var len = input.Length;

            if (len < 1)
            {
                result = null;
                return false;
            }

            result = 0D;
            var afterDecimal = 0D;
            var divisor = 1UL;

            var negative = false;
            var foundDecimal = false;

            for (var i = 0; i < len; i++)
            {
                var currentChar = input[i];

                if (currentChar >= '0' && currentChar <= '9')
                {
                    var digit = (byte)(currentChar - '0');

                    if (foundDecimal)
                    {
                        afterDecimal = (afterDecimal * 10 + digit);
                        divisor *= 10;
                    }
                    else
                    {
                        result = (double)(result * 10 + digit);
                    }
                }
                else if (currentChar == '-')
                {
                    if (i != 0)
                    {
                        result = null;
                        return false;
                    }

                    negative = true;
                }
                else if (currentChar == decimalChar)
                {
                    if (!foundDecimal)
                        foundDecimal = true;
                    else
                    {
                        result = null;
                        return false;
                    }
                }
                else
                {
                    if (i == len - 1 && char.ToLowerInvariant(currentChar) == 'd')
                        return true;

                    result = null;
                    return false;
                }
            }

            result += afterDecimal / divisor;
            result = negative ? (double)-result! : result;

            return true;
        }

        /// <summary>
        /// Attempts to parse the <paramref name="input"/> into a <see cref="float"/>.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <param name="decimalChar">The character that denotes the start of the decimal.</param>
        /// <param name="result">The float result.</param>
        /// <returns>true if the number could be parsed, false otherwise.</returns>
        public static bool TryParseSingle(string input, char decimalChar, [MaybeNullWhen(false)] out float? result)
        {
            var len = input.Length;

            if (len < 1)
            {
                result = null;
                return false;
            }

            result = 0F;
            var afterDecimal = 0F;
            var divisor = 1UL;

            var negative = false;
            var foundDecimal = false;

            for (var i = 0; i < len; i++)
            {
                var currentChar = input[i];

                if (currentChar >= '0' && currentChar <= '9')
                {
                    var digit = (byte)(currentChar - '0');

                    if (foundDecimal)
                    {
                        afterDecimal = (afterDecimal * 10 + digit);
                        divisor *= 10;
                    }
                    else
                    {
                        result = (float)(result * 10 + digit);
                    }
                }
                else if (currentChar == '-')
                {
                    if (i != 0)
                    {
                        result = null;
                        return false;
                    }

                    negative = true;
                }
                else if (currentChar == decimalChar)
                {
                    if (!foundDecimal)
                        foundDecimal = true;
                    else
                    {
                        result = null;
                        return false;
                    }
                }
                else
                {
                    if (i == len - 1 && char.ToLowerInvariant(currentChar) == 'f')
                        return true;

                    result = null;
                    return false;
                }
            }

            result += afterDecimal / divisor;
            result = negative ? (float)-result! : result;

            return true;
        }

        /// <summary>
        /// Attempts to parse the <paramref name="input"/> into a <see cref="decimal"/>.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <param name="decimalChar">The character that denotes the start of the decimal.</param>
        /// <param name="result">The decimal result.</param>
        /// <returns>true if the number could be parsed, false otherwise.</returns>
        public static bool TryParseDecimal(string input, char decimalChar, [MaybeNullWhen(false)] out decimal? result)
        {
            var len = input.Length;

            if (len < 1)
            {
                result = null;
                return false;
            }

            result = 0m;
            var afterDecimal = 0m;
            var divisor = 1UL;

            var negative = false;
            var foundDecimal = false;

            for (var i = 0; i < len; i++)
            {
                var currentChar = input[i];

                if (currentChar >= '0' && currentChar <= '9')
                {
                    var digit = (byte)(currentChar - '0');

                    if (foundDecimal)
                    {
                        afterDecimal = (afterDecimal * 10 + digit);
                        divisor *= 10;
                    }
                    else
                    {
                        result = (result * 10 + digit);
                    }
                }
                else if (currentChar == '-')
                {
                    if (i != 0)
                    {
                        result = null;
                        return false;
                    }

                    negative = true;
                }
                else if (currentChar == decimalChar)
                {
                    if (!foundDecimal)
                        foundDecimal = true;
                    else
                    {
                        result = null;
                        return false;
                    }
                }
                else
                {
                    if (i == len - 1 && char.ToLowerInvariant(currentChar) == 'm')
                        return true;

                    result = null;
                    return false;
                }
            }

            result += afterDecimal / divisor;
            result = negative ? (decimal)-result! : result;

            return true;
        }

        /// <summary>
        /// Parses the <paramref name="input"/> into a <see cref="byte"/>, without any checks. The 
        /// resulting number may be incorrect if the input is not valid. This is method is faster than trying
        /// to parse the underlying number.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <returns>The byte result.</returns>
        public static byte ParseKnownByte(string input)
        {
            byte result = 0;

            for (var i = 0; i < input.Length; i++)
            {
                result = (byte)(result * 10 + (input[i] - '0'));
            }

            return result;
        }

        /// <summary>
        /// Parses the <paramref name="input"/> into a <see cref="sbyte"/>, without any checks. The 
        /// resulting number may be incorrect if the input is not valid. This is method is faster than trying
        /// to parse the underlying number.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <returns>The sbyte result.</returns>
        public static sbyte ParseKnownSByte(string input)
        {
            sbyte result = 0;
            var negative = false;


            for (var i = 0; i < input.Length; i++)
            {
                var currentChar = input[i];

                if (currentChar == '-')
                {
                    negative = true;
                    continue;
                }

                result = (sbyte)(result * 10 + (currentChar - '0'));
            }

            return negative ? (sbyte)-result : result;
        }

        /// <summary>
        /// Parses the <paramref name="input"/> into a <see cref="ushort"/>, without any checks. The 
        /// resulting number may be incorrect if the input is not valid. This is method is faster than trying
        /// to parse the underlying number.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <returns>The ushort result.</returns>
        public static ushort ParseKnownUInt16(string input)
        {
            ushort result = 0;

            for (var i = 0; i < input.Length; i++)
            {
                result = (ushort)(result * 10 + (input[i] - '0'));
            }

            return result;
        }

        /// <summary>
        /// Parses the <paramref name="input"/> into a <see cref="short"/>, without any checks. The 
        /// resulting number may be incorrect if the input is not valid. This is method is faster than trying
        /// to parse the underlying number.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <returns>The short result.</returns>
        public static short ParseKnownInt16(string input)
        {
            short result = 0;
            var negative = false;

            for (var i = 0; i < input.Length; i++)
            {
                var currentChar = input[i];

                if (currentChar == '-')
                {
                    negative = true;
                    continue;
                }

                result = (short)(result * 10 + (currentChar - '0'));
            }

            return negative ? (short)-result : result;
        }

        /// <summary>
        /// Parses the <paramref name="input"/> into a <see cref="uint"/>, without any checks. The 
        /// resulting number may be incorrect if the input is not valid. This is method is faster than trying
        /// to parse the underlying number.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <returns>The uint result.</returns>
        public static uint ParseKnownUInt32(string input)
        {
            uint result = 0;
            var len = input.Length;

            for (var i = 0; i < len; i++)
            {
                var currentChar = input[i];

                if (i == len - 1 && char.ToLowerInvariant(currentChar) == 'u')
                    break;

                result = (uint)(result * 10 + (currentChar - '0'));
            }

            return result;
        }

        /// <summary>
        /// Parses the <paramref name="input"/> into a <see cref="int"/>, without any checks. The 
        /// resulting number may be incorrect if the input is not valid. This is method is faster than trying
        /// to parse the underlying number. 
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <returns>The int result.</returns>
        public static int ParseKnownInt32(string input)
        {
            var result = 0;
            var negative = false;

            for (var i = 0; i < input.Length; i++)
            {
                var currentChar = input[i];

                if (currentChar == '-')
                {
                    negative = true;
                    continue;
                }

                result = result * 10 + (currentChar - '0');
            }

            return negative ? -result : result;
        }

        /// <summary>
        /// Parses the <paramref name="input"/> into a <see cref="ulong"/>, without any checks. The 
        /// resulting number may be incorrect if the input is not valid. This is method is faster than trying
        /// to parse the underlying number.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <returns>The ulong result.</returns>
        public static ulong ParseKnownUInt64(string input)
        {
            ulong result = 0;
            var len = input.Length;

            for (var i = 0; i < len; i++)
            {
                var currentChar = input[i];

                if (i == len - 2 && char.ToLowerInvariant(currentChar) == 'u'
                    && char.ToLowerInvariant(input[i + 1]) == 'l')
                    break;

                result = result * 10 + (ulong)(currentChar - '0');
            }

            return result;
        }

        /// <summary>
        /// Parses the <paramref name="input"/> into a <see cref="long"/>, without any checks. The 
        /// resulting number may be incorrect if the input is not valid. This is method is faster than trying
        /// to parse the underlying number.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <returns>The long result.</returns>
        public static long ParseKnownInt64(string input)
        {
            long result = 0;
            var negative = false;
            var length = input.Length;

            for (var i = 0; i < length; i++)
            {
                var currentChar = input[i];

                if (currentChar == '-')
                {
                    negative = true;
                    continue;
                }

                if (i == length - 1 && char.ToLowerInvariant(currentChar) == 'l')
                    break;

                result = result * 10 + (currentChar - '0');
            }

            return negative ? -result : result;
        }

        /// <summary>
        /// Parses the <paramref name="input"/> into a <see cref="double"/>, without any checks. The 
        /// resulting number may be incorrect if the input is not valid. This is method is faster than trying
        /// to parse the underlying number.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <param name="decimalChar">The character that denotes the start of the decimal.</param>
        /// <returns>The double result.</returns>
        public static double ParseKnownDouble(string input, char decimalChar = DEFAULT_DECIMAL_SEPARATOR)
        {
            var result = 0D;
            var afterDecimal = 0D;
            var divisor = 1UL;

            var decimals = false;
            var negative = false;

            var length = input.Length;

            for (var i = 0; i < length; i++)
            {
                var currentChar = input[i];

                if (currentChar == '-')
                {
                    negative = true;
                    continue;
                }

                if (currentChar == decimalChar)
                {
                    decimals = true;
                    continue;
                }

                if (i == length - 1 && char.ToLowerInvariant(currentChar) == 'd')
                    break;

                if (decimals)
                {
                    afterDecimal = afterDecimal * 10 + (currentChar - '0');
                    divisor *= 10;
                }
                else
                {
                    result = result * 10 + (currentChar - '0');
                }
            }

            result += afterDecimal / divisor;
            return negative ? -result : result;
        }

        /// <summary>
        /// Parses the <paramref name="input"/> into a <see cref="float"/>, without any checks. The 
        /// resulting number may be incorrect if the input is not valid. This is method is faster than trying
        /// to parse the underlying number.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <param name="decimalChar">The character that denotes the start of the decimal.</param>
        /// <returns>The double result.</returns>
        public static float ParseKnownSingle(string input, char decimalChar = DEFAULT_DECIMAL_SEPARATOR)
        {
            var result = 0F;
            var afterDecimal = 0F;
            var divisor = 1UL;

            var decimals = false;
            var negative = false;

            var length = input.Length;

            for (var i = 0; i < length; i++)
            {
                var currentChar = input[i];

                if (currentChar == '-')
                {
                    negative = true;
                    continue;
                }

                if (currentChar == decimalChar)
                {
                    decimals = true;
                    continue;
                }

                if (i == length - 1 && char.ToLowerInvariant(currentChar) == 'f')
                    break;

                if (decimals)
                {
                    afterDecimal = afterDecimal * 10 + (currentChar - '0');
                    divisor *= 10;
                }
                else
                {
                    result = result * 10 + (currentChar - '0');
                }
            }

            result += afterDecimal / divisor;
            return negative ? -result : result;
        }

        /// <summary>
        /// Parses the <paramref name="input"/> into a <see cref="decimal"/>, without any checks. The 
        /// resulting number may be incorrect if the input is not valid. This is method is faster than trying
        /// to parse the underlying number.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <param name="decimalChar">The character that denotes the start of the decimal.</param>
        /// <returns>The double result.</returns>
        public static decimal ParseKnownDecimal(string input, char decimalChar = DEFAULT_DECIMAL_SEPARATOR)
        {
            var result = 0M;
            var afterDecimal = 0M;
            var divisor = 1UL;

            var decimals = false;
            var negative = false;

            var length = input.Length;

            for (var i = 0; i < length; i++)
            {
                var currentChar = input[i];

                if (currentChar == '-')
                {
                    negative = true;
                    continue;
                }

                if (currentChar == decimalChar)
                {
                    decimals = true;
                    continue;
                }

                if (i == length - 1 && char.ToLowerInvariant(currentChar) == 'm')
                    break;

                if (decimals)
                {
                    afterDecimal = afterDecimal * 10 + (currentChar - '0');
                    divisor *= 10;
                }
                else
                {
                    result = result * 10 + (currentChar - '0');
                }
            }

            result += afterDecimal / divisor;
            return negative ? -result : result;
        }

        /// <summary>
        /// Attempts to parse the <paramref name="input"/> into the <paramref name="typeCode"/>.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <param name="decimalChar">The character that denotes the start of the decimal.</param>
        /// <param name="typeCode">The type code of the number to parse the input as.</param>
        /// <param name="result">The result, as an object.</param>
        /// <returns>true if the number could be parsed; false otherwise.</returns>
        public static bool TryParse(string input, char decimalChar, TypeCode typeCode, [MaybeNullWhen(false)] out object? result)
        {
            switch (typeCode)
            {
                case TypeCode.Byte:
                {
                    var parsed = TryParseByte(input, out var @byte);
                    result = @byte;
                    return parsed;
                }
                case TypeCode.SByte:
                {
                    var parsed = TryParseSByte(input, out var @sbyte);
                    result = @sbyte;
                    return parsed;
                }
                case TypeCode.Int16:
                {
                    var parsed = TryParseInt16(input, out var @short);
                    result = @short;
                    return parsed;
                }
                case TypeCode.UInt16:
                {
                    var parsed = TryParseUInt16(input, out var @ushort);
                    result = @ushort;
                    return parsed;
                }
                case TypeCode.Int32:
                {
                    var parsed = TryParseInt32(input, out var @int);
                    result = @int;
                    return parsed;
                }
                case TypeCode.UInt32:
                {
                    var parsed = TryParseUInt32(input, out var @uint);
                    result = @uint;
                    return parsed;
                }
                case TypeCode.Int64:
                {
                    var parsed = TryParseInt64(input, out var @long);
                    result = @long;
                    return parsed;
                }
                case TypeCode.UInt64:
                {
                    var parsed = TryParseUInt64(input, out var @ulong);
                    result = @ulong;
                    return parsed;
                }
                case TypeCode.Double:
                {
                    var parsed = TryParseDouble(input, decimalChar, out var @double);
                    result = @double;
                    return parsed;
                }
                case TypeCode.Single:
                {
                    var parsed = TryParseSingle(input, decimalChar, out var @single);
                    result = @single;
                    return parsed;
                }
                case TypeCode.Decimal:
                {
                    var parsed = TryParseDecimal(input, decimalChar, out var @decimal);
                    result = @decimal;
                    return parsed;
                }

                default:
                {
                    result = null;
                    return false;
                }
            }
        }

        /// <summary>
        /// Parses the <paramref name="input"/> into the given <paramref name="typeCode"/>, without any checks. The 
        /// resulting number may be incorrect if the input is not valid. This is method is faster than trying
        /// to parse the underlying number.
        /// </summary>
        /// <param name="input">The number as a string.</param>
        /// <param name="decimalChar">The character that denotes the start of the decimal.</param>
        /// <param name="typeCode">The type code of the number to parse the input as.</param>
        /// <param name="result">The result, as an object.</param>
        public static object? ParseKnown(string input, TypeCode typeCode, char? decimalChar = DEFAULT_DECIMAL_SEPARATOR)
        {
            return typeCode switch
            {
                TypeCode.Byte => ParseKnownByte(input),
                TypeCode.SByte => ParseKnownSByte(input),
                TypeCode.Int16 => ParseKnownInt16(input),
                TypeCode.UInt16 => ParseKnownUInt16(input),
                TypeCode.Int32 => ParseKnownInt32(input),
                TypeCode.UInt32 => ParseKnownUInt32(input),
                TypeCode.Int64 => ParseKnownInt64(input),
                TypeCode.UInt64 => ParseKnownUInt64(input),
                TypeCode.Double => ParseKnownDouble(input, (char) (decimalChar!)),
                TypeCode.Single => ParseKnownSingle(input, (char) (decimalChar!)),
                TypeCode.Decimal => ParseKnownDecimal(input, (char) (decimalChar!)),
                _ => null
            };
        }
    }
}