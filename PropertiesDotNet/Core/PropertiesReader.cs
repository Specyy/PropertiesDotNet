using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

using PropertiesDotNet.Utils;

namespace PropertiesDotNet.Core
{
    enum ParserState : byte
    {
        Start = 0,
        Comment,
        Key,
        Assigner,
        Value,
        Error,
        End
    }

    /// <summary>
    /// Token reader for a ".properties" document.
    /// </summary>
    public sealed class PropertiesReader : IPropertiesReader
    {
        /// <inheritdoc/>
        public PropertiesReaderSettings Settings
        {
            get => _settings;
            set => _settings = value ?? PropertiesReaderSettings.Default;
        }

        /// <inheritdoc/>
        public PropertiesToken Token => _token;

        /// <inheritdoc/>
        public event TokenRead? TokenRead;

        /// <inheritdoc/>
        public StreamMark? TokenStart => _tokenStart;

        /// <inheritdoc/>
        public StreamMark? TokenEnd => _tokenEnd;

        /// <inheritdoc/>
        public bool HasLineInfo => true;

        /// <summary>
        /// Returns whether the current token contains logical lines. This only applies to keys and values.
        /// </summary>
        public bool LogicalLines =>
            (_token.Type == PropertiesTokenType.Key || _token.Type == PropertiesTokenType.Value) &&
            _textLogicalLines;

        /// <summary>
        /// Returns the comment handle for the current token. This handle is either an
        /// exclamtion mark (!) or a pound symbol (#) or \0 if the current token is not a comment.
        /// </summary>
        public char CommentHandle => _token.Type == PropertiesTokenType.Comment ? _commentHandle : default;

        private PropertiesReaderSettings _settings;
        private MarkingReader _stream;
        private bool _disposed;
        private PropertiesToken _token;
        private StringBuilder _textPool;

        private ParserState _state;
        private char _commentHandle;
        private bool _textLogicalLines;
        private StreamMark _tokenStart;
        private StreamMark _tokenEnd;

        /// <summary>
        /// Creates a new <see cref="PropertiesReader"/>.
        /// </summary>
        /// <param name="input">The input document.</param>
        /// <param name="settings">The settings for this reader.</param>
        public PropertiesReader(string input, PropertiesReaderSettings? settings = null) : this(new StringReader(input),
            settings)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesReader"/>.
        /// </summary>
        /// <param name="input">The input document.</param>
        /// <param name="settings">The settings for this reader.</param>
        public PropertiesReader(Stream input, PropertiesReaderSettings? settings = null) : this(
            new StreamReader(input, true), settings)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesReader"/>.
        /// </summary>
        /// <param name="input">The input document.</param>
        /// <param name="settings">The settings for this reader.</param>
        public PropertiesReader(TextReader input, PropertiesReaderSettings? settings = null)
        {
            _settings = settings ?? PropertiesReaderSettings.Default;
            _stream = new MarkingReader(input ?? throw new ArgumentNullException(nameof(input)));
            _textPool = new StringBuilder();
        }

        /// <inheritdoc/>
        public bool MoveNext()
        {
            if (ReadToken())
            {
                TokenRead?.Invoke(this, _token);
                return true;
            }

            return false;
        }

        private bool ReadToken()
        {
            switch (_state)
            {
                case ParserState.Start:
                    int next = _stream.Peek();

                    // Remove white-space before token
                    while (IsWhiteSpace(next) || IsNewLine(next))
                    {
                        if (IsNewLine(next))
                            _stream.ReadLineEnd();
                        else
                            _stream.Read();
                        next = _stream.Peek();
                    }

                    if (IsCommentHandle(next))
                    {
                        if (!Settings.IgnoreComments)
                        {
                            _state = ParserState.Comment;
                            return ReadToken();
                        }

                        SkipComments();
                    }

                    _state = _stream.EndOfStream ? ParserState.End : ParserState.Key;
                    return ReadToken();

                case ParserState.Comment:
                    ReadComment();
                    return true;

                case ParserState.Key:
                    ReadKey();
                    return true;

                case ParserState.Assigner:
                    return ReadAssigner();

                case ParserState.Value:
                    ReadValue();

                    // Ignore return value because we basically only do this for the disposing behaviour
                    // but we still need to emit this token
                    if (_state == ParserState.End)
                        ReadToken();

                    return true;

                case ParserState.Error:
                    _state = ParserState.End;
                    return ReadToken();

                default:
                case ParserState.End:
                    if (Settings.CloseOnEnd)
                        Dispose();
                    return false;
            }
        }

        private void SkipComments()
        {
            // Skip over this and proceeding comments
            // Use iteration instead of recursion
            do
            {
                for (_stream.Read(); !_stream.EndOfStream; _stream.Read())
                {
                    if (IsNewLine(_stream.Peek()))
                    {
                        _stream.ReadLineEnd();

                        // Remove white-space before token
                        while (IsWhiteSpace(_stream.Peek()) || IsNewLine(_stream.Peek()))
                        {
                            if (IsNewLine(_stream.Peek()))
                                _stream.ReadLineEnd();
                            else
                                _stream.Read();
                        }

                        break;
                    }
                }
            } while (IsCommentHandle(_stream.Peek()));
        }

        private void ReadComment()
        {
            _tokenStart = _stream.Position;

            _commentHandle = (char)_stream.Read();
            _textPool.Length = 0;

            // Remove white-space before actual text
            while (IsWhiteSpace(_stream.Peek()))
                _stream.Read();

            while (!IsNewLine(_stream.Peek()) && !_stream.EndOfStream)
                _textPool.Append((char)_stream.Read());

            _tokenEnd = _stream.Position;
            _stream.ReadLineEnd();

            _state = ParserState.Start;
            _token = new PropertiesToken(PropertiesTokenType.Comment, _textPool.ToString());
        }

        private void ReadKey()
        {
            _textPool.Length = 0;
            _tokenStart = _stream.Position;
            _textLogicalLines = false;

            while (!IsNewLine(_stream.Peek()) && !_stream.EndOfStream)
            {
                if (_stream.Peek() == '\\')
                {
                    if (!HandleEscapeSequence())
                        return;
                }
                else if (IsAssigner(_stream.Peek()))
                {
                    _tokenEnd = _stream.Position;
                    _state = ParserState.Assigner;
                    _token = new PropertiesToken(PropertiesTokenType.Key, _textPool.ToString());
                    return;
                }
                // Enforce ISO-8859-1
                else if (!Settings.AllCharacters && !IsLatin1(_stream.Peek()))
                {
                    _tokenEnd = _tokenStart = _stream.Position;
                    HandleError(
                       $"Unrecognized character '{(char)_stream.Peek()}' ({(ushort)_stream.Peek()}) at line {_tokenStart.Line} column {_tokenStart.Column}!");
                    return;
                }
                else
                {
                    _textPool.Append((char)_stream.Read());
                }
            }

            _tokenEnd = _stream.Position;
            _state = ParserState.Value;
            _token = new PropertiesToken(PropertiesTokenType.Key, _textPool.ToString());
        }

        private bool ReadAssigner()
        {
            _tokenStart = _stream.Position;
            int lastAssigner = _stream.Read();

            if (IsWhiteSpace(lastAssigner))
            {
                // Consume whitespace until last or assigner
                while (IsWhiteSpace(_stream.Peek()))
                    _stream.Read();

                // If we thought the assigner was a white-space but we were actually
                // just skipping the preceding white-space before the real assigner
                if (IsLiteralAssigner(_stream.Peek()))
                {
                    _tokenStart = _stream.Position;
                    lastAssigner = _stream.Read();
                }
                // Key with a bunch of trailing white-spaces but no value
                else if (IsNewLine(_stream.Peek()) || _stream.EndOfStream)
                {
                    _state = ParserState.Value;
                    return ReadToken();
                }
            }

            _tokenEnd = _stream.Position;
            _state = ParserState.Value;
            _token = new PropertiesToken(PropertiesTokenType.Assigner, ((char)lastAssigner).ToString());
            return true;
        }

        private void ReadValue()
        {
            // Remove leading white-space before value
            while (IsWhiteSpace(_stream.Peek()))
                _stream.Read();

            _textPool.Length = 0;
            _tokenStart = _stream.Position;
            _textLogicalLines = false;

            while (!IsNewLine(_stream.Peek()) && !_stream.EndOfStream)
            {
                if (_stream.Peek() == '\\')
                {
                    if (!HandleEscapeSequence())
                        return;
                }
                // Enforce ISO-8859-1
                else if (!Settings.AllCharacters && !IsLatin1(_stream.Peek()))
                {
                    _tokenEnd = _tokenStart = _stream.Position;
                    HandleError(
                       $"Unrecognized character '{(char)_stream.Peek()}' ({(ushort)_stream.Peek()}) at line {_tokenStart.Line} column {_tokenStart.Column}!");
                    return;
                }
                else
                {
                    _textPool.Append((char)_stream.Read());
                }
            }

            _tokenEnd = _stream.Position;
            _state = _stream.EndOfStream ? ParserState.End : ParserState.Start;
            _token = new PropertiesToken(PropertiesTokenType.Value, _textPool.ToString());
        }

        private bool HandleEscapeSequence()
        {
            StreamMark escapeStart = _stream.Position;

            // Eat '\\'
            _stream.Read();

            int escape = _stream.Read();

            switch (escape)
            {
                // Normal escapes
                case '0': _textPool.Append('\0'); break;
                case 'a': _textPool.Append('\a'); break;
                case 'f': _textPool.Append('\f'); break;
                case 'r': _textPool.Append('\r'); break;
                case 'n': _textPool.Append('\n'); break;
                case 't': _textPool.Append('\t'); break;
                case 'v': _textPool.Append('\v'); break;
                case '\\': _textPool.Append('\\'); break;
                case '\"': _textPool.Append('\"'); break;
                case '\'': _textPool.Append('\''); break;
                case '=': _textPool.Append('='); break;
                case ':': _textPool.Append(':'); break;
                case ' ': _textPool.Append('\x20'); break;

                // Logical lines
                // TODO: Maybe switch from this case into the default case and use a IsNewLine(...) check
                case '\r':
                case '\n':
                    _textLogicalLines = true;

                    // Move back cursor from read since ReadLineEnd() moves for us
                    if (escape != '\r' || _stream.Peek() != '\n')
                        _stream.Cursor.AdvanceColumn(-1);

                    _stream.ReadLineEnd();

                    // Skip trailing white-space of value or assigner
                    while (IsWhiteSpace(_stream.Peek()))
                        _stream.Read();

                    break;

                case 'u':
                    return ParseUnicodeEscape(in escapeStart, (char)escape);

                // Invalid escapes or end of stream
                default:
                    if ((escape == 'x' || escape == 'U') && Settings.AllUnicodeEscapes)
                    {
                        return ParseUnicodeEscape(in escapeStart, (char)escape);
                    }
                    else if (_stream.EndOfStream)
                    {
                        _textPool.Append('\\');
                    }
                    else if (!Settings.InvalidEscapes)
                    {
                        _tokenStart = escapeStart;
                        _tokenEnd = _stream.Position;
                        HandleError($"Invalid escape code \"\\{(char)escape}\" at line {_tokenStart.Line} column {_tokenStart.Column}!");
                        return false;
                    }
                    else
                    {
                        _textPool.Append((char)escape);
                    }
                    break;
            }

            return true;
        }

        private bool ParseUnicodeEscape(in StreamMark errEscapeStart, char identifier)
        {
            int codePoint = 0;

            // TODO: Maybe make 'u' && 'x' explicit
            byte codeLength = identifier == 'U' ? (byte)8 : (byte)4;

            for (byte i = 0; i < codeLength; i++)
            {
                int hex = ToHex(_stream.Read()); // '9' -> 9

                if (hex < 0 || hex > 15)
                {
                    if (i > 0 && identifier == 'x')
                        break;

                    return HandleUnicodeError(in errEscapeStart);
                }
                // First 2 must be 00 on valid '\U00xxxxxx'
                else if (identifier == 'U' && i < 2 && hex != 0)
                    return HandleUnicodeError(in errEscapeStart);

                codePoint = (codePoint << 4) + hex;

                // A valid UTF32 value is between 0x000000 and 0x10ffff, inclusive, 
                // and should not include surrogate codepoint values (0x00d800 ~ 0x00dfff)
                if (!(codePoint <= 0x10FFFF && (codePoint < 0xD800 || codePoint > 0xDFFF)))
                    return HandleUnicodeError(in errEscapeStart);
            }

            _textPool.Append(char.ConvertFromUtf32(codePoint));
            return true;
        }

        private bool HandleUnicodeError(in StreamMark errEscapeStart)
        {
            _tokenStart = errEscapeStart;
            _tokenEnd = _stream.Position;

            // TODO: Display sequence in error msg
            HandleError(
                $"Invalid Unicode escape sequence at line {_tokenStart.Line} column {_tokenStart.Column}!");

            return false;
        }

        private void HandleError(string message)
        {
            if (Settings.CloseOnEnd)
                Dispose();

            _state = ParserState.Error;
            _token = new PropertiesToken(PropertiesTokenType.Error, message);

            if (Settings.ThrowOnError)
                throw new PropertiesException(message);
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsWhiteSpace(int c) => c == '\x20' || c == '\t' || c == '\f';

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsLiteralAssigner(int c) => c == '=' || c == ':';

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsAssigner(int c) => IsLiteralAssigner(c) || IsWhiteSpace(c);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsNewLine(int c) => c == '\r' || c == '\n';

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsCommentHandle(int c) => c == '#' || c == '!';

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private int ToHex(int c) => c <= '9' ? c - '0' :
                (c <= 'F' ? c - 'A' + 10 : c - 'a' + 10);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsLatin1(int c) => c <= 0xFF;

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stream.Dispose();
            }

            _textPool.Length = 0;
            _textPool = null!;
            _stream = null!;
            _textLogicalLines = default;
            _commentHandle = default;
            _disposed = true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
                return;

            Dispose(Settings.CloseOnEnd);
            GC.SuppressFinalize(this);
        }
    }
}