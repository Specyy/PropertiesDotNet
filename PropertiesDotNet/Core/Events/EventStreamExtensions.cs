using System.Collections.Generic;

namespace PropertiesDotNet.Core.Events
{
    /// <summary>
    /// Provides extension methods for <see cref="IEventStream"/>.
    /// </summary>
    public static class EventStreamExtensions
    {
        /// <summary>
        /// Queues the specified events into this stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="events">The events to enqueue.</param>
        public static void Enqueue(this EventStream stream, IEventStream events)
        {
            foreach (PropertiesEvent evt in events)
                stream.Enqueue(evt);
        }

        /// <summary>
        /// Queues the specified events into this stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="events">The events to enqueue.</param>
        public static void Enqueue(this EventStream stream, IEnumerable<PropertiesEvent> events)
        {
            foreach (PropertiesEvent evt in events)
                stream.Enqueue(evt);
        }

        /// <summary>
        /// Dequeues the specified number of events from this stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="count">The number of events to dequeue.</param>
        /// <param name="dequeued">The dequeued events.</param>
        /// <returns>The number of actual dequeued events.</returns>
        public static int Dequeue(this IEventStream stream, int count, out ICollection<PropertiesEvent> dequeued)
        {
            dequeued = new LinkedList<PropertiesEvent>();

            for (var i = 0; i < count; i++)
            {
                if (stream.Count < 1)
                    return i;

                dequeued.Add(stream.Dequeue());
            }

            return count;
        }

        /// <summary>
        /// Dequeues the specified number of events from this stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="count">The number of events to dequeue.</param>
        /// <returns>The dequeued events.</returns>
        public static IEnumerable<PropertiesEvent> Dequeue(this IEventStream stream, int count)
        {
            for (var i = 0; i < count; i++)
            {
                if (stream.Count < 1)
                    yield break;

                yield return stream.Dequeue();
            }
        }
    }
}
