using System;

namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Delegate that is called when a token is read from a document.
    /// </summary>
    /// <param name="reader">The reader where the token was read.</param>
    /// <param name="token">The token that was read.</param>
    public delegate void TokenRead(IPropertiesReader reader, PropertiesToken token);

    /// <summary>
    /// Represents a ".properties" document reader. An <see cref="IPropertiesReader"/> reads a ".properties"
    /// document in tokens and provides access to them in a stream-like format.
    /// </summary>
    public interface IPropertiesReader : IDisposable
    {
        /// <summary>
        /// Event raised when a token is read from a document.
        /// </summary>
        event TokenRead? TokenRead;

        /// <summary>
        /// The settings for this reader.
        /// </summary>
        PropertiesReaderSettings Settings { get; set; }

        /// <summary>
        /// Returns the current token.
        /// </summary>
        PropertiesToken Token { get; }

        /// <summary>
        /// Represents a marker on the starting position of the current token.
        /// </summary>
        StreamMark? TokenStart { get; }

        /// <summary>
        /// Represents a marker on the ending position of the current token.
        /// </summary>
        StreamMark? TokenEnd { get; }

        /// <summary>
        /// Whether this reader preserves line information.
        /// </summary>
        bool HasLineInfo { get; }

        /// <summary>
        /// Moves to the next token.
        /// </summary>
        /// <returns>Whether there are any tokens left to read.</returns>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        bool MoveNext();
    }

    /// <summary>
    /// Provides extension methods for an <see cref="IPropertiesReader"/>.
    /// </summary>
    public static class PropertiesReaderExtensions
    {
        /// <summary>
        /// Reads the current token, or the next token if the reader is at the start state.
        /// </summary>
        /// <param name="reader">The underlying reader.</param>
        /// <returns>The token that was read</returns>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        public static PropertiesToken Read(this IPropertiesReader reader)
        {
            var token = reader.Token;
            return reader.MoveNext() && token.Type == PropertiesTokenType.None ? reader.Read() : token;
        }

        /// <summary>
        /// Moves to the next property within the document.
        /// </summary>
        /// <param name="reader">The underlying reader.</param>
        /// <returns>Whether or not the next property was moved to.</returns>
        public static bool MoveToContent(this IPropertiesReader reader)
        {
            do
            {
                switch (reader.Token.Type)
                {
                    case PropertiesTokenType.Key:
                    case PropertiesTokenType.Assigner:
                    case PropertiesTokenType.Value:
                        return true;
                    case PropertiesTokenType.Error:
                        return false;
                    default:
                        break;
                }
            } while (reader.MoveNext());

            return false;
        }

        /// <summary>
        /// Reads the current property, or the next property if the reader is not at a property.
        /// </summary>
        /// <param name="reader">The underlying reader.</param>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        /// <returns>Whether the property could be read.</returns>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        public static bool TryReadProperty(this IPropertiesReader reader, out string? key, out string? value)
            => reader.TryReadProperty(out key, out _, out value);

        /// <summary>
        /// Reads the current property, or the next property if the reader is not at a property.
        /// </summary>
        /// <param name="reader">The underlying reader.</param>
        /// <param name="key">The property key.</param>
        /// <param name="assigner">The property assigner.</param>
        /// <param name="value">The property value.</param>
        /// <returns>Whether the property could be read.</returns>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        public static bool TryReadProperty(this IPropertiesReader reader, out string? key, out char? assigner, out string? value)
        {
            var token = reader.Token;

            if (token.Type == PropertiesTokenType.None)
            {
                reader.MoveNext();
                token = reader.Token;
            }

            if (token.Type == PropertiesTokenType.Key)
            {
                key = token.Text;

                if (reader.MoveNext())
                {
                    token = reader.Token;

                    if (token.Type == PropertiesTokenType.Assigner)
                    {
                        reader.MoveNext();
                        assigner = (token = reader.Token).Text?[0];
                    }

                    if (token.Type == PropertiesTokenType.Value)
                    {
                        value = token.Text;
                        assigner = null;
                        return true;
                    }
                }
                else
                {
                    assigner = null;
                    value = null;
                    return true;
                }
            }

            key = null;
            assigner = null;
            value = null;
            return false;
        }

        /// <summary>
        /// Reads the text value for the current token as a <see langword="byte"/>.
        /// </summary>
        /// <param name="reader">The underlying reader.</param>
        /// <returns>The token value as a <see langword="byte"/>.</returns>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        public static byte ReadByte(this IPropertiesReader reader)
        {
            return byte.Parse(reader.Read().Text);
        }

        /// <summary>
        /// Reads the text value for the current token as a <see langword="sbyte"/>.
        /// </summary>
        /// <param name="reader">The underlying reader.</param>
        /// <returns>The token value as a <see langword="sbyte"/>.</returns>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        public static sbyte ReadSByte(this IPropertiesReader reader)
        {
            return sbyte.Parse(reader.Read().Text);
        }

        /// <summary>
        /// Reads the text value for the current token as a <see langword="ushort"/>.
        /// </summary>
        /// <param name="reader">The underlying reader.</param>
        /// <returns>The token value as a <see langword="ushort"/>.</returns>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        public static ushort ReadUInt16(this IPropertiesReader reader)
        {
            return ushort.Parse(reader.Read().Text);
        }

        /// <summary>
        /// Reads the text value for the current token as a <see langword="short"/>.
        /// </summary>
        /// <param name="reader">The underlying reader.</param>
        /// <returns>The token value as a <see langword="short"/>.</returns>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        public static short ReadInt16(this IPropertiesReader reader)
        {
            return short.Parse(reader.Read().Text);
        }

        /// <summary>
        /// Reads the text value for the current token as a <see langword="uint"/>.
        /// </summary>
        /// <param name="reader">The underlying reader.</param>
        /// <returns>The token value as a <see langword="uint"/>.</returns>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        public static uint ReadUInt32(this IPropertiesReader reader)
        {
            return uint.Parse(reader.Read().Text);
        }

        /// <summary>
        /// Reads the text value for the current token as a <see langword="int"/>.
        /// </summary>
        /// <param name="reader">The underlying reader.</param>
        /// <returns>The token value as a <see langword="int"/>.</returns>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        public static int ReadInt32(this IPropertiesReader reader)
        {
            return int.Parse(reader.Read().Text);
        }

        /// <summary>
        /// Reads the text value for the current token as a <see langword="ulong"/>.
        /// </summary>
        /// <param name="reader">The underlying reader.</param>
        /// <returns>The token value as a <see langword="ulong"/>.</returns>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        public static ulong ReadUInt64(this IPropertiesReader reader)
        {
            return ulong.Parse(reader.Read().Text);
        }

        /// <summary>
        /// Reads the text value for the current token as a <see langword="long"/>.
        /// </summary>
        /// <param name="reader">The underlying reader.</param>
        /// <returns>The token value as a <see langword="long"/>.</returns>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        public static long ReadInt64(this IPropertiesReader reader)
        {
            return long.Parse(reader.Read().Text);
        }

        /// <summary>
        /// Reads the text value for the current token as a <see langword="float"/>.
        /// </summary>
        /// <param name="reader">The underlying reader.</param>
        /// <returns>The token value as a <see langword="float"/>.</returns>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        public static float ReadFloat(this IPropertiesReader reader)
        {
            return float.Parse(reader.Read().Text);
        }

        /// <summary>
        /// Reads the text value for the current token as a <see langword="double"/>.
        /// </summary>
        /// <param name="reader">The underlying reader.</param>
        /// <returns>The token value as a <see langword="double"/>.</returns>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        public static double ReadDouble(this IPropertiesReader reader)
        {
            return double.Parse(reader.Read().Text);
        }

        /// <summary>
        /// Reads the text value for the current token as a <see langword="decimal"/>.
        /// </summary>
        /// <param name="reader">The underlying reader.</param>
        /// <returns>The token value as a <see langword="decimal"/>.</returns>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        public static decimal ReadDecimal(this IPropertiesReader reader)
        {
            return decimal.Parse(reader.Read().Text);
        }

        /// <summary>
        /// Reads the text value for the current token as a <see langword="bool"/>.
        /// </summary>
        /// <param name="reader">The underlying reader.</param>
        /// <returns>The token value as a <see langword="bool"/>.</returns>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        public static bool ReadBool(this IPropertiesReader reader)
        {
            string text = reader.Read().Text;

            if (bool.TryParse(text, out bool value))
                return value;

            return !text.Equals('0') && (text.Equals('1') ? true : throw new ArgumentException(nameof(value)));
        }

        /// <summary>
        /// Reads the text value for the current token as a <see langword="char"/>.
        /// </summary>
        /// <param name="reader">The underlying reader.</param>
        /// <returns>The token value as a <see langword="char"/>.</returns>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        public static char ReadChar(this IPropertiesReader reader)
        {
            return char.Parse(reader.Read().Text);
        }
    }
}
