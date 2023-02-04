using System;
using System.Runtime;

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
        public readonly string? Text { get; }

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
            Text = value;
        }

        /// <summary>
        /// Checks if these <see cref="PropertiesToken"/>s are equal.
        /// </summary>
        /// <param name="other">The <see cref="PropertiesToken"/> to check.</param>
        /// <returns>true if they are equal; false otherwise.</returns>
        public bool Equals(PropertiesToken other)
        {
            return Type == other.Type && Text == other.Text;
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
            return Type == other.Type && Text == other.Text;
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
            return $"Type: {Type}, Value: {Text}";
        }

        /// <summary>
        /// Returns the hash code for this token.
        /// </summary>
        /// <returns>The hash code for this token.</returns>
        public override int GetHashCode() => HashCodeHelper.GenerateHashCode(Type, Text);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is PropertiesToken token && Equals(token);

        /// <summary>
        /// Creates a new comment token.
        /// </summary>
        /// <param name="value">The text content of the comment.</param>
        /// <returns>A new comment token.</returns>
        public static PropertiesToken Comment(string? value)
        {
            return new PropertiesToken(PropertiesTokenType.Comment, value);
        }

        /// <summary>
        /// Creates a new key token.
        /// </summary>
        /// <param name="key">The text content of the key.</param>
        /// <returns>A new key token.</returns>
        /// <exception cref="ArgumentException">If the key is <see langword="null"/> or empty.</exception>
        public static PropertiesToken Key(string key)
        {
            return new PropertiesToken(PropertiesTokenType.Key, key);
        }

        /// <summary>
        /// Creates a new assigner token.
        /// </summary>
        /// <param name="assigner">The value of the assigner.</param>
        /// <returns>A new assigner token.</returns>
        /// <exception cref="ArgumentException">If the assigner is not '=', ':' or a white-space.</exception>
        public static PropertiesToken Assigner(char? assigner = '=')
        {
            if (assigner != '=' && assigner != ':' && assigner != ' ' && assigner != '\t' && assigner != '\f')
                throw new ArgumentException($"Assigner must be '=', ':' or a white-space!");

            return new PropertiesToken(PropertiesTokenType.Assigner, assigner.ToString());
        }

        /// <summary>
        /// Creates a new value token.
        /// </summary>
        /// <param name="value">The text content of the value.</param>
        /// <returns>A new value token.</returns>
        public static PropertiesToken Value(string? value)
        {
            return new PropertiesToken(PropertiesTokenType.Value, value);
        }
    }
}
