using System;
using System.Globalization;
using System.IO;
using System.Text;

using PropertiesDotNet.Core.Events;

namespace PropertiesDotNet.Core
{
	/// <summary>
	/// Represents a class that writes <see cref="PropertiesEvent"/>s into a stream as text.
	/// </summary>
	public sealed class PropertiesWriter : IPropertiesWriter
	{
		/// <inheritdoc/>
		public PropertiesWriterSettings Settings { get; }

		/// <inheritdoc/>
		public event EventWritten? EventWritten;

		private PropertiesEvent? _lastEvent;

		private bool _propertyStarted;

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
		/// Creates a new <see cref="PropertiesWriter"/>.
		/// </summary>
		/// <param name="output">The output stream.</param>
		/// <param name="settings">The settings for this writer.</param>
		public PropertiesWriter(TextWriter output, PropertiesWriterSettings? settings = null)
		{
			Settings = settings ?? PropertiesWriterSettings.Default;
			_stream = output ?? throw new ArgumentNullException(nameof(output));
			_textPool = new StringBuilder();
			_cursor = new StreamCursor();
		}

		/// <inheritdoc/>
		public void Write(PropertiesEvent @event)
		{
			WriteEvent(@event);
			EventWritten?.Invoke(this, @event);
		}

		private void WriteEvent(PropertiesEvent @event)
		{
			if (_propertyStarted)
			{
				// Expect key/text
				if (_lastEvent is PropertyStart)
				{
					if (!(@event is Value) && @event is Text text)
					{
						WriteText(text, true);
					}
					else
					{
						throw new PropertiesStreamException(_cursor.CurrentPosition, _cursor.CurrentPosition, $"Expected {nameof(Key)} after {nameof(PropertyStart)}, got {@event?.GetType().FullName ?? "null"}");
					}
				}

				// Check if last event is key
				// Expect assigner/(value/text)/property end
				else if (!(_lastEvent is Value) && _lastEvent is Text)
				{
					if (@event is ValueAssigner assigner)
					{
						WriteAssigner(assigner);
					}
					else if (!(@event is Key) && @event is Text text)
					{
						// Assigner
						Write(' ');
						WriteText(text, false);
					}
					else if (@event is PropertyEnd)
					{
						_propertyStarted = false;
					}
					else
					{
						throw new PropertiesStreamException(_cursor.CurrentPosition, _cursor.CurrentPosition, $"Expected {nameof(ValueAssigner)}, {nameof(Value)}, or {nameof(PropertyEnd)} after {nameof(Key)}, got {@event?.GetType().FullName ?? "null"}");
					}
				}

				// Expect (value/text) or property end
				else if (_lastEvent is ValueAssigner)
				{
					if (!(@event is Key) && @event is Text text)
					{
						WriteText(text, false);
					}
					else if (@event is PropertyEnd)
					{
						_propertyStarted = false;
					}
					else
					{
						throw new PropertiesStreamException(_cursor.CurrentPosition, _cursor.CurrentPosition, $"Expected {nameof(Value)} or {nameof(PropertyEnd)} after {nameof(ValueAssigner)}, got \"{@event?.GetType().FullName ?? "null"}\"");
					}
				}

				// Check if last event is value
				else if (!(_lastEvent is Key) && _lastEvent is Text)
				{
					if (@event is PropertyEnd)
						_propertyStarted = false;
					else
						throw new PropertiesStreamException(_cursor.CurrentPosition, _cursor.CurrentPosition, $"Expected {nameof(PropertyEnd)} after {nameof(Value)}, got \"{@event?.GetType().FullName ?? "null"}\"");
				}

				else
				{
					// We should never be here
					throw new InvalidOperationException($"Unknown event \"{@event?.GetType().FullName ?? "null"}\"");
				}
			}
			else
			{
				// Check for document end
				if (_lastEvent is DocumentEnd)
				{
					throw new PropertiesStreamException(_cursor.CurrentPosition, _cursor.CurrentPosition, $"No event can proceed {nameof(DocumentEnd)}!");
				}

				// Expect document start as first

				if (_lastEvent is null)
				{
					if (!(@event is DocumentStart))
						throw new PropertiesStreamException(_cursor.CurrentPosition, _cursor.CurrentPosition, $"Expected {nameof(DocumentStart)}, got {@event?.GetType().FullName ?? "null"}");
				}

				else if (@event is Comment comment)
				{
					if (Settings.IgnoreComments)
						return;

					WriteComment(comment);
				}

				else if (@event is PropertyStart)
				{
					WriteLine();
					_propertyStarted = true;
				}

				else if (@event is DocumentEnd)
				{
					if (Settings.CloseStreamOnEnd)
						_stream.Dispose();
				}
				else
				{
					throw new PropertiesStreamException(_cursor.CurrentPosition, _cursor.CurrentPosition, $"Expected {nameof(PropertyStart)}, {nameof(Comment)} or {nameof(DocumentEnd)}, got {@event?.GetType().FullName ?? "null"}");
				}
			}

			_lastEvent = @event;
		}

		/// <summary>
		/// Flushes the buffered events into the underlying stream.
		/// </summary>
		public void Flush()
		{
			_stream.Flush();
		}

		private void WriteAssigner(ValueAssigner assigner)
		{
			Write(assigner.Value);
		}

		private void WriteText(Text value, bool isKey)
		{
			_textPool.Length = 0;

			string stringValue;

			if ((stringValue = value.Value) is null)
			{
				stringValue = isKey ?
					throw new ArgumentNullException(nameof(value.Value), "Dynamic key value cannot be null!") :
					string.Empty;
			}

			var newLine = _cursor.Column == 1;

			for (var i = 0; i < value.Value.Length; i++)
			{
				var current = stringValue[i];

				switch (current)
				{
					case '#':
					case '!':
					{
						if (newLine)
							Append('\\');

						Append(current);
						break;
					}

					case '\r':
					case '\n':
					{
						WriteNewLine(value, stringValue, current, ref i);
						newLine = true;
						continue;
					}

					case ' ':
					case '\t':
					case '\f':
					{
						WriteWhiteSpace(isKey, newLine, current);
						break;
					}

					case '=':
					case ':':
					{
						if (isKey)
							AppendEscaped(current);
						else
							Append(current);

						break;
					}

					case '\\':
					{
						AppendEscaped(current);
						break;
					}

					default:
					{
						WriteTextCharacter(stringValue, current, ref i);
						break;
					}
				}

				if (newLine)
					newLine = false;
			}

			// Manually write
			_stream.Write(_textPool.ToString());
		}

		private void WriteNewLine(Text value, string stringValue, char current, ref int currentIndex)
		{
			// Write logical line
			if (value.LogicalLines)
			{
				if (current == '\r' && currentIndex < stringValue.Length - 1 && stringValue[currentIndex + 1] == '\n')
					currentIndex++;

				AppendEscaped(_stream.NewLine);

				// Start writing one character before the '\\'
				var returnColumn = _cursor.Column - (uint)_stream.NewLine.Length; // TODO: ^ (-1)
				_cursor.AdvanceLine();

				while(_cursor.Column < returnColumn)
					Append(' ');
			}

			// Escape new line
			else
			{
				AppendEscaped(current == '\r' ? 'r' : 'n');
			}
		}

		private void WriteWhiteSpace(bool isKey, bool newLine, char current)
		{
			if (isKey || newLine || _textPool.Length == 0)
			{
				Append('\\');
			}

			if (Settings.AllCharacters)
			{
				Append(current);
			}
			else
			{
				switch(current)
				{
					case '\t':
						AppendEscaped('t');
						break;

					case '\f':
						AppendEscaped('f');
						break;

					default:
						AppendEscaped(' ');
						break;
				}
			}
		}

		private void WriteTextCharacter(string stringValue, char current, ref int currentIndex)
		{
			// Normal character
			if (Settings.AllCharacters || IsLatin1Printable(current))
			{
				Append(current);
			}

			// Non-ISO-8859-1 character
			else
			{
				// 8-number escape
				if (char.IsHighSurrogate(current))
				{
					if (!Settings.AllUnicodeEscapes)
						throw new PropertiesStreamException(null, null, $"Cannot create long unicode escape for character ({char.ConvertToUtf32(current, stringValue[currentIndex + 1])})");
					
					if (currentIndex < stringValue.Length - 1 && char.IsLowSurrogate(stringValue[currentIndex + 1]))
						AppendEscaped('U').Append(char.ConvertToUtf32(current, stringValue[++currentIndex]).ToString("X8", CultureInfo.InvariantCulture));
					else
						throw new PropertiesStreamException(null, null, "Missing low surrogate for UTF-16 character!");
				}

				// 4-number escape
				else
				{
					AppendEscaped('u').Append(((ushort)current).ToString("X4", CultureInfo.InvariantCulture));
				}
			}
		}

		private PropertiesWriter Append(char toAppend)
		{
			_textPool.Append(toAppend);
			_cursor.AdvanceColumn();
			return this;
		}

		private PropertiesWriter Append(string toAppend)
		{
			_textPool.Append(toAppend);
			_cursor.AdvanceColumn(toAppend.Length);
			return this;
		}

		private PropertiesWriter AppendEscaped(char toAppend)
		{
			Append('\\');
			Append(toAppend);
			return this;
		}

		private PropertiesWriter AppendEscaped(string toAppend)
		{
			Append('\\');
			Append(toAppend);
			return this;
		}

		private void WriteComment(Comment comment)
		{
			if (!(_lastEvent is DocumentStart))
				WriteLine();

			Write(comment.HandleCharacter);
			Write(' ');
			Write(comment.Value);
		}

		private void Write(char ch)
		{
			_stream.Write(ch);
			_cursor.AdvanceColumn();
		}

		private void Write(string chars)
		{
			_stream.Write(chars);
			_cursor.AdvanceColumn(chars.Length);
		}

		private void WriteLine()
		{
			_stream.WriteLine();
			_cursor.AdvanceLine();
			// Method above advances 1 for us
			_cursor.AbsoluteOffset += (uint)(_stream.NewLine.Length == 0 ? 0 : _stream.NewLine.Length - 1);
		}

		private bool IsLatin1Printable(char item)
		{
			return (item >= '\x20' && item <= '\x7E') ||
				(item >= '\xA0' && item <= '\xFF');
		}
	}
}