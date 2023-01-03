using System;
using System.Collections;
using System.Collections.Generic;

namespace PropertiesDotNet.Core.Events
{
    /// <summary>
    /// Represents a mutable stream of <see cref="PropertiesEvent"/>s.
    /// </summary>
    public sealed class EventStream : IEventStream
    {
        /// <summary>
        /// The number of <see cref="PropertiesEvent"/>s in this stream.
        /// </summary>
        public int Count => _events.Count;

        /// <inheritdoc/>
        public bool IsSynchronized => false;

        /// <inheritdoc/>
        public object SyncRoot => throw new NotSupportedException($"Cannot retrieve sync root of {nameof(EventStream)}!");

        private readonly Queue<PropertiesEvent> _events;

        /// <summary>
        /// Creates a new <see cref="EventStream"/>.
        /// </summary>
        public EventStream()
        {
            _events = new Queue<PropertiesEvent>();
        }

        /// <summary>
        /// Creates a new <see cref="EventStream"/>.
        /// </summary>
        /// <param name="event">The event to copy into this stream.</param>
        public EventStream(PropertiesEvent @event)
        {
            _events = new Queue<PropertiesEvent>();
            Enqueue(@event);
        }

        /// <summary>
        /// Creates a new <see cref="EventStream"/>.
        /// </summary>
        /// <param name="events">The events to copy into this stream.</param>
        public EventStream(params PropertiesEvent[] events)
        {
            _events = new Queue<PropertiesEvent>();

            foreach (PropertiesEvent evt in events)
                Enqueue(evt);
        }

        /// <summary>
        /// Creates a new <see cref="EventStream"/>.
        /// </summary>
        /// <param name="events">The events to copy into this stream.</param>
        public EventStream(IEventStream events)
        {
            _events = new Queue<PropertiesEvent>(events);
        }

        /// <summary>
        /// Creates a new <see cref="EventStream"/>.
        /// </summary>
        /// <param name="events">The events to copy into this stream.</param>
        public EventStream(IEnumerable<PropertiesEvent> events) : this()
        {
            foreach (PropertiesEvent evt in events)
                Enqueue(evt);
        }

        /// <summary>
        /// Queues the given event into this stream.
        /// </summary>
        /// <param name="event">The event to queue.</param>
        public void Enqueue(PropertiesEvent @event)
        {
            if (@event is null)
                throw new ArgumentNullException($"{nameof(EventStream)} cannot contain null events!");

            _events.Enqueue(@event);
        }

        /// <inheritdoc/>
        public PropertiesEvent? Peek()
        {
            return Count < 1 ? null : _events.Peek();
        }

        /// <inheritdoc/>
        public PropertiesEvent? Dequeue()
        {
            return Count < 1 ? null : _events.Dequeue();
        }

        /// <summary>
        /// Returns a copy of this event stream.
        /// </summary>
        /// <returns>A copy of this event stream.</returns>
        public object Clone()
        {
            return new EventStream(this);
        }

        /// <inheritdoc/>
        public void CopyTo(Array array, int index)
        {
            _events.CopyTo(array as PropertiesEvent[], index);
        }

        /// <inheritdoc/>
        public IEnumerator<PropertiesEvent> GetEnumerator()
        {
            return _events.GetEnumerator();
        }

        /// <summary>
        /// Writes the remaining events from a <see cref="IPropertiesReader"/> into this event stream.
        /// </summary>
        /// <param name="reader">The reader to read the events from.</param>
        public void ReadFrom(IPropertiesReader reader)
        {
            while (reader.TryRead(out var evt))
                Enqueue(evt);
        }

        /// <inheritdoc/>
        public void WriteTo(IPropertiesWriter writer, bool keep = true)
        {
            if (keep)
            {
                foreach (PropertiesEvent evt in this)
                    writer.Write(evt);
            }
            else
            {
                while (Count > 0)
                    writer.Write(Dequeue());
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _events.GetEnumerator();
        }
    }
}
