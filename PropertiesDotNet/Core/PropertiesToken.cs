using System;
using System.Collections.Generic;
using System.Text;

namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Represents a token in a .properties document.
    /// </summary>
    public readonly struct PropertiesToken
    {
        /// <summary>
        /// The type of this token. A <see cref="PropertiesTokenType.None"/> acts a null value
        /// when returned by an <see cref="IPropertiesReader"/> that has no tokens left to parse.
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
        public readonly bool Canonical => Type != PropertiesTokenType.None && Type != PropertiesTokenType.Error;

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
