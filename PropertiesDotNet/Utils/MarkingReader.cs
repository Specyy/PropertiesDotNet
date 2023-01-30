using System;
using System.IO;

using PropertiesDotNet.Core;

namespace PropertiesDotNet.Utils
{
    internal class MarkingReader : IDisposable
    {
        private TextReader _stream;
        private StreamCursor _cursor;
        private bool _disposed;

        internal bool EndOfStream => _disposed || _stream.Peek() == -1;
        internal StreamMark Position => _cursor.CurrentPosition;
        internal StreamCursor Cursor => _cursor;

        internal MarkingReader(TextReader stream)
        {
            _stream = stream;
            _cursor = new StreamCursor();
        }

        internal int Peek() => _disposed ? -1 : _stream.Peek();

        internal int Read()
        {
            int read = -1;

            if (!_disposed && (read = _stream.Read()) != -1)
            {
                if (read == '\t')
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

        internal void ReadLineEnd()
        {
            int read;

            if (!_disposed && (read = _stream.Read()) != -1)
            {
                // 2 skips on CRLF
                if (read == '\r' && _stream.Peek() == '\n')
                {
                    _stream.Read();
                    _cursor.AdvanceColumn(1);
                }

                _cursor.AdvanceLine();
            }
        }

        internal virtual void Dispose(bool disposing)
        {
            if (disposing)
                _stream.Dispose();

            _cursor = null!;
            _stream = null!;
            _disposed = true;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}