using System;

namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Delegate called when a token is written.
    /// </summary>
    /// <param name="writer">The writer where the token was written.</param>
    /// <param name="token">The token that was written.</param>
    public delegate void TokenWritten(IPropertiesWriter writer, PropertiesToken token);

    /// <summary>
    /// Represents a ".properties" document writer used to write document information, in the form of
    /// tokens. This interface allows for multiple custom implementations of ".properties" writers.
    /// </summary> 
    public interface IPropertiesWriter : IDisposable
    {
        /// <summary>
        /// Event raised when a token is written into an <see cref="IPropertiesWriter"/>.
        /// </summary>
        event TokenWritten? TokenWritten;

        /// <summary>
        /// The settings for this writer.
        /// </summary>
        PropertiesWriterSettings Settings { get; }

        /// <summary>
        /// Writes or emits the token to this writer.
        /// </summary>
        /// <param name="token">The token to write.</param>
        /// <exception cref="PropertiesException">If an incorrect token was passed as an argument, 
        /// depending on the context.</exception>
        /// <returns>true if the token was successfully written; false otherwise.</returns>
        bool Write(PropertiesToken token);

        /// <summary>
        /// Dumps all the cached tokens to the underlying stream.
        /// </summary>
        void Flush();
    }

    /// <summary>
    /// Provides extensions methods for an <see cref="IPropertiesWriter"/>.
    /// </summary>
    public static class PropertiesWriterExtensions
    {
        /// <summary>
        /// Writes a new property to the document.
        /// </summary>
        /// <param name="writer">The underlying writer</param>
        /// <param name="key">The key for the property</param>
        /// <param name="value">The value of the property.</param>
        /// <returns>true if the property could be written; false otherwise.</returns>
        public static bool WriteProperty(this IPropertiesWriter writer, string key, string? value)
        {
            return writer.Write(new PropertiesToken(PropertiesTokenType.Key, key)) && writer.Write(new PropertiesToken(PropertiesTokenType.Value, value));
        }
    }
}