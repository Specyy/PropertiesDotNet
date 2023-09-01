using System;
#if !NETSTANDARD1_3
using System.Runtime.Serialization;
#endif

namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Represents an exception thrown by the PropertiesDotNet library.
    /// </summary>
#if !NETSTANDARD1_3
    [Serializable]
#endif
    public sealed class PropertiesException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="PropertiesException"/>.
        /// </summary>
        public PropertiesException() : base()
        {
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesException"/>.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public PropertiesException(string? message) : base(message)
        {

        }

        /// <summary>
        /// Creates a new <see cref="PropertiesException"/>.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PropertiesException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

#if !NETSTANDARD1_3
        private PropertiesException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
#endif
    }
}