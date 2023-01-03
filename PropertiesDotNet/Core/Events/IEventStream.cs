using System;
using System.Collections;
using System.Collections.Generic;

namespace PropertiesDotNet.Core.Events
{
    /// <summary>
    /// Represents a stream of <see cref="PropertiesEvent"/>s.
    /// </summary>
    public interface IEventStream : ICollection, IEnumerable<PropertiesEvent>
#if !NETSTANDARD1_3
        , ICloneable
#endif
    {
        /// <summary>
        /// Returns the event at current stream position, without reading it.
        /// </summary>
        /// <returns>The event at the current stream position, or null if there are no events.</returns>
        PropertiesEvent? Peek();

        /// <summary>
        /// Returns the event at the current stream position, then moves onto the next.
        /// </summary>
        /// <returns>The event at the current stream position.</returns>
        PropertiesEvent? Dequeue();
        
        /// <summary>
        /// Writes this event stream into a <see cref="IPropertiesWriter"/>.
        /// </summary>
        /// <param name="writer">The writer to write the events into.</param>
        /// <param name="keep">Whether to remove the events from the queue once they are written
        /// to the <paramref name="writer"/>.</param>
        void WriteTo(IPropertiesWriter writer, bool keep = true);
    }
}
