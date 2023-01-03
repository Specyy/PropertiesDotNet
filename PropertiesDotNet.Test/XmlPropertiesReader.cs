#if !NET35 && !NETSTANDARD1_3
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

using PropertiesDotNet.Core.Events;

namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Represents a ".properties" reader that reads a XML document into a stream of <see cref="PropertiesEvent"/>s.<br/>
    /// The underlying document must have the following DOCTYPE declaration, as specified by the specification:<br/>
    /// <br/>
    /// <i>
    ///     &lt;!DOCTYPE properties SYSTEM "http://java.sun.com/dtd/properties.dtd"&gt;
    /// </i>
    /// 
    /// <br/>
    /// <br/>
    ///     
    /// And must follow the following XML DTD format (link to document: <b><a href="http://java.sun.com/dtd/properties.dtd"/></b>):<br/>
    /// <br/>
    /// <i>
    ///     &lt;? xml version="1.0" encoding="UTF-8"?&gt;
    ///     <br/>
    ///     <br/>
    ///     &lt;!-- DTD for properties --&gt;<br/>
    ///     &lt;!ELEMENT properties (comment?, entry* ) &gt;<br/>
    ///     &lt;!ATTLIST properties version CDATA #FIXED "1.0"&gt;<br/>
    ///     &lt;!ELEMENT comment (#PCDATA) &gt;<br/>
    ///     &lt;!ELEMENT entry (#PCDATA) &gt;<br/>
    ///     &lt;!ATTLIST entry key CDATA #REQUIRED&gt;
    /// </i>
    /// </summary>
    public sealed class XmlPropertiesReader : IPropertiesReader
    {
        private static XmlReaderSettings DefaultSettings => new XmlReaderSettings()
        {
            ValidationType = ValidationType.DTD,
            DtdProcessing = DtdProcessing.Parse,
            XmlResolver = new XmlUrlResolver(),
        };

        /// <inheritdoc/>
        public PropertiesReaderSettings Settings { get; }

        /// <inheritdoc/>
        public bool HasDetailedStreamPosition => false;

        /// <inheritdoc/>
        public event EventRead? EventRead;

        private StreamMark CurrentPosition
        {
            get
            {
                IXmlLineInfo lineInfo = (IXmlLineInfo)_reader;
                return new StreamMark((uint)lineInfo.LinePosition, (uint)lineInfo.LineNumber, 0);
            }
        }

        private PropertiesEvent? _lastEvent;
        private PropertiesEvent? _nextEvent;
        private bool _propertyStarted;

        private readonly XmlReader _reader;

        /// <summary>
        /// Creates a new <see cref="XmlPropertiesReader"/>.
        /// </summary>
        /// <param name="input">The input document.</param>
        /// <param name="settings">The settings for this reader.</param>
        /// <exception cref="NotSupportedException">If the default <see cref="XmlReader"/> implementation
        /// does not implement <see cref="IXmlLineInfo"/>.</exception>
        public XmlPropertiesReader(string input, PropertiesReaderSettings? settings = null) : this(new StringReader(input), settings)
        {

        }

        /// <summary>
        /// Creates a new <see cref="XmlPropertiesReader"/>.
        /// </summary>
        /// <param name="input">The input document.</param>
        /// <param name="settings">The settings for this reader.</param>
        /// <exception cref="NotSupportedException">If the default <see cref="XmlReader"/> implementation
        /// does not implement <see cref="IXmlLineInfo"/>.</exception>
        public XmlPropertiesReader(Stream input, PropertiesReaderSettings? settings = null) : this(new StreamReader(input, true), settings)
        {

        }

        /// <summary>
        /// Creates a new <see cref="XmlPropertiesReader"/>.
        /// </summary>
        /// <param name="input">The input document.</param>
        /// <param name="settings">The settings for this reader.</param>
        /// <exception cref="NotSupportedException">If the default <see cref="XmlReader"/> implementation
        /// does not implement <see cref="IXmlLineInfo"/>.</exception>
        public XmlPropertiesReader(TextReader input, PropertiesReaderSettings? settings = null)
        {
            Settings = settings ?? PropertiesReaderSettings.Default;

            _reader = XmlReader.Create(input, DefaultSettings);

            // Reader must have line info for event position
            if (_reader is IXmlLineInfo info && info.HasLineInfo())
            {
                // Prepare next event
                _nextEvent = ReadNextEvent();
            }
            else
            {
                throw new NotSupportedException($"XML Reading is not supported \"{input.GetType()}\"!");
            }
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
                return null;

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

                // No assigner in XML document

                // Check for value
                else if (_lastEvent is Key)
                    return ReadText(false);

                // If we are here, we have/had met a property end/error
                return ReadPropertyEnd();
            }

            // Check for comment or stream end
            // No comments or stream ends inside properties
            else
            {
                // Check for document end
                if (_lastEvent is DocumentEnd)
                    return null;

                // Produce document start if not yet produced
                if (_lastEvent is null)
                    return ReadDocumentStart()!;

                if (ReadComment(out Comment? comment))
                    return comment!;

                // Check for end element
                // Check sudden stream end
                if ((_reader.NodeType == XmlNodeType.EndElement && _reader.Name == "properties") ||
                    _reader.ReadState == ReadState.EndOfFile ||
                    _lastEvent is Error)
                    return ReadDocumentEnd()!;

                // If we are here, we have/had found a property start
                return ReadPropertyStart();
            }
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private DocumentStart? ReadDocumentStart()
        {
            return new DocumentStart(CurrentPosition, CurrentPosition);
        }

        private bool ReadComment([NotNullWhen(true)] out Comment? comment)
        {
            // Comments can never have errors, so output can always be Comment

            // Read white-space before comment
            //while (_stream.IsWhiteSpace() || _stream.IsNewLine())
            //    _stream.Read();

            //// If we find a comment symbol
            //if (_stream.Check(0, '#') || _stream.Check(0, '!'))
            //{
            //    // Skip comment is required
            //    if (Settings.IgnoreComments)
            //    {
            //        while (!_stream.IsNewLine() && !_stream.EndOfStream)
            //            _stream.Read();

            //        _stream.SkipLine();
            //        comment = null;

            //        // Skip comment on next line if there are any
            //        // Return value should always be false
            //        ReadComment(out _);
            //        return false;
            //    }

            //    // Read comment value
            //    comment = ReadCommentValue();
            //    return true;
            //}

            // Not a comment
            comment = null;
            return false;
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private Comment ReadCommentValue()
        {
            //StreamMark start = _stream.CurrentPosition;

            //// Eat and resolve handle
            //char handleChar;
            //CommentHandle handle = (handleChar = _stream.Read()) == '!' ? CommentHandle.Exclamation : CommentHandle.Hash;

            //// Eat leading white-space
            //_stream.SkipWhiteSpace();

            //_textPool.Length = 0;

            //while (!_stream.IsNewLine() && !_stream.EndOfStream)
            //    _textPool.Append(_stream.Read());

            //StreamMark end = _stream.CurrentPosition;

            //// Consume new line identifier
            //_stream.SkipLine();

            // Read comment value
            //return new Comment(start, end, handle, handleChar, _textPool.ToString());
            return default!;
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private PropertyStart ReadPropertyStart()
        {
            _propertyStarted = true;
            return new PropertyStart(CurrentPosition, CurrentPosition);
        }

        private PropertiesEvent ReadText(bool isKey)
        {
            // Read leading white-space before text
            // Leading white-space read by comment check on key
            //if (!isKey)
            //    _stream.SkipWhiteSpace();

            //StreamMark start = _stream.CurrentPosition;
            //bool logicalLines = false;

            //_textPool.Length = 0;

            //while (true)
            //{
            //    // Check for line or stream end
            //    if (_stream.IsNewLine() || _stream.EndOfStream)
            //        break;

            //    // Check for escaped character
            //    if (_stream.Check(0, '\\'))
            //    {
            //        // Save position of '\\'
            //        StreamMark escapeStart = _stream.CurrentPosition;

            //        // Eat '\\'
            //        _stream.Read();

            //        // Don't escape stream end
            //        if (_stream.EndOfStream)
            //            return CreateError($"Cannot escape stream end at line {escapeStart.Line} column {escapeStart.Column}!", escapeStart);

            //        // Check for Unicode escape
            //        if (CheckUnicodeEscape(out int codeLength))
            //        {
            //            if (ReadUnicodeEscape(codeLength, out int codePoint))
            //                _textPool.Append(char.ConvertFromUtf32(codePoint));
            //            else
            //                return CreateError($"Invalid Unicode escape sequence at line {escapeStart.Line} column {escapeStart.Column}!", escapeStart);
            //        }

            //        // Check for logical line
            //        else if (_stream.IsNewLine())
            //        {
            //            if (!logicalLines)
            //                logicalLines = true;

            //            // SKip new line identifier
            //            _stream.SkipLine();

            //            // Eat white-space until start
            //            _stream.SkipWhiteSpace();
            //        }

            //        // Check for normal escapes
            //        else if (SpecificationConstants.EscapeCharactersToCodes.TryGetValue(_stream.Peek(), out char escapeCode))
            //        {
            //            _textPool.Append(escapeCode);
            //            _stream.Read();
            //        }

            //        // Invalid escape code
            //        else if (Settings.InvalidEscapes)
            //        {
            //            // Read invalid escape silently, as specification states

            //            // Enforce ISO-8859-1
            //            if (!Settings.AllCharacters && !_stream.IsLatin1())
            //            {
            //                StreamMark position = _stream.CurrentPosition;
            //                return CreateError($"Unrecognized character '{_stream.Peek()}' at line {position.Line} column {position.Column}!");
            //            }

            //            _textPool.Append(_stream.Read());
            //        }
            //        else
            //        {
            //            // Error on invalid escape if we should
            //            return CreateError($"Invalid escape code at line {escapeStart.Line} column {escapeStart.Column}!", escapeStart);
            //        }
            //    }

            //    // Check for value assignment
            //    else if (_stream.Check(checks: SpecificationConstants.Assignments))
            //    {
            //        // If plain key, this is a value assignment  
            //        if (isKey)
            //            break;

            //        // If we are a value, we add the assigner
            //        _textPool.Append(_stream.Read());
            //    }

            //    // Normal character
            //    else
            //    {
            //        // Enforce ISO-8859-1
            //        if (!Settings.AllCharacters && !_stream.IsLatin1())
            //        {
            //            StreamMark position = _stream.CurrentPosition;
            //            return CreateError($"Unrecognized character '{_stream.Peek()}' at line {position.Line} column {position.Column}!");
            //        }

            //        _textPool.Append(_stream.Read());
            //    }
            //}

            //StreamMark end = _stream.CurrentPosition;

            //// End result
            //Text text;

            //if (isKey)
            //{
            //    // Skip trailing white-space, but not assigner
            //    if (_stream.IsWhiteSpace())
            //        _stream.SkipWhiteSpace(1);

            //    if (_stream.Check(1, checks: SpecificationConstants.Assignments))
            //        _stream.Read();

            //    text = new Key(start, end, _textPool.ToString());
            //}
            //else
            //{
            //    text = new Value(start, end, _textPool.ToString());
            //}

            //text.LogicalLines = logicalLines;
            //return text;

            return null;
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool CheckUnicodeEscape(out int codeLength)
        {
            // Check for default Unicode escape
            //if (_stream.Check(0, 'u'))
            //{
            //    codeLength = 4;
            //    return true;
            //}

            //else if (Settings.AllUnicodeEscapes)
            //{
            //    // Check for 1-4 digit escape
            //    if (_stream.Check(0, 'x'))
            //    {
            //        codeLength = 4;
            //        return true;
            //    }
            //    // Check for 8 digit escape
            //    else if (_stream.Check(0, 'U'))
            //    {
            //        codeLength = 8;
            //        return true;
            //    }
            //}

            codeLength = default;
            return false;
        }

        private bool ReadUnicodeEscape(int codeLength, out int codePoint)
        {
            // Eat 'u', 'U', or 'x'
            //char identifier = _stream.Read();

            //codePoint = 0;

            //for (int i = 0; i < codeLength; i++)
            //{
            //    int hex = _stream.ToHex(); // ASCII -> Number value

            //    if (hex < 0 || hex > 15)
            //    {
            //        if (i > 0 && identifier == 'x')
            //            break;

            //        codePoint = -1;
            //        return false;
            //    }

            //    //  F    F    F    F
            //    // 0000'0000 0000'0000
            //    // Shift to appropriate spot - first read = MSB, last read = LSB
            //    // x << 4 = x * 16
            //    codePoint = (codePoint << 4) + hex;

            //    // A valid UTF32 value is between 0x000000 and 0x10ffff, inclusive, 
            //    // and should not include surrogate codepoint values (0x00d800 ~ 0x00dfff)
            //    if (codePoint > 0x10FFFF || (codePoint >= 0xD800 && codePoint <= 0xDFFF))
            //    {
            codePoint = -1;
            return false;
            //    }
            //}

            //return true;
        }

        private Error CreateError(string message, StreamMark? start = null, StreamMark? end = null)
        {
            //StreamMark realStart = start ?? _stream.CurrentPosition;
            //StreamMark realEnd = end ?? _stream.CurrentPosition;

            //return Settings.ThrowOnError ? throw new PropertiesReaderException(realStart, realEnd, message) : new Error(realStart, realEnd, message);
            return default;
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private ValueAssigner ReadValueAssigner()
        {
            //StreamMark start = _stream.CurrentPosition;
            //char value = _stream.Read();
            //StreamMark end = _stream.CurrentPosition;

            //return new ValueAssigner(start, end, value);
            return default;
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private PropertyEnd ReadPropertyEnd()
        {
            _propertyStarted = false;
            return new PropertyEnd(CurrentPosition, CurrentPosition);
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private DocumentEnd? ReadDocumentEnd()
        {
            // if (Settings.CloseStreamOnEnd)
            //    _stream.Dispose();

            return new DocumentEnd(CurrentPosition, CurrentPosition);
        }
    }
}
#endif
