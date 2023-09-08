using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

using PropertiesDotNet.Utils;

namespace PropertiesDotNet.Core
{
    enum ReaderState : byte
    {
        Start,
        Comment,
        Key,
        Assigner,
        Value,
        Error,
        End
    }

    /// <summary>
    /// A ".properties" document reader that implements a non-cached, forward-only token generation scheme.
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
        /// exclamtion mark (!) or a pound symbol (#) or null if the current token is not a comment.
        /// </summary>
        public char? CommentHandle => Token.Type == PropertiesTokenType.Comment ? _commentHandle : null;

        private bool EndOfStream => _stream.Peek() == -1;

        private readonly TextReader _stream;
        private readonly StreamCursor _cursor;
        private readonly StringBuilder _textPool;
        private bool _disposed;

        private PropertiesReaderSettings _settings;
        private ReaderState _state;
        private char? _commentHandle;

        /// <summary>
        /// Creates a new <see cref="PropertiesReader"/>.
        /// </summary>
        /// <param name="document">The .properties document as a string.</param>
        /// <param name="settings">The settings for this reader.</param>
        public PropertiesReader(string document, PropertiesReaderSettings? settings = null) : this(new StringReader(document),
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
            _stream = input ?? throw new ArgumentNullException(nameof(input));
            _cursor = new StreamCursor();
            _textPool = new StringBuilder();
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesReader"/> from the file <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The file path of the input document.</param>
        /// <param name="settings">The settings for this reader.</param>
        /// <returns>An appropriate <see cref="PropertiesReader"/>.</returns>
        public static PropertiesReader FromFile(string path, PropertiesReaderSettings? settings = null) => new PropertiesReader(File.OpenRead(path), settings);

        /// <summary>
        /// Creates a new <see cref="PropertiesReader"/> from the <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The input document.</param>
        /// <param name="settings">The settings for this reader.</param>
        /// <returns>An appropriate <see cref="PropertiesReader"/>.</returns>
        public static PropertiesReader FromFile(FileInfo file, PropertiesReaderSettings? settings = null) => new PropertiesReader(file.OpenRead(), settings);

        /// <inheritdoc/>
        public bool MoveNext()
        {
            if (ReadToken() || _state == ReaderState.Error)
            {
                TokenRead?.Invoke(this, Token);
                return true;
            }

            return false;
        }

        private bool ReadToken()
        {
            switch (_state)
            {
                case ReaderState.Start:
                    // Remove white-space before token
                    while (IsBlank(_stream.Peek()))
                        Read();

                    if (IsCommentHandle(_stream.Peek()))
                    {
                        if (Settings.IgnoreComments)
                        {
                            do Read();
                            while (!IsLineEnd(_stream.Peek()));

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
                    return ReadComment();

                case ReaderState.Key:
                case ReaderState.Value:
                    return ReadText();

                case ReaderState.Assigner:
                    return ReadAssigner();

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

        private bool ReadComment()
        {
            TokenStart = _cursor.Position;

            _commentHandle = (char)Read();
            _textPool.Length = 0;

            // Remove white-space before actual text
            while (IsWhiteSpace(_stream.Peek()))
                Read();

            while (!IsLineEnd(_stream.Peek()))
            {
                if (!Settings.AllCharacters && !IsLatin1(_stream.Peek()))
                {
                    TokenStart = TokenEnd = _cursor.Position;
                    CreateError($"Unrecognized character '{(char)_stream.Peek()}' ({(ushort)_stream.Peek()}) at line {TokenStart?.Line} column {TokenStart?.Column}!");
                    return false;
                }

                _textPool.Append((char)Read());
            }

            TokenEnd = _cursor.Position;
            Token = new PropertiesToken(PropertiesTokenType.Comment, _textPool.ToString());
            _state = ReaderState.Start;
            return true;
        }

        private bool ReadText()
        {
            // Remove leading white-space before value
            if (_state == ReaderState.Value)
                while (IsWhiteSpace(_stream.Peek()))
                    Read();

            _textPool.Length = 0;
            TokenStart = _cursor.Position;

            while (!IsLineEnd(_stream.Peek()))
            {
                if (_stream.Peek() == '\\')
                {
                    if (!ReadEscape())
                        return false;
                }
                else if (_state == ReaderState.Key && IsAssigner(_stream.Peek()))
                {
                    TokenEnd = _cursor.Position;
                    Token = new PropertiesToken(PropertiesTokenType.Key, _textPool.ToString());
                    _state = ReaderState.Assigner;
                    return true;
                }
                // Enforce ISO-8859-1
                else if (!Settings.AllCharacters && !IsLatin1(_stream.Peek()))
                {
                    TokenStart = TokenEnd = _cursor.Position;
                    CreateError($"Unrecognized character '{(char)_stream.Peek()}' ({(ushort)_stream.Peek()}) at line {TokenStart?.Line} column {TokenStart?.Column}!");
                    return false;
                }
                else
                {
                    _textPool.Append((char)Read());
                }
            }

            TokenEnd = _cursor.Position;

            if (_state == ReaderState.Key)
            {
                Token = new PropertiesToken(PropertiesTokenType.Key, _textPool.ToString());
                _state = ReaderState.Value;
            }
            else
            {
                _state = EndOfStream ? ReaderState.End : ReaderState.Start;
                Token = new PropertiesToken(PropertiesTokenType.Value,
                    Token.Type == PropertiesTokenType.Key && _textPool.Length == 0 ? null : _textPool.ToString());

                if (_state == ReaderState.End)
                    ReadToken();
            }

            return true;
        }

        private bool ReadAssigner()
        {
            TokenStart = _cursor.Position;
            int lastAssigner = Read();

            if (IsWhiteSpace(lastAssigner))
            {
                // Consume whitespace until last or assigner
                while (IsWhiteSpace(_stream.Peek()))
                    Read();

                // If we thought the assigner was a white-space but we were really
                // just skipping the preceding white-space before the actual assigner
                if (IsLiteralAssigner(_stream.Peek()))
                {
                    TokenStart = _cursor.Position;
                    lastAssigner = Read();
                }
                // Key with a bunch of trailing white-spaces but no value
                else if (IsLineEnd(_stream.Peek()))
                {
                    _state = ReaderState.Value;
                    return ReadToken();
                }
            }

            TokenEnd = _cursor.Position;
            Token = new PropertiesToken(PropertiesTokenType.Assigner, ((char)lastAssigner).ToString());
            _state = ReaderState.Value;
            return true;
        }

        private bool ReadEscape()
        {
            StreamMark escapeStart = _cursor.Position;

            // Eat '\\'
            Read();

            int escape = Read();

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
                    while (IsWhiteSpace(_stream.Peek()))
                        Read();
                    break;

                case 'u':
                    return ReadUnicodeEscape(in escapeStart, escape);
                case 'x':
                case 'U':
                    if (Settings.AllUnicodeEscapes)
                        return ReadUnicodeEscape(in escapeStart, escape);

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
                        CreateError($"Invalid escape code \"\\{(char)escape}\" at line {escapeStart.Line} column {escapeStart.Column}!");
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

        private bool ReadUnicodeEscape(in StreamMark errEscapeStart, int identifier)
        {
            int codePoint = 0;
            int codeLength = identifier == 'U' ? 8 : 4;

            for (int i = 0; i < codeLength; i++, Read())
            {
                int hex = ReadHex(_stream.Peek()); // '9' -> 9

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

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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

        private int Read()
        {
            int read;
            if ((read = _stream.Read()) != -1)
            {
                if (IsNewLine(read))
                {
                    // 2 skips on CRLF
                    if (read == '\r' && _stream.Peek() == '\n')
                    {
                        _stream.Read();
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

            return read;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
                return;

            if (Settings.CloseOnEnd)
                _stream.Dispose();

            _textPool.Length = 0;
            _disposed = true;
        }

        #region Character analyzing
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsWhiteSpace(int c) => c == '\x20' || c == '\t' || c == '\f';

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsNewLine(int c) => c == '\r' || c == '\n';

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsBlank(int c) => IsWhiteSpace(c) || IsNewLine(c);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsLineEnd(int c) => IsNewLine(c) || EndOfStream;

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
        private bool IsCommentHandle(int c) => c == '#' || c == '!';

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private int ReadHex(int c) => c <= '9' ? c - '0' :
                (c <= 'F' ? c - 'A' + 10 : c - 'a' + 10);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsLatin1(int c) => c <= 0xFF && c >= 0;
        #endregion
    }
}