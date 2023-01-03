using PropertiesDotNet.Core;
using PropertiesDotNet.Core.Events;

using System;

namespace PropertiesDotNet.ObjectModel
{
    /// <summary>
    /// Represents a value assigner node in a ".properties" document.
    /// </summary>
    public sealed class PropertiesAssigner : PropertiesNode, IEquatable<char>, IEquatable<string>, IEquatable<PropertiesAssigner>
    {
        /// <summary>
        ///	The type of assigner.
        /// </summary>
        public ValueAssignerType AssignerType 
        { 
            get => _assignerType;
            set 
            {
                // Change value if value is changed
                _value = value switch
                {
                    ValueAssignerType.Equals => '=',
                    ValueAssignerType.Colon => ':',
                    ValueAssignerType.Whitespace => ' ',
                    _ => throw new ArgumentException($"Unknown assigner type \"{value}\"!")
                };

                _assignerType = value;
            }
        }
        
        private ValueAssignerType _assignerType;

        /// <summary>
        ///	The assignment character.
        /// </summary>
        public char Value 
        { 
            get => _value; 
            set
            {
                // Change assigner if value is changed
                _assignerType = value switch
                {
                    '=' => ValueAssignerType.Equals,
                    ':' => ValueAssignerType.Colon,
                    ' ' => ValueAssignerType.Whitespace,
                    '\t' => ValueAssignerType.Whitespace,
                    '\f' => ValueAssignerType.Whitespace,
                    _ => throw new ArgumentException($"Cannot create {nameof(ValueAssigner)} with value '{value}'!"),
                };

                _value = value;
            }
        }
        
        private char _value;

        /// <inheritdoc/>
        public override PropertiesNodeType NodeType => PropertiesNodeType.Assigner;

        /// <summary>
        /// Deserializes a <see cref="PropertiesAssigner"/> from the given <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">The reader with the node data.</param>
        public PropertiesAssigner(IPropertiesReader reader) : base(reader)
        {
            // Ensure assigner
            ValueAssigner read = reader.ReadSerialized<ValueAssigner>()!;

            // Read data
            _value = read.Value == default ? throw new PropertiesSerializationException("Encountered null value while loading value assigner!") : read.Value;
            _assignerType = read.AssignerType;
            Start = read.Start;
            End = read.End;
        }
        
        /// <summary>
        /// Creates a new <see cref="PropertiesAssigner"/>.
        /// </summary>
        /// <param name="value">The assignment type.</param>
        public PropertiesAssigner(ValueAssignerType value) : base(null, null)
        {
            AssignerType = value;
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesAssigner"/>.
        /// </summary>
        /// <param name="value">The assignment character.</param>
        public PropertiesAssigner(string value) : this(value.Length > 1 ? throw new ArgumentException($"Cannot create {nameof(PropertiesAssigner)} with value '{value}'!") : value[0])
        {
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesAssigner"/>.
        /// </summary>
        /// <param name="value">The assignment character.</param>
        public PropertiesAssigner(char value) : this(null, null, value)
        {
        }
        
        internal PropertiesAssigner(StreamMark? start, StreamMark? end, char value) : base(start, end)
        {
            Value = value;
        }

        /// <inheritdoc/>
        public override IEventStream ToEventStream() => new ReadOnlyEventStream((ValueAssigner)this);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ValueAssignerType other) => AssignerType == other;

        /// <inheritdoc/>
        public bool Equals(char other) => Value.Equals(other);

        /// <inheritdoc/>
        public bool Equals(string? other)
        {
            if (other is null)
                return Value == default;

            return other.Length == 1 && Equals(other[0]);
        }

        /// <summary>
        /// Returns this <see cref="PropertiesAssigner"/>'s <see cref="Value"/>.
        /// </summary>
        /// <returns>This <see cref="PropertiesAssigner"/>'s <see cref="Value"/>.</returns>
        public override string ToString() => Value.ToString();

        /// <inheritdoc/>
        public override bool Equals(PropertiesNode? node)
        {
            return node is PropertiesAssigner assigner && Equals(assigner);
        }

        /// <inheritdoc/>
        public bool Equals(PropertiesAssigner? other) => Equals(other?.Value ?? default);

        /// <inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        /// <summary>
        /// Creates a new <see cref="PropertiesAssigner"/> from the given data.
        /// </summary>
        /// <param name="assigner">The assigner.</param>
        public static implicit operator PropertiesAssigner(ValueAssigner assigner) => new PropertiesAssigner(assigner.Start, assigner.End, assigner.Value);

        /// <summary>
        /// Creates a new <see cref="ValueAssigner"/> from the given data.
        /// </summary>
        /// <param name="assigner">The assigner.</param>
        public static implicit operator ValueAssigner(PropertiesAssigner assigner) => new ValueAssigner(assigner.Start, assigner.End, assigner.Value);

        /// <summary>
        /// Creates a new <see cref="PropertiesAssigner"/>.
        /// </summary>
        /// <param name="value">The value for this <see cref="PropertiesAssigner"/>.</param>
        public static implicit operator PropertiesAssigner(char value) => new PropertiesAssigner(value);

        /// <summary>
        /// Creates a new <see cref="PropertiesAssigner"/>.
        /// </summary>
        /// <param name="type">The type for this <see cref="PropertiesAssigner"/>.</param>
        public static implicit operator PropertiesAssigner(ValueAssignerType type) => new PropertiesAssigner(type);
    }
}
