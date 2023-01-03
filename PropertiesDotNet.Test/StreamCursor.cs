using System;
using System.Runtime.CompilerServices;

#if !NETSTANDARD1_3
using System.Runtime.Serialization;
#endif

namespace PropertiesDotNet.Test
{
    /// <summary>
    /// A cursor for marking a reading or writing position.
    /// </summary>
#if !NETSTANDARD1_3
    [Serializable]
#endif

    internal sealed class StreamCursor
#if !NETSTANDARD1_3
        : ISerializable
#endif
    {
        /// <summary>
        /// The current index or offset.
        /// </summary>
        public ulong AbsoluteOffset { get; set; }

        /// <summary>
        /// The current line.
        /// </summary>
        public uint Line { get; set; } = 1;

        /// <summary>
        /// The current column.
        /// </summary>
        public uint Column { get; set; } = 1;

        /// <summary>
        /// The current position of the cursor, as a <see cref="StreamMark"/>.
        /// </summary>
        public StreamMark CurrentPosition { get => new StreamMark(Line, Column, AbsoluteOffset); }

        /// <summary>
        /// Creates a <see cref="StreamCursor"/>.
        /// </summary>
        public StreamCursor()
        {
        }

#if !NETSTANDARD1_3
        /// <summary>
        /// Deserializes a <see cref="StreamCursor"/>.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The context.</param>
        public StreamCursor(SerializationInfo info, StreamingContext context)
        {
            Line = info.GetUInt32(nameof(StreamMark.Line));
            Column = info.GetUInt32(nameof(StreamMark.Column));
            AbsoluteOffset = info.GetUInt64(nameof(StreamMark.AbsoluteOffset));
        }
#endif

        /// <summary>
        /// Advances by the specified amount of characters.
        /// </summary>
        /// <param name="amount">The amount.</param>
        public void AdvanceColumn(int amount = 1)
        {
            if (Line == 0)
                Line = 1;

            if (amount < 0)
            {
                AbsoluteOffset -= (ulong)-amount;
                Column -= (uint)-amount;
            }
            else
            {
                AbsoluteOffset += (ulong)amount;
                Column += (uint)amount;
            }
        }

        /// <summary>
        /// Advances to a new line.
        /// </summary>
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void AdvanceLine()
        {
            AbsoluteOffset++;
            Line++;
            Column = 1;
        }

        /// <summary>
        /// Resets this cursor.
        /// </summary>
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Reset()
        {
            Line = 1;
            AbsoluteOffset = 0;
            Column = 1;
        }

#if !NETSTANDARD1_3
        /// <inheritdoc/>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(StreamMark.Line), Line, Line.GetType());
            info.AddValue(nameof(StreamMark.Column), Column, Column.GetType());
            info.AddValue(nameof(StreamMark.AbsoluteOffset), AbsoluteOffset, AbsoluteOffset.GetType());
        }
#endif

        /// <summary>
        /// Returns the current position as a string.
        /// </summary>
        /// <returns>The current position as a string.</returns>
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override string ToString() => CurrentPosition.ToString();
    }
}
