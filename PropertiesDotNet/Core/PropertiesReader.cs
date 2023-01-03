using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using PropertiesDotNet.Core.Events;

namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Represents a ".properties" reader that reads a text document into a stream of <see cref="PropertiesEvent"/>s.
    /// </summary>
    public sealed class PropertiesReader : IPropertiesReader
    {
        private static readonly Dictionary<char, char> _escapeCharactersToCodes = new Dictionary<char, char>()
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

        private static readonly char[] _assignmentChars = {'=', ':', ' ', '\t', '\f'};

        /// <inheritdoc/>
        public event EventRead? EventRead;

        /// <inheritdoc/>
        public PropertiesReaderSettings Settings { get; }

        /// <inheritdoc/>
        public bool HasDetailedStreamPosition => true;

        private readonly PropertiesStreamReader _stream;

        private PropertiesEvent? _lastEvent;
        private PropertiesEvent? _nextEvent;

        private bool _propertyStarted;

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
        public PropertiesReader(TextReader input, PropertiesReaderSettings? settings = null) : this(
            new PropertiesStreamReader(input), settings)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesReader"/>.
        /// </summary>
        /// <param name="input">The input document.</param>
        /// <param name="settings">The settings for this reader.</param>
        public PropertiesReader(PropertiesStreamReader input, PropertiesReaderSettings? settings = null)
        {
            Settings = settings ?? PropertiesReaderSettings.Default;
            _stream = input ?? throw new ArgumentNullException(nameof(input));

            _textPool = new StringBuilder();

            // Prepare next event
            _nextEvent = ReadNextEvent();
        }

        /// <inheritdoc/>
        public PropertiesEvent? Peek()
        {
            return _nextEvent;
        }

        /// <inheritdoc/>
        public PropertiesEvent? Read()
        {
            // End check
            if (_nextEvent is null)
                return default;

            // Alert subscribers
            EventRead?.Invoke(this, _nextEvent);

            _lastEvent = _nextEvent;
            _nextEvent = ReadNextEvent();

            return _lastEvent;
        }

        private PropertiesEvent? ReadNextEvent()
        {
            // Read property
            if (_propertyStarted)
            {
                // Check for key
                if (_lastEvent is PropertyStart)
                    return ReadText(true);

                // Check for assigner
                // Assigner cannot be line or stream end
                if (_lastEvent is Key && !_stream.IsNewLine() && !_stream.EndOfStream)
                    return ReadValueAssigner();

                // Check for value
                if (_lastEvent is ValueAssigner || _lastEvent is Key)
                    return ReadText(false);

                // If we are here, we have/had met a property end/error
                return ReadPropertyEnd();
            }

            // Check for comment or stream end
            // No comments or stream ends inside properties

            // Check for document end
            if (_lastEvent is DocumentEnd)
                return null;

            // Produce document start if not yet produced
            if (_lastEvent is null)
                return ReadDocumentStart();

            // Close on error
            if (_lastEvent is Error)
                return ReadDocumentEnd();

            if (ReadComment(out var comment))
                return comment;

            // Check sudden stream end
            if (_stream.EndOfStream)
                return ReadDocumentEnd();

            // If we are here, we have/had found a property start
            return ReadPropertyStart();
        }

        private DocumentStart ReadDocumentStart()
        {
            return new DocumentStart(_stream.CurrentPosition, _stream.CurrentPosition);
        }

        private bool ReadComment([MaybeNullWhen(false)] out Comment? comment)
        {
            // Comments can never have errors, so output can always be Comment

            // Read white-space before comment
            while (_stream.IsWhiteSpace() || _stream.IsNewLine())
                _stream.Read();

            // If we find a comment symbol
            if (_stream.Check(0, '#') || _stream.Check(0, '!'))
            {
                // Skip comment if required
                if (Settings.IgnoreComments)
                {
                    while (!_stream.IsNewLine() && !_stream.EndOfStream)
                        _stream.Read();

                    _stream.SkipLine();
                    comment = null;

                    // Check and skip comment on next line if there are any
                    // Return value should always be false
                    return ReadComment(out _);
                }

                // Read comment value
                comment = ReadCommentValue();
                return true;
            }

            // Not a comment
            comment = null;
            return false;
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private Comment ReadCommentValue()
        {
            var start = _stream.CurrentPosition;

            // Eat and resolve handle
            char handleChar;
            var handle =
                (handleChar = _stream.Read()) == '!' ? CommentHandle.Exclamation : CommentHandle.Hash;

            // Eat leading white-space
            _stream.SkipWhiteSpace();

            _textPool.Length = 0;

            while (!_stream.IsNewLine() && !_stream.EndOfStream)
                _textPool.Append(_stream.Read());

            var end = _stream.CurrentPosition;

            // Consume new line identifier
            _stream.SkipLine();

            // Read comment value
            return new Comment(start, end, handle, handleChar, _textPool.ToString());
        }

        private PropertyStart ReadPropertyStart()
        {
            _propertyStarted = true;
            return new PropertyStart(_stream.CurrentPosition, _stream.CurrentPosition);
        }

        private PropertiesEvent ReadText(bool isKey)
        {
            // Read leading white-space before text
            // Leading white-space read by comment check on key
            if (!isKey)
                _stream.SkipWhiteSpace();

            var start = _stream.CurrentPosition;
            var logicalLines = false;

            _textPool.Length = 0;

            while (true)
            {
                // Check for line or stream end
                if (_stream.IsNewLine() || _stream.EndOfStream)
                    break;

                // Check for escaped character
                if (_stream.Check(0, '\\'))
                {
                    if (!ReadEscapedText(ref logicalLines, out Error error))
                        return error;
                }

                // Check for value assignment
                else if (_stream.Check(offset: 0, _assignmentChars))
                {
                    // If plain key, this is a value assignment  
                    if (isKey)
                        break;

                    // If we are a value, we add the assigner
                    _textPool.Append(_stream.Read());
                }

                // Normal character
                else
                {
                    // Enforce ISO-8859-1
                    if (!Settings.AllCharacters && !_stream.IsLatin1())
                    {
                        var position = _stream.CurrentPosition;
                        return CreateError(
                            $"Unrecognized character '{_stream.Peek()}' ({(ushort) _stream.Peek()}) at line {position.Line} column {position.Column}!");
                    }

                    _textPool.Append(_stream.Read());
                }
            }

            var end = _stream.CurrentPosition;

            // End result
            return FinalizeText(ref start, ref end, isKey, logicalLines);
        }

        private bool ReadEscapedText(ref bool logicalLines, out Error? error)
        {
            // Save position of '\\'
            var escapeStart = _stream.CurrentPosition;

            // Eat '\\'
            _stream.Read();

            // Don't escape stream end
            if (_stream.EndOfStream)
            {
                error = CreateError($"Cannot escape stream end at line {escapeStart.Line} column {escapeStart.Column}!",
                    escapeStart);
                return false;
            }

            // Check for Unicode escape
            if (CheckUnicodeEscape(out var codeLength))
            {
                if (ReadUnicodeEscape(codeLength, out var codePoint))
                {
                    _textPool.Append(char.ConvertFromUtf32(codePoint));
                }
                else
                {
                    error = CreateError(
                        $"Invalid Unicode escape sequence at line {escapeStart.Line} column {escapeStart.Column}!",
                        escapeStart);
                    return false;
                }
            }

            // Check for logical line
            else if (_stream.IsNewLine())
            {
                if (!logicalLines)
                    logicalLines = true;

                // Skip new line identifier
                _stream.SkipLine();

                // Eat white-space until start
                _stream.SkipWhiteSpace();
            }

            // Check for normal escapes
            else if (_escapeCharactersToCodes.TryGetValue(_stream.Peek(), out var escapeCode))
            {
                _textPool.Append(escapeCode);
                _stream.Read();
            }

            // Invalid escape code
            else if (Settings.InvalidEscapes)
            {
                // Read invalid escape silently, as specification states
                // Enforce ISO-8859-1
                if (!Settings.AllCharacters && !_stream.IsLatin1())
                {
                    var position = _stream.CurrentPosition;
                    error = CreateError(
                        $"Unrecognized character '{_stream.Peek()}' ({(ushort) _stream.Peek()}) at line {position.Line} column {position.Column}!");
                    return false;
                }

                _textPool.Append(_stream.Read());
            }

            // Error on invalid escape
            else
            {
                error = CreateError($"Invalid escape code at line {escapeStart.Line} column {escapeStart.Column}!",
                    escapeStart);
                return false;
            }

            error = null;
            return true;
        }

        private bool CheckUnicodeEscape(out byte codeLength)
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

        private Text FinalizeText(ref StreamMark start, ref StreamMark end, bool isKey, bool logicalLines)
        {
            Text text;

            if (isKey)
            {
                // Skip trailing white-space, but not assigner
                if (_stream.IsWhiteSpace())
                    _stream.SkipWhiteSpace(1);

                // Read current if current is ' ' and next is ':' or '=' (since the case might be: "hello = world")
                // However, we must check if it is not empty (for cases like this: "= world"; we are only parsing syntax)
                if (_textPool.Length != 0 && _stream.Check(offset: 1, _assignmentChars))
                    _stream.Read();

                text = new Key(start, end, _textPool.ToString());
            }
            else
            {
                text = new Value(start, end, _textPool.ToString());
            }

            text.LogicalLines = logicalLines;
            return text;
        }

        private Error CreateError(string message, StreamMark? start = null, StreamMark? end = null)
        {
            var realStart = start ?? _stream.CurrentPosition;
            var realEnd = end ?? _stream.CurrentPosition;

            return Settings.ThrowOnError
                ? throw new PropertiesStreamException(realStart, realEnd, message)
                : new Error(realStart, realEnd, message);
        }

        private ValueAssigner ReadValueAssigner()
        {
            var start = _stream.CurrentPosition;
            var value = _stream.Read();
            var end = _stream.CurrentPosition;

            return new ValueAssigner(start, end, value);
        }

        private PropertyEnd ReadPropertyEnd()
        {
            _propertyStarted = false;
            return new PropertyEnd(_stream.CurrentPosition, _stream.CurrentPosition);
        }

        private DocumentEnd ReadDocumentEnd()
        {
            if (Settings.CloseStreamOnEnd)
                _stream.Dispose();

            return new DocumentEnd(_stream.CurrentPosition, _stream.CurrentPosition);
        }
    }
}