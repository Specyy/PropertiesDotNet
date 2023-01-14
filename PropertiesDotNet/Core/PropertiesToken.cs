namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Represents a token in a .properties document.
    /// </summary>
    public readonly struct PropertiesToken
    {
        /// <summary>
        /// A "null" value. Returned when an <see cref="IPropertiesReader"/> has not yet
        /// read a token (stream start).
        /// </summary>
        public readonly PropertiesTokenType Type { get; }

        /// <summary>
        /// The textual value of this token.
        /// </summary>
        public readonly string? Value { get; }

        /// <summary>
        /// A property returning whether this token is canonical to a ".properties" document (i.e, is
        /// within the specification).
        /// </summary>
        public readonly bool Canonical => Type == PropertiesTokenType.Key ||
            Type == PropertiesTokenType.Assigner ||
            Type == PropertiesTokenType.Value ||
            Type == PropertiesTokenType.Comment;

        /// <summary>
        /// Creates a new .properties document token.
        /// </summary>
        /// <param name="type">The type of this token.</param>
        /// <param name="value">The textual value of this token.</param>
        public PropertiesToken(PropertiesTokenType type, string? value)
        {
            Type = type;
            Value = value;
        }
    }
}
