using System;
using System.IO;

namespace PropertiesDotNet.Test
{
    /// <summary>
    /// Represents a look-ahead character stream buffer.
    /// </summary>
    internal sealed class LookAheadBuffer : IDisposable
    {
        private char[] _buffer;
        private int _length;
        private int _currentIndex;

        private bool _endOfInput;
        private bool _disposed;

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
        public int Length => _length;

        /// <summary>
        /// Gets a value indicating whether the end of the input reader has been reached.
        /// </summary>
        public bool EndOfStream => _endOfInput && _length == 0;

        /// <summary>
        /// Whether this stream has been disposed.
        /// </summary>
        public bool Disposed => _disposed;

        /// <summary>
        /// Creates a new <see cref="LookAheadBuffer"/>.
        /// </summary>
        /// <param name="steam">The input stream.</param>
        /// <param name="capacity">The buffer capacity.</param>
        public LookAheadBuffer(TextReader steam, int capacity)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity), "The capacity must be greater than 0!");

            Stream = steam ?? throw new ArgumentNullException(nameof(steam));
            Capacity = capacity;

            // Fill buffer
            _buffer = new char[Capacity];

            for (_length = 0, _currentIndex = 0; _currentIndex < Capacity; _currentIndex++, _length++)
            {
                int read = Stream.Read();

                if (read < 0)
                {
                    _endOfInput = true;
                    break;
                }

                _buffer[_currentIndex] = (char)read;
            }

            _currentIndex = 0;
        }

        /// <summary>
        /// Returns the character at the specified offset from current stream position.
        /// </summary>
        /// <param name="offset">The character offset.</param>
        /// <returns>The character at the specified offset from current stream position.</returns>
        public char Peek(int offset)
        {
            if (offset < 0 || offset >= Capacity)
                throw new ArgumentOutOfRangeException(nameof(offset), $"The offset must be greater than 0, but less than {Capacity}!");

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
                if (!(_buffer is null))
                    _buffer = null!;

                return default;
            }

            // Save last
            char current = _buffer[_currentIndex];

            // If no more to read, set internal values to default, or '\0'
            if (_endOfInput)
            {
                _buffer[_currentIndex] = default;
                _length--;
            }
            // Read character from stream
            else
            {
                // Read next
                int read;

                if ((read = Stream.Read()) < 0)
                {
                    _endOfInput = true;
                    read = default(char);
                    _length--;
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
            if (_disposed)
                return;

            _endOfInput = _disposed = true;
            Stream.Dispose();
        }
    }
}
