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
            _ => Settings.ThrowOnError ? throw new PropertiesException($"Unknown token type: {token.Type}") : false,
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
                return Settings.ThrowOnError ? throw new PropertiesException($"Expected {StateToString(_state)} got {nameof(ParserState.Comment)}!") : false;

            WriteInternal(handle);
            WriteInternal(' ');
            WriteInternal(value);
            WriteLineInternal();

            TokenWritten?.Invoke(this, PropertiesToken.Comment(value));
            _state = WriterState.CommentOrKey;
            CheckFlush();
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
            TokenWritten?.Invoke(this, PropertiesToken.Assigner(assigner));
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

        private bool WriteText(bool key, string text, bool logicalLines)
        {
            bool newLine = key;
            int fallbackStartIndex = _textPool.Length - 1;
            StreamMark fallback = _cursor.Position;

            if (!key && _state == WriterState.ValueOrAssigner)
                // TODO: Maybe emit event for default assigner
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
                            WriteInternal('\\');

                            uint returnColumn = _cursor.Column;

                            WriteLineInternal();

                            while (_cursor.Column < returnColumn)
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
                        if (key && i == 0)
                            WriteInternal('\\');

                        WriteInternal(ch);
                        newLine = false;
                        break;

                    case '=':
                    case ':':
                        // TODO: Check if should not need on logical - change parser if case
                        if (key)
                            WriteInternal('\\');

                        WriteInternal(ch);
                        newLine = false;
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
                        else if (!WriteEscaped(text, ref i))
                        {
                            if (_textPool.Length > 0)
                            {
                                _textPool.Remove(fallbackStartIndex, i);
                                _cursor.CopyFrom(in fallback);
                            }
                            return false;
                        }

                        newLine = false;
                        break;
                }
            }

            int index = fallbackStartIndex + (!key && _state == WriterState.ValueOrAssigner ? 2 : 1);
            char[] chars = new char[_textPool.Length - index];

            for (int i = 0; i < chars.Length; i++)
                chars[i] = _textPool[index + i];

            TokenWritten?.Invoke(this, new PropertiesToken(key ? PropertiesTokenType.Key : PropertiesTokenType.Value, new string(chars)));
            return true;
        }

        private bool WriteEscaped(string key, ref int i)
        {
            // 8-number escape
            if (char.IsHighSurrogate(key[i]))
            {
                if (!Settings.AllUnicodeEscapes)
                {
                    int unicode = char.ConvertToUtf32(key[i], key[i + 1]);
                    return Settings.ThrowOnError ?
                        throw new PropertiesException($"Cannot create long unicode escape for character \"{char.ConvertFromUtf32(unicode)}\" ({unicode})!")
                        : false;
                }
                else if (i + 1 < key.Length && char.IsLowSurrogate(key[i + 1]))
                {
                    WriteInternal('\\');
                    WriteInternal('U');
                    // TODO: Test perf
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
                WriteInternal('\\');
                WriteInternal('u');
                // TODO: Test perf
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

        private void WriteInternal(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            for (int i = 0; i < value.Length; i++)
            {
                WriteInternal(value[i]);
            }
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void WriteLineInternal() => WriteInternal(Environment.NewLine);

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

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsLatin1Printable(char ch) => (ch >= '\x20' && ch <= '\x7E') || (ch >= '\xA0' && ch <= '\xFF');

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

        private string StateToString(WriterState state) => state switch
        {
            WriterState.CommentOrKey => "Comment or Key",
            WriterState.ValueOrAssigner => "Value or Assigner",
            _ => state.ToString(),
        };
    }
}