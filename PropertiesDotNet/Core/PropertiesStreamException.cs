using System;

#if !NETSTANDARD1_3
using System.Runtime.Serialization;
#endif

namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Represents an exception thrown when reading or writing to a ".properties" stream,
    /// and an error is encountered.
    /// </summary>
    public sealed class PropertiesStreamException : PropertiesException
    {
        /// <summary>
        /// The position in the stream where the exception was thrown.
        /// </summary>
        public StreamMark? Start { get; }

        /// <summary>
        /// The end position in the stream where the exception was thrown.
        /// </summary>
        public StreamMark? End { get; }

        /// <summary>
        /// Creates a new <see cref="PropertiesStreamException"/>.
        /// </summary>
        /// <param name="start">The starting position of this exception in the stream.</param>
        /// <param name="end">The ending position of this exception in the stream.</param>
        public PropertiesStreamException(StreamMark? start, StreamMark? end) : base()
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesStreamException"/>.
        /// </summary>
        /// <param name="start">The starting position of this exception in the stream.</param>
        /// <param name="end">The ending position of this exception in the stream.</param>
        /// <param name="message">The exception message.</param>
        public PropertiesStreamException(StreamMark? start, StreamMark? end, string? message) : base(message)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesStreamException"/>.
        /// </summary>
        /// <param name="start">The starting position of this exception in the text stream.</param>
        /// <param name="end">The ending position of this exception in the text stream.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PropertiesStreamException(StreamMark? start, StreamMark? end, string? message, Exception? innerException) : base(message, innerException)
        {
            Start = start;
            End = end;
        }

#if !NETSTANDARD1_3
        /// <summary>
        /// Creates a new <see cref="PropertiesStreamException"/>.
        /// </summary>
        /// <param name="info">The serialization information.</param>
        /// <param name="context">The serialization context.</param>
        public PropertiesStreamException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
#endif


    }
}
