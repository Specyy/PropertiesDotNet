using System;

using PropertiesDotNet.Utils;

namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Represents a token in a ".properties" document.
    /// </summary>
    public readonly struct PropertiesToken : IEquatable<PropertiesToken>
    {
        /// <summary>
        /// The type of this token.
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
        /// Creates a new ".properties" document token.
        /// </summary>
        /// <param name="type">The type of this token.</param>
        /// <param name="value">The textual value of this token.</param>
        public PropertiesToken(PropertiesTokenType type, string? value)
        {
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Checks if these <see cref="PropertiesToken"/>s are equal.
        /// </summary>
        /// <param name="other">The <see cref="PropertiesToken"/> to check.</param>
        /// <returns>true if they are equal; false otherwise.</returns>
        public bool Equals(PropertiesToken other)
        {
            return Type == other.Type && Value == other.Value;
        }

        /// <summary>
        /// Checks if these <see cref="PropertiesToken"/>s are equal.
        /// </summary>
        /// <param name="other">The <see cref="PropertiesToken"/> to check.</param>
        /// <returns>true if they are equal; false otherwise.</returns>
        public bool Equals(PropertiesToken? other)
        {
            return other.HasValue && Equals(other.Value);
        }

        /// <summary>
        /// Checks if these <see cref="PropertiesToken"/>s are equal.
        /// </summary>
        /// <param name="other">The <see cref="PropertiesToken"/> to check.</param>
        /// <returns>true if they are equal; false otherwise.</returns>
        public bool Equals(in PropertiesToken other)
        {
            return Type == other.Type && Value == other.Value;
        }

        /// <summary>
        /// Checks if these <see cref="PropertiesToken"/>s are equal.
        /// </summary>
        /// <param name="token">The first token.</param>
        /// <param name="other">The <see cref="PropertiesToken"/> to check.</param>
        /// <returns>true if they are equal; false otherwise.</returns>
        public static bool operator ==(PropertiesToken? token, PropertiesToken? other)
        {
            if (!token.HasValue && !other.HasValue)
                return true;

            if (!token.HasValue || !other.HasValue)
                return false;

            return token!.Value.Equals(other!);
        }

        /// <summary>
        /// Checks if these <see cref="PropertiesToken"/>s are not equal.
        /// </summary>
        /// <param name="token">The first token.</param>
        /// <param name="other">The <see cref="PropertiesToken"/> to check.</param>
        /// <returns>true if they are not equal; false otherwise.</returns>
        public static bool operator !=(PropertiesToken? token, PropertiesToken? other)
        {
            return !(token == other);
        }

        /// <summary>
        /// Returns this token as a string.
        /// </summary>
        /// <returns>The this token as a string.</returns>
        public override string ToString()
        {
            return $"Type: {Type}, Value: {Value}";
        }

        /// <summary>
        /// Returns the hash code for this token.
        /// </summary>
        /// <returns>The hash code for this token.</returns>
        public override int GetHashCode() => HashCodeHelper.GenerateHashCode(Type, Value);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is PropertiesToken token && Equals(token);
    }
}
