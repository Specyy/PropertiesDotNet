using System;

#if !NETSTANDARD1_3
using System.Runtime.Serialization;
#endif

namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Represents an exception thrown when a serialization or deserialization error is encountered.
    /// </summary>
    public class PropertiesSerializationException : PropertiesException
    {
        /// <summary>
        /// Creates a new <see cref="PropertiesSerializationException"/>.
        /// </summary>
        public PropertiesSerializationException() : base()
        {
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesSerializationException"/>.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public PropertiesSerializationException(string? message) : base(message)
        {

        }

        /// <summary>
        /// Creates a new <see cref="PropertiesSerializationException"/>.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PropertiesSerializationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

#if !NETSTANDARD1_3
        /// <summary>
        /// Creates a new <see cref="PropertiesSerializationException"/>.
        /// </summary>
        /// <param name="info">The serialization information.</param>
        /// <param name="context">The serialization context.</param>
        public PropertiesSerializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
#endif
    }
}
