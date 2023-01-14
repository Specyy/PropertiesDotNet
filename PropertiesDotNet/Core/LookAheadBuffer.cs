using System;
using System.IO;

namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Represents a look-ahead character stream buffer.
    /// </summary>
    internal sealed class LookAheadBuffer : IDisposable
    {
        internal const int DEFAULT_CAPACITY = 1024;

        private int _currentIndex;
        private char[] _buffer;

        private bool _endOfInput;

        /// <summary>
        /// Returns the character stream.
        /// </summary>
        public TextReader Stream { get; }

        /// <summary>
        /// The capacity of this buffer.
        /// </summary>
        public int Capacity { get; }

        /// <summary>
        /// The number of characters inside the buffer.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the end of the input reader has been reached.
        /// </summary>
        public bool EndOfStream => _endOfInput && Length == 0;

        /// <summary>
        /// Whether this stream has been disposed.
        /// </summary>
        public bool Disposed { get; private set; }

        /// <summary>
        /// Creates a new <see cref="LookAheadBuffer"/>.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="capacity">The buffer capacity.</param>
        public LookAheadBuffer(TextReader stream, int capacity)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity), "The capacity must be greater than 0!");

            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _buffer = new char[Capacity = capacity];

            // Fill buffer
            _endOfInput = (Length = Stream.ReadBlock(_buffer, 0, Capacity)) < Capacity;
            _currentIndex = 0;
        }

        /// <summary>
        /// Returns the character at the specified offset from current stream position.
        /// </summary>
        /// <param name="offset">The character offset.</param>
        /// <returns>The character at the specified offset from current stream position.</returns>
        public char Peek(int offset)
        {
            //if (offset < 0 || offset >= Capacity)
            //    throw new ArgumentOutOfRangeException(nameof(offset),
            //        $"The offset must be greater than 0, but less than {Capacity}!");

            if (EndOfStream)
                return default;

            int realOffset = _currentIndex + offset;

            if (realOffset >= Capacity)
                realOffset -= Capacity;

            return _buffer[realOffset];
        }

        /// <summary>
        /// Reads the next character from the stream into this buffer.
        /// </summary>
        /// <returns>The last character.</returns>
        public char Read()
        {
            // Clear buffer on stream end
            if (EndOfStream)
            {
                _buffer = null!;
                return default;
            }

            // Save last
            char current = _buffer[_currentIndex];

            // If no more to read, set internal values to default, or '\0'
            if (_endOfInput)
            {
                _buffer[_currentIndex] = default;
                Length--;
            }
            // Read character from stream
            else
            {
                // Read next
                int read = Stream.Read();

                if (read < 0)
                {
                    _endOfInput = true;
                    read = default(char);
                    Length--;
                }

                _buffer[_currentIndex] = (char)read;
            }

            // Advance position by 1
            _currentIndex++;

            if (_currentIndex >= Capacity)
                _currentIndex -= Capacity;

            return current;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Disposed)
                return;

            _endOfInput = Disposed = true;
            _buffer = null!;
            Stream.Dispose();
        }
    }
}