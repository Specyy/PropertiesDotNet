using PropertiesDotNet.Core.Events;

namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Delegate that is called when a <see cref="PropertiesEvent"/> is read from a stream.
    /// </summary>
    /// <param name="reader">The reader where the event was read.</param>
    /// <param name="event">The event that was read.</param>
    public delegate void EventRead(IPropertiesReader reader, PropertiesEvent @event);

    /// <summary>
    /// Represents a ".properties" document reader used to read document information in the form of
    /// <see cref="PropertiesEvent"/>s. This interface allows for multiple custom implementations of ".properties" readers.
    /// </summary> 
    public interface IPropertiesReader
    {
        /// <summary>
        /// Event raised when a <see cref="PropertiesEvent"/> is read from a document.
        /// </summary>
        event EventRead? EventRead;

        /// <summary>
        /// The settings for this reader.
        /// </summary>
        PropertiesReaderSettings Settings { get; }

        /// <summary>
        /// Returns whether this reader provides detailed <see cref="StreamMark"/> information for
        /// event stream position.
        /// </summary>
        bool HasDetailedStreamPosition { get; }

        /// <summary>
        /// Peeks the next event.
        /// </summary>
        /// <returns>The next event, without consuming it, or null if there are no available events.</returns>
        PropertiesEvent? Peek();

        /// <summary>
        /// Reads the next event.
        /// </summary>
        /// <returns>The next event, or null if there are no available events.</returns>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        PropertiesEvent? Read();
    }
}
