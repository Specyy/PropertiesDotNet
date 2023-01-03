using System;

#if !NETSTANDARD1_3
using System.Runtime.Serialization;
#endif

namespace PropertiesDotNet.Core.Events
{
    /// <summary>
    /// Represents a textual event in a ".properties" document. This class should only be instantiated to be used
    /// as a work-around for dynamically writing <see cref="Key"/> or <see cref="Events.Value"/> events.
    /// </summary>
    public class Text : PropertiesEvent, IEquatable<Text>, IEquatable<string>
#if !NETSTANDARD1_3
        , ISerializable
#endif
    {
        /// <summary>
        /// Returns the string value of this <see cref="Text"/>.
        /// </summary>
        public virtual string? Value { get; }

        /// <summary>
        /// Returns whether this <see cref="Text"/> should be written with logical lines.
        /// </summary>
        public virtual bool LogicalLines { get; set; }

        /// <inheritdoc/>
        public override bool Canonical => false;

        /// <inheritdoc/>
        public override int DepthIncrease => 0;

#if !NETSTANDARD1_3
        /// <summary>
        /// Creates a new <see cref="Text"/>.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The context.</param>
        public Text(SerializationInfo info, StreamingContext context)
            : base((StreamMark?)info.GetValue(nameof(Start), typeof(StreamMark?)),
                  (StreamMark?)info.GetValue(nameof(End), typeof(StreamMark?)))
        {
            Value = info.GetString(nameof(Value));
        }
#endif

        /// <summary>
        /// Creates a new <see cref="Text"/>.
        /// </summary>
        /// <param name="start">The starting position of this event in the text stream. Null if this
        /// event was created for writing.</param>
        /// <param name="end">The ending position of this event in the text stream. Null if this
        /// event was created for writing.</param>
        /// <param name="value">The string value of this <see cref="Text"/>.</param>
        public Text(StreamMark? start, StreamMark? end, string? value) : base(start, end)
        {
            Value = value;
        }

        /// <summary>
        /// Dynamically writes a <see cref="Text"/> value.
        /// </summary>
        /// <param name="value">The string value of this <see cref="Text"/>.</param>
        public Text(string? value) : this(null, null, value)
        {

        }

        /// <inheritdoc/>
        public override bool Equals(PropertiesEvent? other) => other is Text text && Equals(text);

        /// <inheritdoc/>
        public virtual bool Equals(Text? other) => Equals(other?.Value);

        /// <inheritdoc/>
        public virtual bool Equals(string? other) => Value?.Equals(other) ?? other is null;

#if !NETSTANDARD1_3
        /// <inheritdoc/>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Start), Start, typeof(StreamMark?));
            info.AddValue(nameof(End), End, typeof(StreamMark?));
        }
#endif

        /// <summary>
        /// Returns the <see cref="Value"/> of this <see cref="Text"/>.
        /// </summary>
        /// <returns>The <see cref="Value"/> of this <see cref="Text"/>.</returns>
        public override string ToString() => Value;
    }
}
