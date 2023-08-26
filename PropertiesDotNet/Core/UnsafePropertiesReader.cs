using System.Runtime.CompilerServices;
using System.Text;

using PropertiesDotNet.Utils;

namespace PropertiesDotNet.Core
{
    /// <summary>
    /// A fast ".properties" document reader that implements a non-cached, 
    /// forward-only token generation scheme using unsafe string manipulation.
    /// </summary>
    /// <remarks>To be used in performance-critical environments.</remarks>
    public sealed class UnsafePropertiesReader : IPropertiesReader
    {
        /// <inheritdoc/>
        public PropertiesReaderSettings Settings
        {
            get => _settings;
            set => _settings = value ?? PropertiesReaderSettings.Default;
        }

        /// <inheritdoc/>
        public event TokenRead? TokenRead;

        /// <inheritdoc/>
        public PropertiesToken Token { get; private set; }

        /// <inheritdoc/>
        public StreamMark? TokenStart { get; private set; }

        /// <inheritdoc/>
        public StreamMark? TokenEnd { get; private set; }

        /// <inheritdoc/>
        public bool HasLineInfo => true;

        /// <summary>
        /// Returns the comment handle for the current token. This handle is either an
        /// exclamtion mark (!) or a pound symbol (#) or \0 if the current token is not a comment.
        /// </summary>
        public char CommentHandle => Token.Type == PropertiesTokenType.Comment ? _commentHandle : default;

        private bool EndOfStream => _index >= _document.Length;

        private PropertiesReaderSettings _settings;
        private readonly string _document;
        private int _index;
        private bool _disposed;
        private StringBuilder _textPool;

        private ReaderState _state;
        private char _commentHandle;
        private readonly StreamCursor _cursor;

        /// <summary>
        /// Creates a new <see cref="UnsafePropertiesReader"/>.
        /// </summary>
        /// <param name="input">A pointer to the input document.</param>
        /// <param name="length">The character length of the input document.</param>
        /// <param name="settings">The settings for this reader.</param>
        public unsafe UnsafePropertiesReader(char* input, int length, PropertiesReaderSettings? settings = null)
            : this(new string(input, 0, length), settings)
        {

        }

        /// <summary>
        /// Creates a new <see cref="UnsafePropertiesReader"/>.
        /// </summary>
        /// <param name="input">The input document.</param>
        /// <param name="settings">The settings for this reader.</param>
        public UnsafePropertiesReader(string input, PropertiesReaderSettings? settings = null)
        {
            _settings = settings ?? PropertiesReaderSettings.Default;
            _document = input;
            _textPool = null!; // Lazy init
            _cursor = new StreamCursor();
        }

        /// <inheritdoc/>
        public unsafe bool MoveNext()
        {
            fixed (char* document = _document)
            {
                if (ReadToken(document) || _state == ReaderState.Error)
                {
                    TokenRead?.Invoke(this, Token);
                    return true;
                }
            }

            return false;
        }

        private unsafe bool ReadToken(char* document)
        {
            switch (_state)
            {
                case ReaderState.Start:
                    // Remove white-space before token
                    while (!EndOfStream && IsBlank(document[_index]))
                        Read(document);

                    if (!EndOfStream && IsCommentHandle(document[_index]))
                    {
                        if (Settings.IgnoreComments)
                        {
                            do Read(document);
                            while (!IsLineEnd(document[_index]));

                            goto case ReaderState.Start;
                        }

                        _state = ReaderState.Comment;
                        goto case ReaderState.Comment;
                    }

                    if (EndOfStream)
                    {
                        _state = ReaderState.End;
                        goto case ReaderState.End;
                    }

                    _state = ReaderState.Key;
                    goto case ReaderState.Key;
                case ReaderState.Comment:
                    return ReadComment(document);

                case ReaderState.Key:
                case ReaderState.Value:
                    return ReadText(document);

                case ReaderState.Assigner:
                    return ReadAssigner(document);

                case ReaderState.Error:
                    _state = ReaderState.End;
                    goto case ReaderState.End;

                default:
                case ReaderState.End:
                    if (Settings.CloseOnEnd)
                        Dispose();
                    return false;
            }
        }

        private unsafe bool ReadComment(char* document)
        {
            TokenStart = _cursor.Position;

            _commentHandle = document[_index];
            Read(document);

            // Remove white-space before actual text
            while (!EndOfStream && IsWhiteSpace(document[_index]))
                Read(document);

            int textStartIndex = _index;

            while (!IsLineEnd(document[_index]))
            {
                if (!Settings.AllCharacters && !IsLatin1(document[_index]))
                {
                    TokenStart = TokenEnd = _cursor.Position;
                    CreateError($"Unrecognized character '{document[_index]}' ({(ushort)document[_index]}) at line {TokenStart?.Line} column {TokenStart?.Column}!");
                    return false;
                }

                Read(document);
            }

            TokenEnd = _cursor.Position;
            Token = new PropertiesToken(PropertiesTokenType.Comment, new string(document, textStartIndex, _index - textStartIndex));
            _state = ReaderState.Start;
            return true;
        }

        private unsafe bool ReadText(char* document)
        {
            // Remove leading white-space before value
            while (!EndOfStream && IsWhiteSpace(document[_index]))
                Read(document);

            TokenStart = _cursor.Position;

            // Use pointer sub-string if no escapes
            bool escapes = false;
            int textStartIndex = _index;

            while (!IsLineEnd(document[_index]))
            {
                if (document[_index] == '\\')
                {
                    // Forced to use stringbuilder for escapes; can no longer do a simple sub-string
                    if (!escapes)
                    {
                        escapes = true;
                        _textPool ??= new StringBuilder();
                        _textPool.Length = 0;

                        _textPool.Append(new string(document, textStartIndex, _index - textStartIndex));
                    }

                    if (!ReadEscape(document))
                        return false;
                }
                else if (_state == ReaderState.Key && IsAssigner(document[_index]))
                {
                    TokenEnd = _cursor.Position;
                    // TODO: Allow for use for Span<T> in later .NET versions?
                    Token = new PropertiesToken(PropertiesTokenType.Key,
                        escapes ? _textPool.ToString() : new string(document, textStartIndex, _index - textStartIndex));
                    _state = ReaderState.Assigner;
                    return true;
                }
                // Enforce ISO-8859-1
                else if (!Settings.AllCharacters && !IsLatin1(document[_index]))
                {
                    TokenStart = TokenEnd = _cursor.Position;
                    CreateError($"Unrecognized character '{document[_index]}' ({(ushort)document[_index]}) at line {TokenStart?.Line} column {TokenStart?.Column}!");
                    return false;
                }
                else
                {
                    if (escapes)
                        _textPool.Append(document[_index]);

                    Read(document);
                }
            }

            TokenEnd = _cursor.Position;

            if (_state == ReaderState.Key)
            {
                // TODO: Allow for use for Span<T> in later .NET versions?
                Token = new PropertiesToken(PropertiesTokenType.Key,
                        escapes ? _textPool.ToString() : new string(document, textStartIndex, _index - textStartIndex));
                _state = ReaderState.Value;
            }
            else
            {
                _state = EndOfStream ? ReaderState.End : ReaderState.Start;

                string text = escapes ? (Token.Type == PropertiesTokenType.Key && _textPool.Length == 0 ? null : _textPool.ToString())
                    : (Token.Type == PropertiesTokenType.Key && textStartIndex == _index ? null : new string(document, textStartIndex, _index - textStartIndex));

                // TODO: Allow for use for Span<T> in later .NET versions?
                Token = new PropertiesToken(PropertiesTokenType.Value, text);

                if (_state == ReaderState.End)
                    ReadToken(document);
            }

            return true;
        }

        private unsafe bool ReadAssigner(char* document)
        {
            TokenStart = _cursor.Position;
            char lastAssigner = document[_index];
            Read(document);

            if (IsWhiteSpace(lastAssigner))
            {
                // Consume whitespace until last or assigner
                while (!EndOfStream && IsWhiteSpace(document[_index]))
                    Read(document);

                // If we thought the assigner was a white-space but we were actually
                // just skipping the preceding white-space before the real assigner
                if (!EndOfStream && IsLiteralAssigner(document[_index]))
                {
                    TokenStart = _cursor.Position;
                    lastAssigner = document[_index];
                    Read(document);
                }
                // Key with a bunch of trailing white-spaces but no value
                else if (IsLineEnd(document[_index]))
                {
                    _state = ReaderState.Value;
                    return ReadToken(document);
                }
            }

            TokenEnd = _cursor.Position;
            _state = ReaderState.Value;
            Token = new PropertiesToken(PropertiesTokenType.Assigner, lastAssigner.ToString());
            return true;
        }

        private unsafe bool ReadEscape(char* document)
        {
            StreamMark escapeStart = _cursor.Position;

            // Eat '\\'
            Read(document);

            char escape = document[_index];
            Read(document);

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
                case '#': _textPool.Append('#'); break;
                case '!': _textPool.Append('!'); break;

                // Logical lines
                // TODO: Maybe switch from this case into the default case and use a IsNewLine(...) check
                case '\r':
                case '\n':
                    while (!EndOfStream && IsWhiteSpace(document[_index]))
                        Read(document);
                    break;

                case 'u':
                    return ReadUnicodeEscape(document, in escapeStart, escape);

                case 'x':
                case 'U':
                    if (Settings.AllUnicodeEscapes)
                        return ReadUnicodeEscape(document, in escapeStart, escape);

                    goto default;

                // Invalid escapes or end of stream
                default:
                    if (EndOfStream)
                    {
                        _textPool.Append('\\');
                    }
                    else if (!Settings.InvalidEscapes)
                    {
                        TokenStart = escapeStart;
                        TokenEnd = _cursor.Position;
                        CreateError($"Invalid escape code \"\\{escape}\" at line {escapeStart.Line} column {escapeStart.Column}!");
                        return false;
                    }
                    else
                    {
                        _textPool.Append(escape);
                    }
                    break;
            }

            return true;
        }

        private unsafe bool ReadUnicodeEscape(char* document, in StreamMark errEscapeStart, char identifier)
        {
            int codePoint = 0;
            int codeLength = identifier == 'U' ? (byte)8 : (byte)4;

            for (int i = 0; i < codeLength; i++)
            {
                int hex = ReadHex(document[_index]); // '9' -> 9
                Read(document);

                if (hex < 0 || hex > 15)
                {
                    if (i > 0 && identifier == 'x')
                        break;

                    return CreateUnicodeError(in errEscapeStart);
                }
                // First 2 must be 00 on valid '\U00xxxxxx'
                else if (identifier == 'U' && i < 2 && hex != 0)
                    return CreateUnicodeError(in errEscapeStart);

                codePoint = (codePoint << 4) + hex;

                // A valid UTF32 value is between 0x000000 and 0x10ffff, inclusive, 
                // and should not include surrogate codepoint values (0x00d800 ~ 0x00dfff)
                if (!(codePoint <= 0x10FFFF && (codePoint < 0xD800 || codePoint > 0xDFFF)))
                    return CreateUnicodeError(in errEscapeStart);
            }

            _textPool.Append(char.ConvertFromUtf32(codePoint));
            return true;
        }

        private bool CreateUnicodeError(in StreamMark errEscapeStart)
        {
            TokenStart = errEscapeStart;
            TokenEnd = _cursor.Position;

            // TODO: Display sequence in error msg
            CreateError($"Invalid Unicode escape sequence at line {errEscapeStart.Line} column {errEscapeStart.Column}!");
            return false;
        }

        private void CreateError(string message)
        {
            if (Settings.CloseOnEnd)
                Dispose();

            _state = ReaderState.Error;
            Token = new PropertiesToken(PropertiesTokenType.Error, message);

            if (Settings.ThrowOnError)
                throw new PropertiesException(message);
        }

        private unsafe void Read(char* document)
        {
            char read = document[_index++];

            if (IsNewLine(read))
            {
                // 2 skips on CRLF
                if (read == '\r' && _index < _document.Length && document[_index] == '\n')
                {
                    _index++;
                    _cursor.AdvanceColumn(1);
                }

                _cursor.AdvanceLine();
            }
            else if (read == '\t')
            {
                _cursor.Column += 4;
                _cursor.AbsoluteOffset++;
            }
            else
            {
                _cursor.AdvanceColumn(1);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
                return;

            if (_textPool != null)
                _textPool.Length = 0;

            _disposed = true;
        }

        #region Character analyzing
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsWhiteSpace(char c) => c == '\x20' || c == '\t' || c == '\f';

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsNewLine(char c) => c == '\r' || c == '\n';

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsBlank(char c) => IsWhiteSpace(c) || IsNewLine(c);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsLineEnd(char c) => IsNewLine(c) || EndOfStream;

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsLiteralAssigner(char c) => c == '=' || c == ':';

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsAssigner(char c) => IsLiteralAssigner(c) || IsWhiteSpace(c);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsCommentHandle(char c) => c == '#' || c == '!';

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private int ReadHex(char c) => c <= '9' ? c - '0' :
                (c <= 'F' ? c - 'A' + 10 : c - 'a' + 10);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsLatin1(char c) => c <= 0xFF && c >= 0;
        #endregion
    }
}