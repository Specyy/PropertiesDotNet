using System;
using System.Collections.Generic;
using System.Text;

namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Delegate that is called when a <see cref="PropertiesToken"/> is read from a stream.
    /// </summary>
    /// <param name="reader">The reader where the token was read.</param>
    /// <param name="token">The token that was read.</param>
    public delegate void TokenRead(IPropertiesReader reader, PropertiesToken token);

    /// <summary>
    /// Represents a .properties document reader. A <see cref="IPropertiesReader"/> reads a .properties
    /// document in tokens and provides access to them in a stream-like format.
    /// </summary>
    public interface IPropertiesReader : IDisposable
    {
        /// <summary>
        /// Event raised when a <see cref="PropertiesToken"/> is read from a document.
        /// </summary>
        event TokenRead? TokenRead;

        /// <summary>
        /// The settings for this reader.
        /// </summary>
        PropertiesReaderSettings Settings { get; }

        /// <summary>
        /// Returns the current token.
        /// </summary>
        PropertiesToken Token { get; }

        /// <summary>
        /// Moves to the next token.
        /// </summary>
        /// <returns>Whether there are any tokens left to read.</returns>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        bool MoveNext();
    }
}
