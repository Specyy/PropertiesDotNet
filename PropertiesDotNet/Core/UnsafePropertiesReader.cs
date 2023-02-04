using System;
using System.Runtime.CompilerServices;
using System.Text;

using PropertiesDotNet.Utils;

namespace PropertiesDotNet.Core
{
    /// <summary>
    /// A fast ".properties" reader using unsafe string manipulation.
    /// </summary>
    public sealed class UnsafePropertiesReader : IPropertiesReader
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
        public StreamMark? TokenStart => _tokenStart;

        /// <inheritdoc/>
        public StreamMark? TokenEnd => _tokenEnd;

        /// <inheritdoc/>
        public bool HasLineInfo => true;

        /// <inheritdoc/>
        public event TokenRead? TokenRead;

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

        private bool EndOfStream => _index >= _document.Length;

        private PropertiesReaderSettings _settings;
        private string _document;
        private int _index;
        private bool _disposed;
        private PropertiesToken _token;
        private StringBuilder _textPool;

        private ParserState _state;
        private char _commentHandle;
        private bool _textLogicalLines;
        private StreamCursor _cursor;
        private StreamMark _tokenStart;
        private StreamMark _tokenEnd;

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
                if (ReadToken(document))
                {
                    TokenRead?.Invoke(this, _token);
                    return true;
                }
            }

            return false;
        }

        private unsafe bool ReadToken(char* document)
        {
            switch (_state)
            {
                case ParserState.Start:

                    // Remove white-space before token
                    while (!EndOfStream && (IsWhiteSpace(document[_index]) || IsNewLine(document[_index])))
                    {
                        if (IsNewLine(document[_index]))
                            ReadLineEnd(document);
                        else
                            Read();
                    }

                    if (!EndOfStream && IsCommentHandle(document[_index]))
                    {
                        if (!Settings.IgnoreComments)
                        {
                            _state = ParserState.Comment;
                            goto case ParserState.Comment;
                        }

                        SkipComments(document);
                    }

                    if (EndOfStream)
                    {
                        _state = ParserState.End;
                        goto case ParserState.End;
                    }
                    else
                    {
                        _state = ParserState.Key;
                        goto case ParserState.Key;
                    }

                case ParserState.Comment:
                    ReadComment(document);
                    return true;

                case ParserState.Key:
                    ReadKey(document);
                    return true;

                case ParserState.Assigner:
                    return ReadAssigner(document);

                case ParserState.Value:
                    ReadValue(document);

                    // Ignore return value because we basically only do this for the disposing behaviour
                    if (_state == ParserState.End)
                        ReadToken(document);

                    return true;

                case ParserState.Error:
                    _state = ParserState.End;
                    goto case ParserState.End;

                default:
                case ParserState.End:
                    if (Settings.CloseOnEnd)
                        Dispose();
                    return false;
            }
        }

        private unsafe void SkipComments(char* document)
        {
            // Skip over this and proceeding comments
            // Use iteration instead of recursion
            do
            {
                for (Read(); !EndOfStream; Read())
                {
                    if (IsNewLine(document[_index]))
                    {
                        ReadLineEnd(document);

                        // Remove white-space before token
                        while (!EndOfStream && (IsWhiteSpace(document[_index]) || IsNewLine(document[_index])))
                        {
                            if (IsNewLine(document[_index]))
                                ReadLineEnd(document);
                            else
                                Read();
                        }

                        break;
                    }
                }
            } while (IsCommentHandle(document[_index]));
        }

        private unsafe void ReadComment(char* document)
        {
            _tokenStart = _cursor.Position;

            _commentHandle = document[_index];
            Read();

            // Remove white-space before actual text
            while (!EndOfStream && IsWhiteSpace(document[_index]))
                Read();

            int textStartIndex = _index;

            while (!EndOfStream && !IsNewLine(document[_index]))
            {
                if (!Settings.AllCharacters && !IsLatin1(document[_index]))
                {
                    _tokenEnd = _tokenStart = _cursor.Position;
                    HandleError(
                       $"Unrecognized character '{document[_index]}' ({(ushort)document[_index]}) at line {_tokenStart.Line} column {_tokenStart.Column}!");
                    return;
                }
                Read();
            }

            _tokenEnd = _cursor.Position;

            _state = ParserState.Start;
            _token = new PropertiesToken(PropertiesTokenType.Comment,
                new string(document, textStartIndex, _index - textStartIndex));

            ReadLineEnd(document);
        }

        private unsafe void ReadKey(char* document)
        {
            _tokenStart = _cursor.Position;
            _textLogicalLines = false;
            // Use pointer sub-string if no escapes
            bool escapes = false;
            int textStartIndex = _index;

            while (!EndOfStream && !IsNewLine(document[_index]))
            {
                if (document[_index] == '\\')
                {
                    // Forced to use stringbuilder for escapes; can no longer do a simple sub-string
                    if (!escapes)
                    {
                        escapes = true;

                        if (_textPool is null)
                            _textPool = new StringBuilder();
                        else
                            _textPool.Length = 0;

                        _textPool.Append(new string(document, textStartIndex, _index - textStartIndex));
                    }

                    if (!HandleEscapeSequence(document))
                        return;
                }
                else if (IsAssigner(document[_index]))
                {
                    _tokenEnd = _cursor.Position;
                    _state = ParserState.Assigner;
                    // TODO: Allow for use for Span<T> in later .NET versions
                    _token = new PropertiesToken(PropertiesTokenType.Key,
                        escapes ? _textPool.ToString() : new string(document, textStartIndex, _index - textStartIndex));
                    return;
                }
                // Enforce ISO-8859-1
                else if (!Settings.AllCharacters && !IsLatin1(document[_index]))
                {
                    _tokenEnd = _tokenStart = _cursor.Position;
                    HandleError(
                       $"Unrecognized character '{document[_index]}' ({(ushort)document[_index]}) at line {_tokenStart.Line} column {_tokenStart.Column}!");
                    return;
                }
                else
                {
                    _textPool?.Append(document[_index]);
                    Read();
                }
            }

            _tokenEnd = _cursor.Position;
            _state = ParserState.Value;
            // TODO: Allow for use for Span<T> in later .NET versions
            _token = new PropertiesToken(PropertiesTokenType.Key,
                escapes ? _textPool.ToString() : new string(document, textStartIndex, _index - textStartIndex));
        }

        private unsafe bool ReadAssigner(char* document)
        {
            _tokenStart = _cursor.Position;
            char lastAssigner = document[_index];
            Read();

            if (IsWhiteSpace(lastAssigner))
            {
                // Consume whitespace until last or assigner
                while (!EndOfStream && IsWhiteSpace(document[_index]))
                    Read();

                // If we thought the assigner was a white-space but we were actually
                // just skipping the preceding white-space before the real assigner
                if (!EndOfStream && IsLiteralAssigner(document[_index]))
                {
                    _tokenStart = _cursor.Position;
                    lastAssigner = document[_index];
                    Read();
                }
                // Key with a bunch of trailing white-spaces but no value
                else if (EndOfStream || IsNewLine(document[_index]))
                {
                    _state = ParserState.Value;
                    return ReadToken(document);
                }
            }

            _tokenEnd = _cursor.Position;
            _state = ParserState.Value;
            _token = new PropertiesToken(PropertiesTokenType.Assigner, lastAssigner.ToString());
            return true;
        }

        private unsafe void ReadValue(char* document)
        {
            // Remove leading white-space before value
            while (!EndOfStream && IsWhiteSpace(document[_index]))
                Read();

            _tokenStart = _cursor.Position;
            _textLogicalLines = false;
            // Use pointer sub-string if no escapes
            bool escapes = false;
            int textStartIndex = _index;

            while (!EndOfStream && !IsNewLine(document[_index]))
            {
                if (document[_index] == '\\')
                {
                    // Forced to use stringbuilder for escapes; can no longer do a simple sub-string
                    if (!escapes)
                    {
                        escapes = true;

                        if (_textPool is null)
                            _textPool = new StringBuilder();
                        else
                            _textPool.Length = 0;

                        // TODO: Allow for use for Span<T> in later .NET versions
                        _textPool.Append(new string(document, textStartIndex, _index - textStartIndex));
                    }

                    if (!HandleEscapeSequence(document))
                        return;
                }
                // Enforce ISO-8859-1
                else if (!Settings.AllCharacters && !IsLatin1(document[_index]))
                {
                    _tokenEnd = _tokenStart = _cursor.Position;
                    HandleError(
                       $"Unrecognized character '{document[_index]}' ({(ushort)document[_index]}) at line {_tokenStart.Line} column {_tokenStart.Column}!");
                    return;
                }
                else
                {
                    _textPool?.Append(document[_index]);
                    Read();
                }
            }

            _tokenEnd = _cursor.Position;
            _state = EndOfStream ? ParserState.End : ParserState.Start;
            // TODO: Allow for use for Span<T> in later .NET versions
            _token = new PropertiesToken(PropertiesTokenType.Value,
                escapes ? _textPool.ToString() : new string(document, textStartIndex, _index - textStartIndex));
        }

        private unsafe bool HandleEscapeSequence(char* document)
        {
            StreamMark escapeStart = _cursor.Position;

            // Eat '\\'
            Read();

            char escape = document[_index];
            Read();

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
                    if (escape != '\r' || document[_index] != '\n')
                        _cursor.AdvanceColumn(-1);

                    ReadLineEnd(document);

                    // Skip trailing white-space of value or assigner
                    while (!EndOfStream && IsWhiteSpace(document[_index]))
                        Read();

                    break;

                case 'u':
                    return ParseUnicodeEscape(document, in escapeStart, escape);

                // Invalid escapes or end of stream
                default:
                    if ((escape == 'x' || escape == 'U') && Settings.AllUnicodeEscapes)
                    {
                        return ParseUnicodeEscape(document, in escapeStart, escape);
                    }
                    else if (EndOfStream)
                    {
                        _textPool.Append('\\');
                    }
                    else if (!Settings.InvalidEscapes)
                    {
                        _tokenStart = escapeStart;
                        _tokenEnd = _cursor.Position;
                        HandleError($"Invalid escape code \"\\{escape}\" at line {_tokenStart.Line} column {_tokenStart.Column}!");
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

        private unsafe bool ParseUnicodeEscape(char* document, in StreamMark errEscapeStart, char identifier)
        {
            int codePoint = 0;

            // TODO: Maybe make 'u' && 'x' explicit
            byte codeLength = identifier == 'U' ? (byte)8 : (byte)4;

            for (byte i = 0; i < codeLength; i++, Read())
            {
                int hex = ToHex(document[_index]); // '9' -> 9

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
            _tokenEnd = _cursor.Position;

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
        private unsafe void ReadLineEnd(char* document)
        {
            if (document[_index] == '\r' && _index + 1 < _document.Length && document[_index + 1] == '\n')
                Read();

            _index++;
            _cursor.AdvanceLine();
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void Read()
        {
            _cursor.AdvanceColumn(1);
            _index++;
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsWhiteSpace(char c) => c == '\x20' || c == '\t' || c == '\f';

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
        private bool IsNewLine(char c) => c == '\r' || c == '\n';

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsCommentHandle(char c) => c == '#' || c == '!';

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private int ToHex(char c) => c <= '9' ? c - '0' :
                (c <= 'F' ? c - 'A' + 10 : c - 'a' + 10);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsLatin1(char c) => c <= 0xFF;

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _document = null!;
            }

            if (_textPool != null)
                _textPool.Length = 0;
            _textPool = null!;
            _document = null!;
            _cursor = null!;
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