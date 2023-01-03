using System;
using System.Collections;
using System.Collections.Generic;

namespace PropertiesDotNet.Core.Events
{
    /// <summary>
    /// Represents an immutable or read-only stream of <see cref="PropertiesEvent"/>s.
    /// </summary>
    public sealed class ReadOnlyEventStream : IEventStream
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
        /// Creates a new <see cref="ReadOnlyEventStream"/>.
        /// </summary>
        /// <param name="event">The event to copy into this stream.</param>
        public ReadOnlyEventStream(PropertiesEvent @event)
        {
            _events = new Queue<PropertiesEvent>();
            Enqueue(@event);
        }

        /// <summary>
        /// Creates a new <see cref="ReadOnlyEventStream"/>.
        /// </summary>
        /// <param name="events">The events to copy into this stream.</param>
        public ReadOnlyEventStream(params PropertiesEvent[] events)
        {
            _events = new Queue<PropertiesEvent>();

            foreach (PropertiesEvent evt in events)
                Enqueue(evt);
        }

        /// <summary>
        /// Creates a new <see cref="ReadOnlyEventStream"/>.
        /// </summary>
        /// <param name="events">The events to copy into this stream.</param>
        public ReadOnlyEventStream(IEventStream events)
        {
            _events = new Queue<PropertiesEvent>(events);
        }

        /// <summary>
        /// Creates a new <see cref="ReadOnlyEventStream"/>.
        /// </summary>
        /// <param name="events">The events to copy into this stream.</param>
        public ReadOnlyEventStream(IEnumerable<PropertiesEvent> events)
        {
            _events = new Queue<PropertiesEvent>();

            foreach (PropertiesEvent evt in events)
                Enqueue(evt);
        }

        private void Enqueue(PropertiesEvent @event)
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
            return new ReadOnlyEventStream(this);
        }

        /// <inheritdoc/>
        public void CopyTo(Array array, int index)
        {
            _events.CopyTo((array as PropertiesEvent[])!, index);
        }

        /// <inheritdoc/>
        public IEnumerator<PropertiesEvent> GetEnumerator()
        {
            return _events.GetEnumerator();
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
