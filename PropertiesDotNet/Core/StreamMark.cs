using PropertiesDotNet.Utils;

using System;

#if !NETSTANDARD1_3
using System.Runtime.Serialization;
#endif

namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Represents a position or mark in a stream.
    /// </summary>
#if !NETSTANDARD1_3
    [Serializable]
#endif
    public readonly struct StreamMark : IEquatable<StreamMark>, IComparable<StreamMark>
#if !NETSTANDARD1_3
        , ISerializable
#endif
    {
        /// <summary>
        /// Returns the absolute offset, or character offset in the stream, starting at 0.
        /// </summary>
        public ulong AbsoluteOffset { get; }

        /// <summary>
        /// Returns the line number, starting at 1.
        /// </summary>
        public uint Line { get; }

        /// <summary>
        /// Returns column number, starting at 1.
        /// </summary>
        public uint Column { get; }

        /// <summary>
        /// Returns column number, starting at 0.
        /// </summary>
        public uint XOffset => Column - 1;

        /// <summary>
        /// Returns the line number, starting at 0.
        /// </summary>
        public uint YOffset => Line - 1;

        /// <summary>
        /// Creates a <see cref="StreamMark"/>.
        /// </summary>
        /// <param name="line">The line number.</param>
        /// <param name="column">The column number.</param>
        /// <param name="absoluteOffset">The absolute offset.</param>
        public StreamMark(uint line, uint column, ulong absoluteOffset)
        {
            if (line < 1)
                throw new ArgumentException("Line number must be greater than 0");

            if (column < 1)
                throw new ArgumentException("Column number must be greater than 0");

            Line = line;
            Column = column;
            AbsoluteOffset = absoluteOffset;
        }

#if !NETSTANDARD1_3
        /// <summary>
        /// Deserializes a <see cref="StreamMark"/>.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The context.</param>
        public StreamMark(SerializationInfo info, StreamingContext context)
        {
            Line = info.GetUInt32(nameof(Line));
            Column = info.GetUInt32(nameof(Column));
            AbsoluteOffset = info.GetUInt64(nameof(AbsoluteOffset));
        }
#endif

        /// <summary>
        /// Increments this mark by the specified amount.
        /// </summary>
        /// <param name="lineCount">The amount to increment the <see cref="Line"/> by.</param>
        /// <param name="columnCount">The amount to increment the <see cref="Column"/> by.</param>
        /// <param name="absoluteOffsetCount">The amount to increment the <see cref="AbsoluteOffset"/> by.</param>
        /// <returns>A new <see cref="StreamMark"/>, containing the information.</returns>
        public StreamMark Add(int lineCount = 0, int columnCount = 0, long absoluteOffsetCount = 0)
        {
            var absOffset = AbsoluteOffset;

            if (absoluteOffsetCount < 0)
                absOffset -= (ulong)-absoluteOffsetCount;
            else
                absOffset += (ulong)absoluteOffsetCount;

            return new StreamMark((uint)(Line + lineCount), (uint)(Column + columnCount), absOffset);
        }

        /// <summary>
        /// Respectively returns -1, 0, or 1 if this <see cref="StreamMark"/> is less than,
        /// equal to, or greater than the <paramref name="other"/>.
        /// </summary>
        /// <param name="other">The <see cref="StreamMark"/> to compare.</param>
        /// <returns>-1, 0, or 1 if this <see cref="StreamMark"/> is less than,
        /// equal to, or greater than the <paramref name="other"/>.</returns>
        public int CompareTo(StreamMark other)
        {
            return CompareTo(ref other);
        }

        /// <summary>
        /// Respectively returns -1, 0, or 1 if this <see cref="StreamMark"/> is less than,
        /// equal to, or greater than the <paramref name="other"/>.
        /// </summary>
        /// <param name="other">The <see cref="StreamMark"/> to compare.</param>
        /// <returns>-1, 0, or 1 if this <see cref="StreamMark"/> is less than,
        /// equal to, or greater than the <paramref name="other"/>.</returns>
        public int CompareTo(ref StreamMark other)
        {
            if (Line > other.Line || Column > other.Column)
                return 1;

            if (Line == other.Line && Column == other.Column)
                return 0;

            return -1;
        }

        /// <summary>
        /// Checks if these <see cref="StreamMark"/>s are equal.
        /// </summary>
        /// <param name="other">The <see cref="StreamMark"/> to check.</param>
        /// <returns>true if they are equal; false otherwise.</returns>
        public bool Equals(StreamMark other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Checks if these <see cref="StreamMark"/>s are equal.
        /// </summary>
        /// <param name="other">The <see cref="StreamMark"/> to check.</param>
        /// <returns>true if they are equal; false otherwise.</returns>
        public bool Equals(StreamMark? other)
        {
            return !(other is null) && Line == other.Value.Line && Column == other.Value.Column;
        }

        /// <summary>
        /// Checks if these <see cref="StreamMark"/>s are equal.
        /// </summary>
        /// <param name="other">The <see cref="StreamMark"/> to check.</param>
        /// <returns>true if they are equal; false otherwise.</returns>
        public bool Equals(ref StreamMark other)
        {
            return Line == other.Line && Column == other.Column;
        }

        /// <summary>
        /// Checks if these <see cref="StreamMark"/>s are equal.
        /// </summary>
        /// <param name="mark">The first mark.</param>
        /// <param name="other">The <see cref="StreamMark"/> to check.</param>
        /// <returns>true if they are equal; false otherwise.</returns>
        public static bool operator ==(StreamMark? mark, StreamMark? other)
        {
            if (mark is null && other is null)
                return true;

            if (mark is null || other is null)
                return true;

            return mark.Value.Equals(other!);
        }

        /// <summary>
        /// Checks if these <see cref="StreamMark"/>s are not equal.
        /// </summary>
        /// <param name="mark">The first mark.</param>
        /// <param name="other">The <see cref="StreamMark"/> to check.</param>
        /// <returns>true if they are not equal; false otherwise.</returns>
        public static bool operator !=(StreamMark? mark, StreamMark? other)
        {
            return !(mark == other);
        }

#if !NETSTANDARD1_3
        /// <inheritdoc/>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Line), Line, Line.GetType());
            info.AddValue(nameof(Column), Column, Column.GetType());
            info.AddValue(nameof(AbsoluteOffset), AbsoluteOffset, AbsoluteOffset.GetType());
        }
#endif
        /// <summary>
        /// Returns the current position as a string.
        /// </summary>
        /// <returns>The current position as a string.</returns>
        public override string ToString()
        {
            return $"Line: {Line}, Column: {Column}, AbsoluteOffset: {AbsoluteOffset}";
        }

        /// <summary>
        /// Returns the hash code for this mark.
        /// </summary>
        /// <returns>The hash code for this mark.</returns>
        public override int GetHashCode() => HashCodeHelper.GenerateHashCode(Line, Column);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is StreamMark mark && Equals(mark);
    }
}
