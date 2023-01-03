using System;

#if !NETSTANDARD1_3
using System.Runtime.Serialization;
#endif

namespace PropertiesDotNet.Core.Events
{
    /// <summary>
    /// Represents a value assigner in a ".properties" document. On an empty value, there is no assigner.
    /// </summary>
#if !NETSTANDARD1_3
    [Serializable]
#endif
    public class ValueAssigner : PropertiesEvent, IEquatable<ValueAssigner>, IEquatable<ValueAssignerType>, IEquatable<char>
#if !NETSTANDARD1_3
        , ISerializable
#endif
    {
        /// <inheritdoc/>
        public override bool Canonical => true;

        /// <inheritdoc/>
        public override int DepthIncrease => 0;

        /// <summary>
        ///	The type of assigner.
        /// </summary>
        public ValueAssignerType AssignerType { get; }

        /// <summary>
        ///	The assignment character.
        /// </summary>
        public char Value { get; }

        /// <summary>
        /// Writes a <see cref="ValueAssigner"/>.
        /// </summary>
        /// <param name="value">The assignment character.</param>
        public ValueAssigner(string value) : this(null, null,
            value.Length > 1 ? throw new ArgumentException($"Cannot create {nameof(ValueAssigner)} with value '{value}'!") : value[0])
        {
        }

        /// <summary>
        /// Writes a <see cref="ValueAssigner"/>.
        /// </summary>
        /// <param name="value">The assignment type.</param>
        public ValueAssigner(ValueAssignerType value) : this(null, null, value)
        {

        }

        /// <summary>
        /// Creates a new <see cref="ValueAssigner"/>.
        /// </summary>
        /// <param name="start">The starting position of this event in the text stream. Null if this
        /// event was created for writing.</param>
        /// <param name="end">The ending position of this event in the text stream. Null if this
        /// event was created for writing.</param>
        /// <param name="value">The assignment type.</param>
        public ValueAssigner(StreamMark? start, StreamMark? end, ValueAssignerType value) : base(start, end)
        {
            AssignerType = value;
            Value = value switch
            {
                ValueAssignerType.Equals => '=',
                ValueAssignerType.Colon => ':',
                ValueAssignerType.Whitespace => ' ',
                _ => throw new ArgumentException($"Unknown assigner type \"{value}\"!"),
            };
        }

        /// <summary>
        /// Writes a <see cref="ValueAssigner"/>.
        /// </summary>
        /// <param name="value">The assignment character.</param>
        public ValueAssigner(char value) : this(null, null, value)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ValueAssigner"/>.
        /// </summary>
        /// <param name="start">The starting position of this event in the text stream. Null if this
        /// event was created for writing.</param>
        /// <param name="end">The ending position of this event in the text stream. Null if this
        /// event was created for writing.</param>
        /// <param name="value">The assignment character.</param>
        public ValueAssigner(StreamMark? start, StreamMark? end, char value) : base(start, end)
        {
            Value = value;
            AssignerType = value switch
            {
                '=' => ValueAssignerType.Equals,
                ':' => ValueAssignerType.Colon,
                ' ' => ValueAssignerType.Whitespace,
                '\t' => ValueAssignerType.Whitespace,
                '\f' => ValueAssignerType.Whitespace,
                _ => throw new ArgumentException($"Cannot create {nameof(ValueAssigner)} with value '{value}'!"),
            };
        }

        /// <summary>
		/// Accepts an <see cref="IEventVisitor"/>.
		/// </summary>
		/// <param name="visitor">The visitor to accept.</param>
		public override void AcceptVisitor(IEventVisitor visitor) => visitor.Visit(this);

        /// <inheritdoc/>
        public override bool Equals(PropertiesEvent? other)
        {
            return other is ValueAssigner val && Equals(val);
        }

        /// <inheritdoc/>
        public bool Equals(ValueAssignerType other)
        {
            return AssignerType == other;
        }

        /// <inheritdoc/>
        public bool Equals(ValueAssigner? other)
        {
            return !(other is null) && Equals(other!.Value);
        }

        /// <inheritdoc/>
        public bool Equals(char other)
        {
            return Value.Equals(other);
        }

        /// <summary>
        /// Returns this <see cref="ValueAssigner"/>'s <see cref="Value"/>.
        /// </summary>
        /// <returns>This <see cref="ValueAssigner"/>'s <see cref="Value"/>.</returns>
        public override string ToString()
        {
            return Value.ToString();
        }

        /// <summary>
        /// Writes a <see cref="ValueAssigner"/>.
        /// </summary>
        /// <param name="value">The value for this <see cref="ValueAssigner"/>.</param>
        public static implicit operator ValueAssigner(char value)
        {
            return new ValueAssigner(value);
        }

        /// <summary>
        /// Writes a <see cref="ValueAssigner"/>.
        /// </summary>
        /// <param name="type">The type for this <see cref="ValueAssigner"/>.</param>
        public static implicit operator ValueAssigner(ValueAssignerType type)
        {
            return new ValueAssigner(type);
        }

#if !NETSTANDARD1_3
        /// <inheritdoc/>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Value), Value, typeof(string));
        }
#endif
    }
}
