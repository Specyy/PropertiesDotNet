using PropertiesDotNet.Core;
using PropertiesDotNet.Core.Events;
using PropertiesDotNet.Utils;

using System;

namespace PropertiesDotNet.ObjectModel
{
    /// <summary>
    /// Represents a property node inside a ".properties" document.
    /// </summary>
    public sealed class PropertiesProperty : PropertiesNode, IEquatable<PropertiesProperty>
    {
        /// <inheritdoc/>
        public override PropertiesNodeType NodeType => PropertiesNodeType.Property;

        /// <summary>
        /// The key of this property.
        /// </summary>
        public PropertiesKey Key { get; }

        /// <summary>
        /// The assigner of this property.
        /// </summary>
        public PropertiesAssigner? Assigner
        {
            get => _assigner;
            set
            {
                // Must have assigner on non-null value
                // Can only sometimes be omitted
                if (!(_value is null) && value is null)
                    throw new ArgumentNullException(nameof(Assigner));

                _assigner = value;
            }
        }

        private PropertiesAssigner? _assigner;

        /// <summary>
        /// The value of this property.
        /// </summary>
        public PropertiesValue? Value
        {
            get => _value;
            set
            {
                // If empty value and changed to text value, create assigner
                // if not yet created
                if (_assigner is null && !string.IsNullOrEmpty(value.Value))
                    _assigner = ValueAssignerType.Equals;

                _value = value ?? new PropertiesValue((string)null);
            }
        }

        private PropertiesValue? _value;

        /// <summary>
        /// Creates a new <see cref="PropertiesProperty"/>.
        /// </summary>
        /// <param name="reader">The reader to deserialize from.</param>
        public PropertiesProperty(IPropertiesReader reader) : base(reader)
        {
            // Ensure start
            Start = reader.ReadSerialized<PropertyStart>()!.Start;

            Key = new PropertiesKey(reader);

            // Assigner is optional on empty value
			// This may also be useful when reading XML documents
            if (reader.Peek() is ValueAssigner)
                Assigner = new PropertiesAssigner(reader);

            _value = new PropertiesValue(reader);

            // Ensure End
            End = reader.ReadSerialized<PropertyEnd>()!.End;
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesProperty"/>.
        /// </summary>
        /// <param name="key">The key for this property.</param>
        /// <param name="value">The value for this property.</param>
        public PropertiesProperty(PropertiesKey key, PropertiesValue? value) : base(null, null)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            _value = value ?? new PropertiesValue((string)null);
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesProperty"/>.
        /// </summary>
        /// <param name="key">The key for this property.</param>
        /// <param name="assigner">The assigner for this property.</param>
        /// <param name="value">The value for this property.</param>
        public PropertiesProperty(PropertiesKey key, PropertiesAssigner? assigner, PropertiesValue? value) : this(key, value)
        {
            if (!(value is null) && assigner is null)
                throw new ArgumentNullException(nameof(assigner));

            Assigner = assigner;
        }

        /// <inheritdoc/>
        public override bool Equals(PropertiesNode? node) =>
            node is PropertiesProperty prop && Equals(prop);

        /// <inheritdoc/>
        public bool Equals(PropertiesProperty? other)
        {
            return Key.Equals(other?.Key) && Assigner.Equals(other?.Assigner) && Value.Equals(other?.Value);
        }

        /// <inheritdoc/>
        public override IEventStream ToEventStream()
        {
            PropertiesEvent[] events = {
                new PropertyStart(),
                Key,
                Assigner,
                Value,
                new PropertyEnd(),
            };

            return new ReadOnlyEventStream(events);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Key}{Assigner}{Value}";
        }

        /// <inheritdoc/>
        public override int GetHashCode() => HashCodeHelper.GenerateHashCode(Key.GetHashCode(), Assigner?.GetHashCode() ?? ' ', Value.GetHashCode());
    }
}