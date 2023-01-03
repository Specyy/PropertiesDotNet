using PropertiesDotNet.Core;
using PropertiesDotNet.Core.Events;

using System;

namespace PropertiesDotNet.ObjectModel
{
    /// <summary>
    /// Represents a key node in a ".properties" document. Keys must be immutable to ensure
    /// their uniqueness.
    /// </summary>
    public sealed class PropertiesKey : PropertiesText, IEquatable<PropertiesKey>
    {
        /// <inheritdoc/>
        public override PropertiesNodeType NodeType => PropertiesNodeType.Key;

        /// <summary>
        /// The value of this <see cref="PropertiesKey"/>
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Deserializes a <see cref="PropertiesKey"/> from the given <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">The reader with the node data.</param>
        public PropertiesKey(IPropertiesReader reader) : base(reader)
        {
            // Ensure key
            Key read = reader.ReadSerialized<Key>()!;

            // Read data
            Value = read.Value ?? throw new PropertiesSerializationException("Encountered null value while loading key!");
            Start = read.Start;
            End = read.End;
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesKey"/>.
        /// </summary>
        /// <param name="value">The key's string value.</param>
        public PropertiesKey(string value) : this(null, null, value)
        {
        }
		
		internal PropertiesKey(StreamMark? start, StreamMark? end, string value) : base(start, end)
		{
			Value = value ?? throw new ArgumentNullException(nameof(value));
		}

        /// <inheritdoc/>
        public override IEventStream ToEventStream() => new ReadOnlyEventStream(this);

        /// <inheritdoc/>
        public bool Equals(PropertiesKey? other) => base.Equals(other);

        /// <inheritdoc/>
        public override string ToString() => Value;

        /// <inheritdoc/>
        public override int GetHashCode() => Value.GetHashCode();

        /// <summary>
        /// Creates a new <see cref="PropertiesKey"/> from the given data.
        /// </summary>
        /// <param name="key">The key.</param>
        public static implicit operator PropertiesKey(Key key) => new PropertiesKey(key.Start, key.End, key.Value);

        /// <summary>
        /// Creates a new <see cref="Key"/> from the given data.
        /// </summary>
        /// <param name="key">The key.</param>
        public static implicit operator Key(PropertiesKey key) => new Key(key.Start, key.End, key.Value);

        /// <summary>
        /// Creates a new <see cref="PropertiesKey"/>.
        /// </summary>
        /// <param name="value">The value for this <see cref="PropertiesKey"/>.</param>
        public static explicit operator PropertiesKey(string value) => new PropertiesKey(value);
    }
}
