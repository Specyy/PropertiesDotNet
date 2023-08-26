using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

using PropertiesDotNet.Utils;

namespace PropertiesDotNet.Core
{
    enum WriterState : byte
    {
        CommentOrKey,
        ValueOrAssigner,
        Value
    }

    /// <summary>
    /// Represents a class that writes <see cref="PropertiesToken"/>s into a stream as text.
    /// </summary>
    public sealed class PropertiesWriter : IPropertiesWriter
    {
        /// <inheritdoc/>
        public PropertiesWriterSettings Settings
        {
            get => _settings;
            set => _settings = value ?? PropertiesWriterSettings.Default;
        }

        /// <inheritdoc/>
        public event TokenWritten? TokenWritten;

        private PropertiesWriterSettings _settings;
        private bool _disposed;
        private uint _flushCounter;

        private WriterState _state;
        private readonly StringBuilder _textPool;
        private readonly StreamCursor _cursor;
        private readonly TextWriter _stream;

        /// <summary>
        /// Creates a new <see cref="PropertiesWriter"/>.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="settings">The settings for this writer.</param>
        public PropertiesWriter(StringBuilder output, PropertiesWriterSettings? settings = null) : this(new StringWriter(output), settings)
        {

        }

        /// <summary>
        /// Creates a new <see cref="PropertiesWriter"/>.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="settings">The settings for this writer.</param>
        public PropertiesWriter(Stream output, PropertiesWriterSettings? settings = null) : this(new StreamWriter(output), settings)
        {

        }

        /// <summary>
        /// Creates a new <see cref="PropertiesWriter"/> that writes to the <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath">The path to the .properties file.</param>
        /// <param name="settings">The settings for this writer.</param>
        public PropertiesWriter(string filePath, PropertiesWriterSettings? settings = null) : this(File.OpenWrite(filePath), settings)
        {

        }

        /// <summary>
        /// Creates a new <see cref="PropertiesWriter"/>.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="settings">The settings for this writer.</param>
        public PropertiesWriter(TextWriter output, PropertiesWriterSettings? settings = null)
        {
            _settings = settings ?? PropertiesWriterSettings.Default;
            _stream = output ?? throw new ArgumentNullException(nameof(output));
            _textPool = new StringBuilder();
            _cursor = new StreamCursor();
        }

        /// <inheritdoc/>
        public bool Write(PropertiesToken token) => token.Type switch
        {
            PropertiesTokenType.Comment => WriteComment(token.Text),
            PropertiesTokenType.Key => WriteKey(token.Text),
            PropertiesTokenType.Assigner => token.Text.Length > 1 ?
                                    (Settings.ThrowOnError ? throw new PropertiesException("Assigner must be '=', ':' or any type of white-space!") : false)
                                    : WriteAssigner(token.Text[0]),
            PropertiesTokenType.Value => WriteValue(token.Text),
            PropertiesTokenType.Error => Settings.ThrowOnError ? throw new PropertiesException("Cannot emit error into properties stream!") : false,
            PropertiesTokenType.None => Settings.ThrowOnError ? throw new PropertiesException("Cannot emit null token into properties stream!") : false,
            // Should never happen
            _ => Settings.ThrowOnError ? throw new PropertiesException($"Unknown token type: {token.Type}!") : false,
        };

        /// <summary>
        /// Writes a document comment.
        /// </summary>
        /// <param name="value">The text content of the comment.</param>
        /// <returns>true if the comment could be written; false otherwise.</returns>
        /// <exception cref="PropertiesException">If the comment could not be written and errors are configured to be
        /// thrown.</exception>
        public bool WriteComment(string? value) => WriteComment('#', value);

        /// <summary>
        /// Writes a document comment.
        /// </summary>
        /// <param name="handle">The comment handle. This must be either a '#' or '!'.</param>
        /// <param name="value">The text content of the comment.</param>
        /// <returns>true if the comment could be written; false otherwise.</returns>
        /// <exception cref="PropertiesException">If the comment could not be written and errors are configured to be
        /// thrown.</exception>
        public bool WriteComment(char handle, string? value)
        {
            if (handle != '#' && handle != '!')
                return Settings.ThrowOnError ? throw new PropertiesException("Command handle must be '#' or '!'") : false;

            if (Settings.IgnoreComments)
                return false;

            if (_state != WriterState.CommentOrKey)
                return Settings.ThrowOnError ? throw new PropertiesException($"Expected {StateToString(_state)} got {nameof(ReaderState.Comment)}!") : false;

            int fallbackStartIndex = _textPool.Length - 1;
            StreamMark fallbackMark = _cursor.Position;
            WriteInternal(handle);
            WriteInternal(' ');

            if (!WriteCommentInternal(value, fallbackStartIndex, in fallbackMark))
                return false;

            WriteLineInternal();

            _state = WriterState.CommentOrKey;
            CheckFlush();
            return true;
        }

        private bool WriteCommentInternal(string? value, int fallbackStartIndex, in StreamMark fallbackMark)
        {
            for (int i = 0; i < value?.Length; i++)
            {
                char ch = value[i];
                switch (ch)
                {
                    case '\r':
                        WriteInternal('\\').WriteInternal('r');
                        break;
                    case '\n':
                        WriteInternal('\\').WriteInternal('n');
                        break;
                    case '\t':
                        WriteInternal('\\').WriteInternal('t');
                        break;
                    case '\f':
                        WriteInternal('\\').WriteInternal('f');
                        break;
                    case '\a':
                        WriteInternal('\\').WriteInternal('a');
                        break;
                    case '\v':
                        WriteInternal('\\').WriteInternal('v');
                        break;
                    case '\0':
                        WriteInternal('\\').WriteInternal('0');
                        break;
                    default:
                        WriteCharacter(ch, value, ref i, in fallbackMark, fallbackStartIndex);
                        break;
                }
            }

            if (!(TokenWritten is null))
            {
                // 2 = handle and white-space
                int index = fallbackStartIndex + 1 + 2;
                char[] chars = new char[_textPool.Length - index];

                for (int i = 0; i < chars.Length; i++)
                    chars[i] = _textPool[index + i];

                TokenWritten(this, new PropertiesToken(PropertiesTokenType.Comment, new string(chars)));
            }

            return true;
        }

        /// <summary>
        /// Writes a new key.
        /// </summary>
        /// <param name="key">The value of the key. This cannot be <see langword="null"/>.</param>
        /// <param name="logicalLines">Whether to emit line escapes as logical lines.</param>
        /// <returns>true if the key could be written; false otherwise.</returns>
        public bool WriteKey(string key, bool logicalLines = false)
        {
            if (key is null)
                return Settings.ThrowOnError ? throw new ArgumentNullException(nameof(key)) : false;

            if (_state != WriterState.CommentOrKey)
                return Settings.ThrowOnError ? throw new PropertiesException($"Expected {StateToString(_state)} got Key!") : false;

            if (WriteText(true, key, logicalLines))
            {
                _state = WriterState.ValueOrAssigner;
                CheckFlush();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Writes a new assigner.
        /// </summary>
        /// <param name="assigner">The assigner value.</param>
        /// <returns>true if the assigner could be written; false otherwise.</returns>
        /// <exception cref="PropertiesException">If the assigner could not be written and errors are configured to be
        /// thrown.</exception>
        public bool WriteAssigner(char assigner = '=')
        {
            if (_state != WriterState.ValueOrAssigner)
                return Settings.ThrowOnError ? throw new PropertiesException($"Expected {StateToString(_state)} got Assigner!") : false;

            if (assigner != '=' && assigner != ':' && assigner != ' ' && assigner != '\t' && assigner != '\f')
                return Settings.ThrowOnError ? throw new PropertiesException($"Assigner must be '=', ':' or a white-space!") : false;

            WriteInternal(assigner);
            TokenWritten?.Invoke(this, new PropertiesToken(PropertiesTokenType.Assigner, assigner.ToString()));
            _state = WriterState.Value;
            CheckFlush();
            return true;
        }

        /// <summary>
        /// Writes a new value.
        /// </summary>
        /// <param name="value">The content of the value.</param>
        /// <param name="logicalLines">Whether to emit line escapes as logical lines.</param>
        /// <returns>true if the value could be written; false otherwise.</returns>
        public bool WriteValue(string? value, bool logicalLines = false)
        {
            if (_state != WriterState.ValueOrAssigner && _state != WriterState.Value)
                return Settings.ThrowOnError ? throw new PropertiesException($"Expected {StateToString(_state)} got Value!") : false;

            // Fast path & for empty properties
            if (string.IsNullOrEmpty(value))
            {
                _state = WriterState.CommentOrKey;
                CheckFlush();
                return true;
            }

            if (WriteText(false, value, logicalLines))
            {
                WriteLineInternal();
                _state = WriterState.CommentOrKey;
                CheckFlush();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Writes a new property.
        /// </summary>
        /// <param name="key">The key for the property</param>
        /// <param name="value">The value of the property.</param>
        /// <returns>true if the property could be written; false otherwise.</returns>
        public bool WriteProperty(string key, string value) => WriteProperty(key, '=', value);

        /// <summary>
        /// Writes a new property.
        /// </summary>
        /// <param name="key">The key for the property</param>
        /// <param name="assigner">The assignement character used for this property.</param>
        /// <param name="value">The value of the property.</param>
        /// <returns>true if the property could be written; false otherwise.</returns>
        public bool WriteProperty(string key, char assigner, string value)
        {
            if (!WriteKey(key))
                return false;

            if (!WriteAssigner(assigner))
            {
                WriteValue(value);
                return false;
            }

            return WriteValue(value);
        }

        private bool WriteText(bool key, string text, bool logicalLines)
        {
            bool newLine = key;
            int fallbackStartIndex = _textPool.Length - 1;
            StreamMark fallbackMark = _cursor.Position;
            uint startColumn = fallbackMark.Column;

            if (!key && _state == WriterState.ValueOrAssigner)
            {
                WriteAssigner();
                startColumn++;
            }

            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];

                switch (ch)
                {
                    case '\r':
                    case '\n':
                        if (WriteNewLine(logicalLines, startColumn, text, ref i, ref newLine))
                            continue;
                        break;

                    case '\t':
                    case ' ':
                    case '\f':
                        WriteWhiteSpace(key || newLine || i == 0, ch);
                        break;

                    case '\a':
                        WriteInternal('\\').WriteInternal('a');
                        break;
                    case '\v':
                        WriteInternal('\\').WriteInternal('v');
                        break;
                    case '\0':
                        WriteInternal('\\').WriteInternal('0');
                        break;

                    case '#':
                    case '!':
                        WriteCommentHandle(key && i == 0, ch);
                        break;

                    case '=':
                    case ':':
                        // TODO: Check if should not need on logical - change parser if case
                        if (key)
                            WriteInternal('\\');

                        WriteInternal(ch);
                        break;

                    case '\\':
                        WriteInternal('\\').WriteInternal('\\');
                        break;

                    default:
                        WriteCharacter(ch, text, ref i, in fallbackMark, fallbackStartIndex);
                        break;
                }

                newLine = false;
            }

            if (!(TokenWritten is null))
            {
                int index = fallbackStartIndex + (!key && _state == WriterState.ValueOrAssigner ? 2 : 1);
                char[] chars = new char[_textPool.Length - index];

                for (int i = 0; i < chars.Length; i++)
                    chars[i] = _textPool[index + i];

                TokenWritten(this, new PropertiesToken(key ? PropertiesTokenType.Key : PropertiesTokenType.Value, new string(chars)));
            }
            return true;
        }

        private bool WriteNewLine(bool logicalLines, uint startColumn, string text, ref int index, ref bool newLine)
        {
            if (logicalLines)
            {
                WriteInternal('\\');
                WriteLineInternal();

                while (_cursor.Column < startColumn)
                    WriteInternal(' ');

                if (text[index] == '\r' && index + 1 < text.Length && text[index + 1] == '\n')
                    index++;

                return newLine = true;
            }

            WriteInternal('\\').WriteInternal(text[index] == '\n' ? 'n' : 'r');
            return newLine = false;
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void WriteWhiteSpace(bool firstOnKey, char whitespace)
        {
            if (firstOnKey)
                WriteInternal('\\').WriteInternal(whitespace == ' ' ? whitespace : (whitespace == '\t' ? 't' : 'f'));
            else
                WriteInternal(whitespace);
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void WriteCommentHandle(bool firstOnKey, char handle)
        {
            if (firstOnKey)
                WriteInternal('\\');

            WriteInternal(handle);
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool WriteCharacter(char character, string text, ref int index, in StreamMark fallbackMark, int fallbackStartIndex)
        {
            if (IsLatin1Printable(character) || Settings.AllCharacters)
            {
                WriteInternal(character);
            }
            else if (!WriteEscaped(text, ref index))
            {
                if (_textPool.Length > 0)
                {
                    _textPool.Remove(fallbackStartIndex, index);
                    _cursor.CopyFrom(in fallbackMark);
                }
                return false;
            }

            return true;
        }

        private bool WriteEscaped(string text, ref int index)
        {
            // 8-number escape
            //if (char.IsHighSurrogate(text[index]))
            //{
            //    if (!Settings.AllUnicodeEscapes)
            //    {
            //        int unicode = char.ConvertToUtf32(text[index], text[index + 1]);
            //        return Settings.ThrowOnError ?
            //            throw new PropertiesException($"Cannot create long unicode escape for character \"{char.ConvertFromUtf32(unicode)}\" ({unicode})!")
            //            : false;
            //    }
            //    else if (index + 1 < text.Length && char.IsLowSurrogate(text[index + 1]))
            //    {
            //        WriteInternal('\\').WriteInternal('U');
            //        // TODO: Test perf
            //        WriteInternal(char.ConvertToUtf32(text[index], text[++index]).ToString("X8", CultureInfo.InvariantCulture));
            //    }
            //    else
            //    {
            //        return Settings.ThrowOnError ?
            //            throw new PropertiesException("Missing low surrogate for UTF-16 character!") : false;
            //    }
            //}
            //
            //else
            //{

            // 4-number escape
            WriteInternal('\\').WriteInternal('u');
            // TODO: Test perf
            WriteInternal(((ushort)text[index]).ToString("X4", CultureInfo.InvariantCulture));

            //}

            return true;
        }

        /// <inheritdoc/>
        public void Flush()
        {
            _flushCounter = 0;

            if (_textPool?.Length > 0)
            {
                _stream.Write(_textPool.ToString());
                _textPool.Length = 0;
                _stream.Flush();
            }
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private PropertiesWriter WriteInternal(char value)
        {
            _textPool.Append(value);
            _cursor.AdvanceColumn(value == '\t' ? 4 : 1);
            return this;
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private PropertiesWriter WriteInternal(string? value)
        {
            for (int i = 0; i < value?.Length; i++)
                WriteInternal(value[i]);

            return this;
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void WriteLineInternal()
        {
            _textPool.Append(Environment.NewLine);

            // 2 skips on CRLF
            if (Environment.NewLine.Length > 1)
                _cursor.AdvanceColumn(Environment.NewLine.Length - 1);

            _cursor.AdvanceLine();
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool CheckFlush()
        {
            if ((_flushCounter = Settings.AutoFlush ? _flushCounter + 1 : 0) == Settings.FlushInterval)
            {
                Flush();
                return true;
            }

            return false;
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsLatin1Printable(char ch) => (ch >= '\x20' && ch <= '\x7E') || (ch >= '\xA0' && ch <= '\xFF');

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
                return;

            Flush();

            if (Settings.CloseOnEnd)
                _stream.Dispose();

            _textPool.Length = 0;
            _disposed = true;
        }

        private string StateToString(WriterState state) => state switch
        {
            WriterState.CommentOrKey => "Comment or Key",
            WriterState.ValueOrAssigner => "Value or Assigner",
            _ => state.ToString(),
        };
    }
}