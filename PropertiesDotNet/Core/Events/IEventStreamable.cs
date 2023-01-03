namespace PropertiesDotNet.Core.Events
{
    /// <summary>
    /// Allows an object to be transformed into an <see cref="IEventStream"/>.
    /// </summary>
    public interface IEventStreamable
    {
        /// <summary>
        /// Returns this object as a stream of events.
        /// </summary>
        /// <returns>This object as a stream of events.</returns>
        IEventStream ToEventStream();
    }

    /// <summary>
    /// Provides extension methods for an <see cref="IEventStreamable"/>.
    /// </summary>
    public static class EventStreamableExtensions
    {
        /// <summary>
        /// Returns this object as a stream of events.
        /// </summary>
        /// <param name="instance">The object.</param>
        /// <param name="stream">The stream of events.</param>
        public static void ToEventStream(IEventStreamable instance, out IEventStream stream)
        {
            stream = instance.ToEventStream();
        }
    }
}
