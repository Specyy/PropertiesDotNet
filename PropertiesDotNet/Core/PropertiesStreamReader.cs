using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Represents a character reader for a ".properties" document, 
    /// which follows the ".properties" specification.
    /// </summary>
    internal sealed class PropertiesStreamReader : IDisposable
    {
        /// <summary>
        /// Whether the end of the stream has been reached.
        /// </summary>
        public bool EndOfStream => _buffer.EndOfStream;

        /// <summary>
        /// Returns the current position of this reader.
        /// </summary>
        public StreamMark CurrentPosition => _cursor.CurrentPosition;

        private LookAheadBuffer _buffer;
        private readonly StreamCursor _cursor;

        /// <summary>
        /// Creates a new <see cref="PropertiesStreamReader"/>.
        /// </summary>
        /// <param name="stream">The stream to buffer.</param>
        /// <param name="bufferCapacity">The capacity for the buffer.</param>
        public PropertiesStreamReader(TextReader stream, int bufferCapacity = LookAheadBuffer.DEFAULT_CAPACITY)
        {
            _buffer = new LookAheadBuffer(stream, bufferCapacity);
            _cursor = new StreamCursor();
        }

        /// <summary>
        /// Returns the character at the given <paramref name="offset"/>.
        /// </summary>
        /// <param name="offset">The character offset, from the <see cref="CurrentPosition"/>.</param>
        /// <returns>The character at the given <paramref name="offset"/>.</returns>
        public char Peek(int offset = 0) => _buffer.Peek(offset);

        /// <summary>
        /// Reads the character at the current position, then advances the reader's position by 1.
        /// </summary>
        /// <returns>The character at the current position.</returns>
        public char Read()
        {
            if (EndOfStream)
                return default;

            // Advance cursor - must be done inside here for CRLF
            if (IsNewLine())
            {
                // Don't skip line on CR of CRLF
                if (Check(0, '\r'))
                {
                    // Should be '\r'
                    var read = _buffer.Read();

                    if (Check(0, '\n'))
                        _cursor.AdvanceColumn();
                    else
                        _cursor.AdvanceLine();

                    return read;
                }

                _cursor.AdvanceLine();
            }
            else if (Check(0, '\t'))
            {
                _cursor.AdvanceColumn(4);
            }
            else
            {
                _cursor.AdvanceColumn();
            }

            return _buffer.Read();
        }

        /// <summary>
        /// Returns whether the character at the given <paramref name="offset"/> forms a new line signature.
        /// </summary>
        /// <param name="offset">The character offset, from the <see cref="CurrentPosition"/>.</param>
        /// <returns>Whether the character at the given <paramref name="offset"/> forms a new line signature.</returns>
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool IsNewLine(int offset = 0)
        {
            var selected = Peek(offset);
            return selected == '\r' ||
                selected == '\n';
            //selected == '\u2028' ||
            //selected == '\u0085'; // CR, LF, LS, NEL
        }

        internal void ReadLineEnd()
        {
            // 2 skips on CRLF
            if (Read() == '\r' && Check(0, '\n'))
                Read();
        }

        /// <summary>
        /// Returns whether the character at the given <paramref name="offset"/> is considered a 
        /// white-space character.
        /// </summary>
        /// <param name="offset">The character offset, from the <see cref="CurrentPosition"/>.</param>
        /// <returns>Whether the character at the given <paramref name="offset"/> is considered a 
        /// white-space character.</returns>
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool IsWhiteSpace(int offset = 0)
        {
            var selected = Peek(offset);
            return selected == ' ' ||
                selected == '\t' ||
                selected == '\f';
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal void ReadWhiteSpace(int offset = 0)
        {
            while (IsWhiteSpace(offset))
                Read();
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal void ReadWhiteOrLine(int offset = 0)
        {
            while (IsWhiteSpace(offset) || IsNewLine(offset))
                Read();
        }

        /// <summary>
        /// Returns whether the character at the given <paramref name="offset"/> is part of the ISO-8859-1
        /// character set.
        /// </summary>
        /// <param name="offset">The character offset, from the <see cref="CurrentPosition"/>.</param>
        /// <returns>Whether the character at the given <paramref name="offset"/> is part of the ISO-8859-1
        /// character set.</returns>
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool IsLatin1(int offset = 0)
        {
            return Peek(offset) <= 0xFF;
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal bool IsHex()
        {
            var character = Peek();
            return
                (character >= '0' && character <= '9') ||
                (character >= 'A' && character <= 'F') ||
                (character >= 'a' && character <= 'f');
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal int ReadHex()
        {
            var selected = Read();
            return selected <= '9' ? selected - '0' :
                (selected <= 'F' ? selected - 'A' + 10 : selected - 'a' + 10);
        }

        /// <summary>
        /// Returns whether the character at the given <paramref name="offset"/> is equal to the <paramref name="check"/>.
        /// </summary>
        /// <param name="offset">The character offset, from the <see cref="CurrentPosition"/>.</param>
        /// <param name="check">The character to check for.</param>
        /// <returns>Whether the character at the given <paramref name="offset"/> is equal to the <paramref name="check"/>.</returns>
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal bool Check(int offset, char check) => Peek(offset) == check;

        /// <summary>
        /// Returns whether the character at the given <paramref name="offset"/> is equal to the any of the given
        /// <paramref name="checks"/>.
        /// </summary>
        /// <param name="offset">The character offset, from the <see cref="CurrentPosition"/>.</param>
        /// <param name="checks">The characters to check for.</param>
        /// <returns>Whether the character at the given <paramref name="offset"/> is equal to the any of the given
        /// <paramref name="checks"/>.</returns>
        internal bool Check(int offset, params char[] checks)
        {
            var selected = Peek(offset);

            for (var i = 0; i < checks.Length; i++)
            {
                if (selected == checks[i])
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether the characters, starting from the given <paramref name="offset"/> are sequentially
        /// equal the given <paramref name="value"/>.
        /// </summary>
        /// <param name="offset">The character offset, from the <see cref="CurrentPosition"/>.</param>
        /// <param name="value">The string value to check for.</param>
        /// <returns>Whether the characters, starting from the given <paramref name="offset"/> are sequentially
        /// equal the given <paramref name="value"/>.</returns>
        internal bool Check(int offset, string value)
        {
            // If there are not enough characters to check for.
            // Ensures that buffer is larger or the same length
            if ((_buffer.Length - offset) <= value.Length)
                return false;

            for (int i = 0; i < value.Length; i++)
            {
                if (Peek(offset + i) != value[i])
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _buffer.Dispose();
            _cursor.Reset();
        }
    }
}
