using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PropertiesDotNet.Core
{
    public sealed class PropertiesReader : IPropertiesReader
    {
        private static readonly Dictionary<char, char> _escapeCharsToCodes = new Dictionary<char, char>()
        {
            {'0', '\0'},
            {'a', '\a'},
            {'f', '\f'},
            {'r', '\r'},
            {'n', '\n'},
            {'t', '\t'},
            {'v', '\v'},
            {'\\', '\\'},
            {'"', '\"'},
            {'\'', '\''},
            {' ', '\x20'},
            {'!', '!'},
            {'#', '#'},
            {':', ':'},
            {'=', '='},
        };

        /// <inheritdoc/>
        public event TokenRead? TokenRead;

        /// <inheritdoc/>
        public PropertiesReaderSettings Settings
        {
            get => _settings;
            set => _settings = value is null ? _settings : value;
        }

        private PropertiesReaderSettings _settings;

        /// <inheritdoc/>
        public ref readonly PropertiesToken Token => ref _token;

        /// <summary>
        /// Represents a marker on the starting position of the current token.
        /// </summary>
        public ref readonly StreamMark TokenStart => ref _tokenStart;

        /// <summary>
        /// Represents a marker on the ending position of the current token.
        /// </summary>
        public ref readonly StreamMark TokenEnd => ref _tokenEnd;

        /// <summary>
        /// Returns whether the current token contains logical lines. This only applies to keys and values.
        /// </summary>
        public bool LogicalLines =>
            (_token.Type == PropertiesTokenType.Key || _token.Type == PropertiesTokenType.Value) &&
            _textLogicalLines;

        /// <summary>
        /// Returns the comment handle for the current token. This handle is either an
        /// exclamtion mark (!) or a pound symbol (#) or \0 if the next token is not a comment.
        /// </summary>
        public char CommentHandle => _token.Type == PropertiesTokenType.Comment ? _commentHandle : default;

        private PropertiesToken _token;
        private StreamMark _tokenStart;
        private StreamMark _tokenEnd;
        private char _commentHandle;
        private bool _textLogicalLines;

        private readonly PropertiesStreamReader _stream;
        private readonly StringBuilder _textPool;

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
            _stream = new PropertiesStreamReader(input ?? throw new ArgumentNullException(nameof(input)));

            _textPool = new StringBuilder();
        }

        /// <inheritdoc/>
        public bool MoveNext()
        {
            if (ReadNextToken())
            {
                if (Settings.CloseStreamOnEnd && _token.Type == PropertiesTokenType.Error)
                    _stream.Dispose();

                TokenRead?.Invoke(this, in _token);
                return true;
            }

            return false;
        }

        private bool ReadNextToken()
        {
            // Stream end check isn't entirely needed
            if (_token.Type == PropertiesTokenType.Error || _stream.EndOfStream)
                return false;

            // Check for comment or key or stream end
            if (_token.Type == PropertiesTokenType.None ||
                _token.Type == PropertiesTokenType.Comment ||
                _token.Type == PropertiesTokenType.Value)
            {
                // Remove white-space before token
                _stream.ReadWhiteOrLine();

                if (!_stream.EndOfStream && !ReadComment())
                    ReadText(true);
            }

            // Check for assigner
            // Assigner cannot be on line or stream end
            else if (_token.Type == PropertiesTokenType.Key &&
                !_stream.IsNewLine() && !_stream.EndOfStream)
            {
                ReadValueAssigner();
            }

            // Check for value
            else if (_token.Type == PropertiesTokenType.Assigner ||
                _token.Type == PropertiesTokenType.Key)
            {
                // White-space before value is not included
                _stream.ReadWhiteSpace();
                ReadText(false);
            }

            return true;
        }

        private bool ReadComment()
        {
            if (!_stream.Check(0, '#') && !_stream.Check(0, '!'))
                return false;

            if (Settings.IgnoreComments)
            {
                // Skip over this and proceeding comments
                // Use iteration instead of recursion
                while (_stream.Check(0, '#') || _stream.Check(0, '!'))
                {
                    for (; !_stream.EndOfStream; _stream.Read())
                    {
                        if (_stream.IsNewLine())
                        {
                            _stream.ReadLineEnd();
                            _stream.ReadWhiteOrLine();
                            break;
                        }
                    }
                }

                return false;
            }

            _tokenStart = _stream.CurrentPosition;

            _commentHandle = _stream.Read();
            _textPool.Length = 0;

            _stream.ReadWhiteOrLine();

            while (!_stream.IsNewLine() && !_stream.EndOfStream)
                _textPool.Append(_stream.Read());

            _tokenEnd = _stream.CurrentPosition;

            _stream.ReadLineEnd();

            _token = new PropertiesToken(PropertiesTokenType.Comment, _textPool.ToString());

            return true;
        }

        private void ReadValueAssigner()
            => _token = new PropertiesToken(PropertiesTokenType.Assigner, _stream.Read().ToString());

        private void ReadText(bool key)
        {
            _tokenStart = _stream.CurrentPosition;
            _textLogicalLines = false;
            _textPool.Length = 0;

            while (!_stream.EndOfStream && !_stream.IsNewLine())
            {
                // Check for escaped character
                if (_stream.Check(0, '\\'))
                {
                    if(!ReadEscaped())
                        return;
                }
                // Check for value assignment
                else if (key && (_stream.IsWhiteSpace() || _stream.Check(0, '=') || _stream.Check(0, ':')))
                {
                    break;
                }
                // Normal character
                else
                {
                    // Enforce ISO-8859-1
                    if (!Settings.AllCharacters && !_stream.IsLatin1())
                    {
                        _tokenEnd = _tokenStart = _stream.CurrentPosition;
                        CreateError(
                           $"Unrecognized character '{_stream.Peek()}' (\\u{(ushort)_stream.Peek()}) at line {_tokenStart.Line} column {_tokenStart.Column}!");
                        return;
                    }

                    _textPool.Append(_stream.Read());
                }
            }

            _tokenEnd = _stream.CurrentPosition;
            FinalizeText(key);
        }

        private bool ReadEscaped()
        {
            // Eat '\\'
            _stream.Read();

            // Don't escape stream end
            if (_stream.EndOfStream)
            {
                _tokenEnd = _tokenStart = _stream.CurrentPosition;
                CreateError($"Cannot escape stream end at line {_tokenStart.Line} column {_tokenStart.Column}!");
                return false;
            }

            // Check for Unicode escape
            else if (IsUnicodeEscape(out byte codeLength))
            {
                if (!ReadUnicodeEscape(codeLength, out int codePoint))
                {
                    _tokenEnd = _tokenStart = _stream.CurrentPosition;
                    CreateError(
                        $"Invalid Unicode escape sequence at line {_tokenStart.Line} column {_tokenStart.Column}!");
                    return false;

                }

                _textPool.Append(char.ConvertFromUtf32(codePoint));
            }

            // Check for normal escapes
            else if (_escapeCharsToCodes.TryGetValue(_stream.Peek(), out var escapeCode))
            {
                _textPool.Append(escapeCode);
                _stream.Read();
            }

            // Check for logical line
            else if (_stream.IsNewLine())
            {
                _textLogicalLines = true;

                _stream.ReadLineEnd();
                _stream.ReadWhiteSpace();
            }

            // Invalid escape code
            else if (Settings.InvalidEscapes)
            {
                // Read invalid escape silently, as specification states
                // Enforce ISO-8859-1
                if (!Settings.AllCharacters && !_stream.IsLatin1())
                {
                    _tokenEnd = _tokenStart = _stream.CurrentPosition;
                    CreateError(
                        $"Unrecognized character '{_stream.Peek()}' (\\u{(ushort)_stream.Peek()}) at line {_tokenStart.Line} column {_tokenStart.Column}!");
                    return false;
                }

                _textPool.Append(_stream.Read());
            }

            // Error on invalid escape
            else
            {
                _tokenEnd = _tokenStart = _stream.CurrentPosition;
                CreateError($"Invalid escape code at line {_tokenStart.Line} column {_tokenStart.Column}!");
                return false;
            }

            return true;
        }

        private bool IsUnicodeEscape(out byte codeLength)
        {
            // Check for default Unicode escape
            if (_stream.Check(0, 'u'))
            {
                codeLength = 4;
                return true;
            }

            if (Settings.AllUnicodeEscapes)
            {
                // Check for 1-4 digit escape
                if (_stream.Check(0, 'x'))
                {
                    codeLength = 4;
                    return true;
                }
                // Check for 8 digit escape

                if (_stream.Check(0, 'U'))
                {
                    codeLength = 8;
                    return true;
                }
            }

            codeLength = default;
            return false;
        }

        private bool ReadUnicodeEscape(byte codeLength, out int codePoint)
        {
            // Eat 'u', 'U', or 'x'
            var identifier = _stream.Read();

            codePoint = 0;

            for (byte i = 0; i < codeLength; i++)
            {
                var hex = _stream.ToHex(); // ASCII -> Number value

                if (hex < 0 || hex > 15)
                {
                    if (i > 0 && identifier == 'x')
                        break;

                    codePoint = -1;
                    return false;
                }

                //  F    F    F    F
                // 0000'0000 0000'0000
                // Shift to appropriate spot - first read = MSB, last read = LSB
                codePoint = (codePoint << 4) + hex;

                // A valid UTF32 value is between 0x000000 and 0x10ffff, inclusive, 
                // and should not include surrogate codepoint values (0x00d800 ~ 0x00dfff)
                if (codePoint <= 0x10FFFF && (codePoint < 0xD800 || codePoint > 0xDFFF))
                    continue;

                codePoint = -1;
                return false;
            }

            return true;
        }

        private void FinalizeText(bool key)
        {
            if (!key)
            {
                _token = new PropertiesToken(PropertiesTokenType.Value, _textPool.ToString());
                
                // Read until next comment/key/stream end
                _stream.ReadWhiteOrLine();
                return;
            }

            // Skip trailing white-space, but not assigner
            if (_stream.IsWhiteSpace())
                _stream.ReadWhiteSpace(1);

            // Read current if current is ' ' and next is ':' or '=' (since the case might be: "hello = world")
            if (_stream.IsWhiteSpace() &&
                (_stream.IsWhiteSpace(1) || _stream.Check(1, '=') || _stream.Check(1, ':')))
                _stream.Read();

            _token = new PropertiesToken(PropertiesTokenType.Key, _textPool.ToString());
        }

        private void CreateError(string message)
        {
            if (Settings.ThrowOnError)
                throw new PropertiesException(message);

            _token = new PropertiesToken(PropertiesTokenType.Error, message);
        }
    }
}
