using PropertiesDotNet.Core;
using PropertiesDotNet.Core.Events;

using System;

namespace PropertiesDotNet.ObjectModel
{
    /// <summary>
    /// Represents a value node in a ".properties" document.
    /// </summary>
    public sealed class PropertiesValue : PropertiesText, IEquatable<PropertiesValue>
    {
        /// <inheritdoc/>
        public override PropertiesNodeType NodeType => PropertiesNodeType.Value;

        /// <summary>
        /// The value of this <see cref="PropertiesKey"/>
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Deserializes a <see cref="PropertiesValue"/> from the given <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">The reader with the node data.</param>
        public PropertiesValue(IPropertiesReader reader) : base(reader)
        {
            // Ensure value
            Value read = reader.ReadSerialized<Value>()!;

            // Read data
            Value = read.Value;
            Start = read.Start;
            End = read.End;
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesValue"/>.
        /// </summary>
        /// <param name="value">The value's string value.</param>
        public PropertiesValue(string? value) : this(null, null, value)
        {
        }
		
		internal PropertiesValue(StreamMark? start, StreamMark? end, string? value) : base(start, end)
		{
			Value = value;
		}

        /// <inheritdoc/>
        public override bool Equals(PropertiesNode? node) => node is PropertiesValue value && Equals(value);

        /// <inheritdoc/>
        public override IEventStream ToEventStream() => new ReadOnlyEventStream(this);

        /// <inheritdoc/>
        public bool Equals(PropertiesValue? other) => base.Equals(other);

        /// <inheritdoc/>
        public override string ToString() => Value;

        /// <inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        /// <summary>
        /// Creates a new <see cref="PropertiesValue"/> from the given data.
        /// </summary>
        /// <param name="value">The value.</param>
        public static implicit operator PropertiesValue(Value value) => new PropertiesValue(value.Start, value.End, value.Value);

        /// <summary>
        /// Creates a new <see cref="Value"/> from the given data.
        /// </summary>
        /// <param name="value">The value.</param>
        public static implicit operator Value(PropertiesValue value) => new Value(value.Start, value.End, value.Value);

        /// <summary>
        /// Creates a new <see cref="PropertiesValue"/>.
        /// </summary>
        /// <param name="value">The value for this <see cref="PropertiesValue"/>.</param>
        public static explicit operator PropertiesValue(string? value) => new PropertiesValue(value);
    }
}
