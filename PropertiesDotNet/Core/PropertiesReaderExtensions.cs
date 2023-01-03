using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PropertiesDotNet.Core.Events;
using System.Reflection;

namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Provides extensions methods for a <see cref="IPropertiesReader"/>.
    /// </summary>
    public static class PropertiesReaderExtensions
    {
        /// <summary>
        /// Reads the next event, if it is of the given type, otherwise throws a <see cref="PropertiesException"/>, expecting
        /// the specified type.
        /// </summary>
        /// <param name="reader">The reader to read the event from.</param>
        /// <typeparam name="T">The next event type to enforce.</typeparam>
        /// <exception cref="PropertiesException">If next event is not of the given type <typeparamref name="T"/>.</exception>
        public static T Read<T>(this IPropertiesReader reader) where T : notnull, PropertiesEvent
        {
            if (reader.TryRead(out T read))
            {
                return read;
            }

            throw new PropertiesException($"Expected {typeof(T)}, got {reader.Peek()?.GetType().FullName ?? "null"}");
        }

        internal static T ReadSerialized<T>(this IPropertiesReader reader) where T : notnull, PropertiesEvent
        {
            if (reader.TryRead(out T read))
            {
                return read;
            }

            throw new PropertiesSerializationException(
                $"While loading node: expected {typeof(T)}, got {reader.Peek()?.GetType().FullName ?? "null"}");
        }

        /// <summary>
        /// Attempts to read the next event.
        /// </summary>
        /// <param name="reader">The reader to read the event from.</param>
        /// <param name="event">The next event, or null if there are no available events.</param>
        /// <returns>Whether the next event could be read.</returns>
        public static bool TryRead(this IPropertiesReader reader, [NotNullWhen(true)] out PropertiesEvent? @event)
        {
            if ((@event = reader.Read()) is null)
                return false;

            return true;
        }

        /// <summary>
        /// Reads the next event if it is of the given type.
        /// </summary>
        /// <param name="reader">The reader to read the event from.</param>
        /// <param name="event">The next event, or null if there are no available events, or it is not of the given type.</param>
        /// <typeparam name="T">The event type to read.</typeparam>
        /// <returns>Whether the next event could be read.</returns>
        public static bool TryRead<T>(this IPropertiesReader reader, [NotNullWhen(true)] out T? @event)
            where T : notnull, PropertiesEvent
        {
            @event = default;

            if (!(reader.Peek() is T))
                return false;

            @event = (T) reader.Read()!;
            return true;
        }

        /// <summary>
        /// Reads the next event if it is of the given type.
        /// </summary>
        /// <param name="reader">The reader to read the event from.</param>
        /// <param name="event">The next event, or null if there are no available events, or it is not of the given type.</param>
        /// <param name="eventType">The event type to read.</param>
        /// <returns>Whether the next event could be read.</returns>
        public static bool TryRead(this IPropertiesReader reader, Type eventType,
            [NotNullWhen(true)] out PropertiesEvent? @event)
        {
            if (eventType.IsAssignableFrom(reader.Peek()?.GetType()))
            {
                @event = reader.Read()!;
                return true;
            }

            @event = null;
            return false;
        }

        /// <summary>
        /// Reads the specified number of events.
        /// </summary>
        /// <param name="reader">The reader to read the events from.</param>
        /// <param name="count">The number of events to read.</param>
        /// <returns>The events that were read.</returns>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        public static IEventStream Read(this IPropertiesReader reader, int count)
        {
            if (count == 0)
                return new ReadOnlyEventStream();

            LinkedList<PropertiesEvent> events = new LinkedList<PropertiesEvent>();

            for (var i = 0; i < count; i++)
            {
                var next = reader.Read();

                if (next is null)
                    break;

                events.AddLast(next);
            }

            return new ReadOnlyEventStream(events);
        }

        /// <summary>
        /// Skips the specified number of events, if there are available events.
        /// </summary>
        /// <param name="reader">The reader to skip the events from.</param>
        /// <param name="count">The number of events to skip.</param>
        /// <returns>The number of events actually skipped.</returns>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        /// <exception cref="ArgumentException">If the <paramref name="count"/> is less than 0.</exception>
        public static int Skip(this IPropertiesReader reader, int count)
        {
            if (count < 0)
                throw new ArgumentException($"Cannot skip {count} events!");

            for (var i = 0; i < count; i++)
            {
                if (reader.Read() is null)
                    return i;
            }

            return count;
        }

        /// <summary>
        /// Skips the nested events of the next event.
        /// </summary>
        /// <param name="reader">The reader to read the events from.</param>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown, or if the next event
        /// is null.</exception>
        public static void SkipNestedEvents(this IPropertiesReader reader)
        {
            long eventDepth = 0;

            do
            {
                var next = reader.Read();

                if (next is null)
                    throw new PropertiesException("No events left to skip!");

                eventDepth += next.DepthIncrease;
            } while (eventDepth > 0);
        }

        /// <summary>
        /// Reads the next property.
        /// </summary>
        /// <param name="reader">The reader to read the events from.</param>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        /// <returns>The next property in the reader, or if the next events do not make up a property.</returns>
        public static IEventStream ReadProperty(this IPropertiesReader reader)
        {
            var start = reader.Read();

            if (!(start is PropertyStart))
                throw new PropertiesException(
                    $"Encountered invalid event \"{start?.GetType().FullName ?? "null"}\" while reading property!");

            LinkedList<PropertiesEvent> list = new LinkedList<PropertiesEvent>();
            list.AddLast(start);

            while (true)
            {
                var next = reader.Read();
                list.AddLast(next);

                if (next is PropertyEnd)
                    break;

                if (!(next is Key || next is ValueAssigner || next is Value))
                    throw new PropertiesException(
                        $"Encountered invalid event \"{next?.GetType().FullName ?? "null"}\" while reading property!");
            }

            return new ReadOnlyEventStream(list);
        }

        /// <summary>
        /// Skips the current property events if the reader is inside a property.
        /// </summary>
        /// <param name="reader">The reader to read the events from.</param>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown, or the current event is
        /// not a property.</exception>
        public static void SkipProperty(this IPropertiesReader reader)
        {
            var next = reader.Peek();

            if (next is PropertyStart || next is Key || next is ValueAssigner || next is Value || next is PropertyEnd)
            {
                while (!(reader.Read() is PropertyEnd))
                    ;
            }
            else
            {
                throw new PropertiesException("Could not skip the current property because it is not a property!");
            }
        }

        /// <summary>
        /// Reads all the events from the reader.
        /// </summary>
        /// <param name="reader">The reader to read the events from.</param>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document, and exceptions are configured to be thrown.</exception>
        /// <returns>All events left in the reader.</returns>
        public static IEventStream ReadToEnd(this IPropertiesReader reader)
        {
            PropertiesEvent? next;
            LinkedList<PropertiesEvent> list = new LinkedList<PropertiesEvent>();

            while (!((next = reader.Read()) is null))
                list.AddLast(next);

            return new ReadOnlyEventStream(list);
        }

        private static bool IsAssignableFrom(this Type t, Type other)
        {
#if !NETSTANDARD1_3
            return t.IsAssignableFrom(other);
#else
            return t.GetTypeInfo().IsAssignableFrom(other.GetTypeInfo());
#endif
        }
    }
}