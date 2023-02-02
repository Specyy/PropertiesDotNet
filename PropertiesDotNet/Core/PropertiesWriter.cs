using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

using PropertiesDotNet.Utils;

namespace PropertiesDotNet.Core
{
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
        public event EventWritten? TokenWritten;

        private DocumentState _state;
        private PropertiesWriterSettings _settings;
        private bool _disposed;
        private uint _flushCounter;

        private StringBuilder _textPool;
        private StreamCursor _cursor;
        private TextWriter _stream;

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
        public bool Write(PropertiesToken token) => WriteToken(token);

        private bool WriteToken(PropertiesToken token)
        {
            switch (token.Type)
            {
                case PropertiesTokenType.Comment:
                    return WriteComment(token.Value);

                case PropertiesTokenType.Key:
                    return WriteKey(token.Value);

                case PropertiesTokenType.Assigner:
                    return token.Value.Length > 1 ?
                        (Settings.ThrowOnError ? throw new PropertiesException("Assigner must be '=', ':' or any type of white-space!") : false)
                        : WriteAssigner(token.Value[0]);

                case PropertiesTokenType.Value:
                    return WriteValue(token.Value);

                case PropertiesTokenType.Error:
                    return Settings.ThrowOnError ? throw new PropertiesException("Cannot emit error into properties stream!") : false;

                case PropertiesTokenType.None:
                    return Settings.ThrowOnError ? throw new PropertiesException("Cannot emit null token into properties stream!") : false;

                default:
                    return Settings.ThrowOnError ? throw new PropertiesException($"Unknown token type: {token.Type}") : false;
            }
        }

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

            if (_state != DocumentState.Start && _state != DocumentState.Comment)
                return Settings.ThrowOnError ? throw new PropertiesException($"Expected {_state} got {nameof(DocumentState.Comment)}!") : false;

            WriteInternal(handle);
            WriteInternal(' ');
            WriteInternal(value);
            WriteLineInternal();
            TokenWritten?.Invoke(this, new PropertiesToken(PropertiesTokenType.Comment, value));
            _state = DocumentState.Start;
            CheckFlush();
            return true;
        }

        /// <summary>
        /// Writes a new key.
        /// </summary>
        /// <param name="key">The value of the key. This cannot be null.</param>
        /// <param name="logicalLines">Whether to emit line escapes as logical liens.</param>
        /// <returns>true if the key could be written; false otherwise.</returns>
        public bool WriteKey(string key, bool logicalLines = false)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            if (_state != DocumentState.Start && _state != DocumentState.Key)
                return Settings.ThrowOnError ? throw new PropertiesException($"Expected {_state} got {nameof(DocumentState.Key)}!") : false;

            if (WriteText(true, key, logicalLines))
            {
                _state = DocumentState.Assigner;
                CheckFlush();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Writes a new assigner.
        /// </summary>
        /// <param name="value">The assigner value.</param>
        /// <returns>true if the assigner could be written; false otherwise.</returns>
        /// <exception cref="PropertiesException">If the assigner could not be written and errors are configured to be
        /// thrown.</exception>
        public bool WriteAssigner(char value = '=')
        {
            if (_state != DocumentState.Value && _state != DocumentState.Assigner)
                return Settings.ThrowOnError ? throw new PropertiesException($"Expected {_state} got {nameof(DocumentState.Assigner)}!") : false;

            if (value != '=' && value != ':' && value != ' ' && value != '\t' && value != '\f')
                return Settings.ThrowOnError ? throw new PropertiesException($"Assigner must be '=', ':' or a white-space!") : false;

            WriteInternal(value);
            _state = DocumentState.Value;
            CheckFlush();
            return true;
        }

        /// <summary>
        /// Writes a new value.
        /// </summary>
        /// <param name="value">The content of the value.</param>
        /// <param name="logicalLines">Whether to emit line escapes as logical liens.</param>
        /// <returns>true if the value could be written; false otherwise.</returns>
        public bool WriteValue(string? value, bool logicalLines = false)
        {
            if (_state != DocumentState.Value && _state != DocumentState.Assigner)
                return Settings.ThrowOnError ? throw new PropertiesException($"Expected {_state} got {nameof(DocumentState.Value)}!") : false;

            if (string.IsNullOrEmpty(value))
                return true;

            if (WriteText(false, value, logicalLines))
            {
                WriteLineInternal();
                _state = DocumentState.Start;
                CheckFlush();
                return true;
            }

            return false;
        }

        private bool WriteText(bool key, string text, bool logicalLines)
        {
            bool newLine = key;
            int fallbackStartIndex = _textPool.Length - 1;
            StreamMark fallback = _cursor.CurrentPosition;

            if (!key && _state == DocumentState.Assigner)
                WriteInternal('=');

            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];

                switch (ch)
                {
                    case '\r':
                    case '\n':
                        if (logicalLines)
                        {
                            // TODO: Write logical line
                            uint column = _cursor.Column;

                            WriteInternal('\\');
                            WriteLineInternal();

                            // TODO: Check if condition works
                            while (_cursor.Column < column)
                                WriteInternal(' ');

                            if (ch == '\r' && i + 1 < text.Length && text[i] == '\n')
                                i++;

                            newLine = true;
                        }
                        else
                        {
                            WriteInternal('\\');
                            WriteInternal(ch == '\n' ? 'n' : 'r');
                            newLine = false;
                        }
                        break;

                    case '\t':
                    case ' ':
                    case '\f':
                        if (key || newLine || i == 0)
                        {
                            WriteInternal('\\');
                            WriteInternal(ch == ' ' ? ch : (ch == '\t' ? 't' : 'f'));
                        }
                        else WriteInternal(ch);

                        newLine = false;
                        break;

                    case '#':
                    case '!':
                        if (newLine)
                        {
                            WriteInternal('\\');
                        }

                        WriteInternal(ch);
                        newLine = false;
                        break;

                    case '=':
                    case ':':
                        if (key)
                            WriteInternal('\\');

                        WriteInternal(ch);
                        break;

                    case '\\':
                        WriteInternal('\\');
                        WriteInternal('\\');
                        newLine = false;
                        break;

                    default:
                        if (IsLatin1Printable(ch) || Settings.AllCharacters)
                        {
                            WriteInternal(ch);
                        }
                        else if (!WriteEscaped(text, ref i) && _textPool.Length > 0)
                        {
                            _textPool.Remove(fallbackStartIndex, i);
                            _cursor.CopyFrom(in fallback);
                            return false;
                        }

                        newLine = false;
                        break;
                }
            }

            int index = fallbackStartIndex + (!key && _state == DocumentState.Assigner ? 2 : 1);
            char[] chars = new char[_textPool.Length - index];

            for (int i = 0; i < chars.Length; i++)
                chars[i] = _textPool[index + i];

            TokenWritten?.Invoke(this, new PropertiesToken(key ? PropertiesTokenType.Key : PropertiesTokenType.Value, new string(chars)));
            return true;
        }

        private bool WriteEscaped(string key, ref int i)
        {
            WriteInternal('\\');

            // 8-number escape
            if (char.IsHighSurrogate(key[i]))
            {
                // TODO: check
                if (!Settings.AllUnicodeEscapes)
                {
                    return Settings.ThrowOnError ?
                        throw new PropertiesException($"Cannot create long unicode escape for character '{char.ConvertToUtf32(key[i], key[i + 1])}'!")
                        : false;
                }

                if (i + 1 < key.Length && char.IsLowSurrogate(key[i + 1]))
                {
                    WriteInternal('U');
                    WriteInternal(char.ConvertToUtf32(key[i], key[++i]).ToString("X8", CultureInfo.InvariantCulture));
                }
                else
                {
                    return Settings.ThrowOnError ?
                        throw new PropertiesException("Missing low surrogate for UTF-16 character!") : false;
                }
            }
            // 4-number escape
            else
            {
                // TODO: Test perf
                WriteInternal('u');
                WriteInternal(((ushort)key[i]).ToString("X4", CultureInfo.InvariantCulture));
            }

            return true;
        }

        /// <inheritdoc/>
        public void Flush()
        {
            if (_textPool?.Length > 0)
            {
                _stream.Write(_textPool.ToString());
                _textPool.Length = 0;
                _stream.Flush();
            }
        }

        private void WriteInternal(char value)
        {
            _textPool.Append(value);

            if (value == '\r' || value == '\n')
            {
                if (value == '\n' && _textPool.Length > 0 && _textPool[_textPool.Length - 1] == '\r')
                {
                    _cursor.Line--;
                    _cursor.AbsoluteOffset--;
                }

                _cursor.AdvanceLine();
            }
            else _cursor.AdvanceColumn(value == '\t' ? 4 : 1);
        }

        private void WriteInternal(char[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                WriteInternal(value[i]);
            }
        }

        private void WriteInternal(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            for (int i = 0; i < value.Length; i++)
            {
                WriteInternal(value[i]);
            }
        }

        private void WriteLineInternal(char value)
        {
            WriteInternal(value);
            WriteInternal(Environment.NewLine);
        }

        private void WriteLineInternal(string? value)
        {
            WriteInternal(value);
            WriteInternal(Environment.NewLine);
        }

        private void WriteLineInternal()
        {
            WriteInternal(Environment.NewLine);
        }

        private bool CheckFlush()
        {
            if ((_flushCounter = Settings.AutoFlush ? _flushCounter + 1 : 0) == Settings.FlushInterval)
            {
                Flush();
                _flushCounter = 0;
                return true;
            }

            return false;
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Flush();
                _stream.Dispose();
            }

            if (_textPool.Length > 0)
                _textPool.Length = 0;

            _textPool = null!;
            _cursor = null!;
            _stream = null!;
            _disposed = true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
                return;

            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private bool IsLatin1Printable(char ch) => (ch >= '\x20' && ch <= '\x7E') || (ch >= '\xA0' && ch <= '\xFF');
    }
}