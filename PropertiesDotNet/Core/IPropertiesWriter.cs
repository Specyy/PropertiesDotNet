using PropertiesDotNet.Core.Events;

namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Delegate called when a <see cref="PropertiesEvent"/> is written.
    /// </summary>
    /// <param name="writer">The writer where the event was written.</param>
    /// <param name="event">The event that was written.</param>
    public delegate void EventWritten(IPropertiesWriter writer, PropertiesEvent @event);

    /// <summary>
    /// Represents a ".properties" document writer used to write document information, in the form of
    /// <see cref="PropertiesEvent"/>s. This interface allows for multiple custom implementations of ".properties" writers.
    /// </summary> 
    public interface IPropertiesWriter
    {
        /// <summary>
        /// Event raised when a <see cref="PropertiesEvent"/> is written into an <see cref="IPropertiesWriter"/>.
        /// </summary>
        event EventWritten? EventWritten;

        /// <summary>
        /// The settings for this writer.
        /// </summary>
        PropertiesWriterSettings Settings { get; }

        /// <summary>
        /// Writes or emits the event to this writer.
        /// </summary>
        /// <param name="event">The event to write.</param>
        /// <exception cref="PropertiesException">If an incorrect <paramref name="event"/> was passed
        /// as an argument, depending on the context.</exception>
        void Write(PropertiesEvent @event);
    }
}
