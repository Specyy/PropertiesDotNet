using System;

namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Delegate called when a <see cref="PropertiesToken"/> is written.
    /// </summary>
    /// <param name="writer">The writer where the token was written.</param>
    /// <param name="token">The token that was written.</param>
    public delegate void TokenWritten(IPropertiesWriter writer, PropertiesToken token);

    /// <summary>
    /// Represents a ".properties" document writer used to write document information, in the form of
    /// <see cref="PropertiesToken"/>s. This interface allows for multiple custom implementations of ".properties" writers.
    /// </summary> 
    public interface IPropertiesWriter : IDisposable
    {
        /// <summary>
        /// Event raised when a <see cref="PropertiesToken"/> is written into an <see cref="IPropertiesWriter"/>.
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
        /// <exception cref="PropertiesException">If an incorrect <paramref name="token"/> was passed
        /// as an argument, depending on the context.</exception>
        /// <returns>true if the token was successfully written; false otherwise.</returns>
        bool Write(PropertiesToken token);

        /// <summary>
        /// Dumps all the cached tokens to the underlying stream.
        /// </summary>
        void Flush();
    }
}