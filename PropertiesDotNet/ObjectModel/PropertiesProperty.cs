using System;
using System.Collections.Generic;
using System.Text;

namespace PropertiesDotNet.ObjectModel
{
    /// <summary>
    /// Represents a property within a ".properties" document.
    /// </summary>
    public class PropertiesProperty : IEquatable<PropertiesProperty>
    {
        /// <summary>
        /// The key for this property. This cannot be <see langword="null"/> or empty.
        /// </summary>
        public virtual string Key { get; protected set; }

        /// <summary>
        /// A list of the comments that will be emitted above this property when it is saved within a stream.
        /// This is <see cref="Nullable{T}"/> in order to save memory.
        /// </summary>
        public virtual List<string>? Comments { get; set; }

        /// <summary>
        /// The value assigner for this property. This must be '=', ':' or any type of white-space.
        /// </summary>
        /// <exception cref="ArgumentException">If the value is not '=', ':' or any type of white-space.</exception>
        public virtual char Assigner
        {
            get => _assigner;
            set
            {
                switch (value)
                {
                    case '=':
                    case ':':
                    case ' ':
                    case '\t':
                    case '\f':
                        _assigner = value;
                        break;

                    default:
                        throw new ArgumentException("Assigner must be '=', ':' or any type of white-space!");
                }
            }
        }

        private char _assigner;

        /// <summary>
        /// The value of this property. This can be <see langword="null"/>.
        /// </summary>
        public virtual string? Value { get; set; }

        /// <summary>
        /// Creates a new properties document property.
        /// </summary>
        /// <param name="key">The key for this property.</param>
        /// <param name="value">The value for this property.</param>
        /// <exception cref="ArgumentException">If the key is <see langword="null"/> empty.</exception>
        public PropertiesProperty(string key, string? value) : this(key, '=', value)
        {

        }

        /// <summary>
        /// Creates a new properties document property.
        /// </summary>
        /// <param name="key">The key for this property.</param>
        /// <param name="assigner">The assigner for this property.</param>
        /// <param name="value">The value for this property.</param>
        /// <exception cref="ArgumentException">If the key is <see langword="null"/> empty, or 
        /// if the assigner is not '=', ':' or any type of white-space. </exception>
        public PropertiesProperty(string key, char assigner, string? value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty!", nameof(key));

            Key = key;
            Assigner = assigner;
            Value = value;
        }

        /// <summary>
        /// Creates a duplicate properties document property.
        /// </summary>
        /// <param name="property">The property to copy.</param>
        public PropertiesProperty(PropertiesProperty property)
        {
            Comments = property.Comments;
            Key = property.Key;
            Assigner = property.Assigner;
            Value = property.Value;
        }

        /// <summary>
        /// Returns this property as it would be written within a ".properties" document.
        /// </summary>
        /// <returns>This property as it would be written within a ".properties" document</returns>
        public override string ToString()
        {
            if (Comments?.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < Comments.Count; i++)
                    sb.Append('#').Append(' ').AppendLine(Comments[i]);

                sb.Append($"{Key}{_assigner}{Value}");
                return sb.ToString();
            }

            return $"{Key}{_assigner}{Value}";
        }

        /// <inheritdoc/>
        public override int GetHashCode() => ToString().GetHashCode();

        /// <summary>
        /// Whether this property has the same key and value as specified.
        /// </summary>
        /// <param name="other">The other </param>
        /// <returns>true if this property has the same key and value as specified; false otherwise.</returns>
        public virtual bool Equals(PropertiesProperty? other)
        {
            if (!Equals(other?.Key, other?.Value))
                return false;

            if (Comments?.Count != other?.Comments?.Count)
                return false;

            for (int i = 0; i < Comments?.Count; i++)
            {
                if (!Comments[i].Equals(other!.Comments[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Whether this property has the same key and value as specified.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <param name="value">The value to check.</param>
        /// <returns>true if this property has the same key and value as specified; false otherwise.</returns>
        public virtual bool Equals(string key, string? value) => Key == key && Value == value;

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is string str)
                return ToString().Equals(str);

            return Equals(obj as PropertiesProperty);
        }

        /// <summary>
        /// Returns this property as it would be written within a ".properties" document.
        /// </summary>
        /// <param name="property">The property.</param>
        public static explicit operator string(PropertiesProperty property) => property.ToString();

        /// <summary>
        /// Returns this property as a key-value pair.
        /// </summary>
        /// <param name="property">The property.</param>
        public static implicit operator KeyValuePair<string, string?>(PropertiesProperty property) => new KeyValuePair<string, string?>(property.Key, property.Value);

        /// <summary>
        /// Transforms this key-value pair into a property.
        /// </summary>
        /// <param name="pair">The key-value pair.</param>
        public static implicit operator PropertiesProperty(KeyValuePair<string, string?> pair) => new PropertiesProperty(pair.Key, pair.Value);
    }
}