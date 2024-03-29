﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PropertiesDotNet.ObjectModel
{
    /// <summary>
    /// Represents a property within a ".properties" document.
    /// </summary>
    public class PropertiesProperty : IEquatable<PropertiesProperty>, IEquatable<KeyValuePair<string, string?>>
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
        /// <remarks>The value may be \0 on a property with an empty value.</remarks>
        /// <exception cref="ArgumentException">If the value is not '=', ':' or any type of white-space.</exception>
        public virtual char? Assigner
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
                    case null:
                        if (Value is null)
                        {
                            _assigner = null;
                            break;
                        }

                        throw new ArgumentException("Assigner must be '=', ':' or any type of white-space");
                }
            }
        }

        private char? _assigner;

        /// <summary>
        /// The value of this property. This can be <see langword="null"/>.
        /// </summary>
        public virtual string? Value
        {
            get => _value;
            set
            {
                if (Assigner is null && value != null)
                    Assigner = '=';

                _value = value;
            }
        }

        private string? _value;

        /// <summary>
        /// Creates a new properties document property.
        /// </summary>
        /// <param name="value">The key-value pair for this property.</param>
        /// <exception cref="ArgumentException">If the key is <see langword="null"/> empty.</exception>
        public PropertiesProperty(KeyValuePair<string, string?> value) : this(value.Key, value.Value)
        {

        }

        /// <summary>
        /// Creates a new properties document property.
        /// </summary>
        /// <param name="key">The key for this property.</param>
        /// <param name="value">The value for this property.</param>
        /// <exception cref="ArgumentException">If the key is <see langword="null"/> empty.</exception>
        public PropertiesProperty(string key, string? value) : this(key, value is null ? null : (char?)'=', value)
        {
            
        }

        /// <summary>
        /// Creates a new properties document property.
        /// </summary>
        /// <param name="key">The key for this property.</param>
        /// <param name="assigner">The assigner for this property.</param>
        /// <param name="value">The value for this property.</param>
        /// <exception cref="ArgumentException">If the key is <see langword="null"/> empty, or 
        /// if the assigner is not '=', ':' or any type of white-space and the value is not null. </exception>
        public PropertiesProperty(string key, char? assigner, string? value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            Key = key;
            Value = value;
            Assigner = assigner;
        }

        /// <summary>
        /// Creates a duplicate properties document property.
        /// </summary>
        /// <param name="property">The property to copy.</param>
        public PropertiesProperty(PropertiesProperty property)
        {
            Comments = property.Comments;
            Key = property.Key;
            Value = property.Value;
            Assigner = property.Assigner;
        }

        /// <summary>
        /// Adds a comment to this property.
        /// </summary>
        /// <param name="comment">The text value of the comment.</param>
        public virtual void AddComment(string comment)
        {
            Comments ??= new List<string>();
            Comments.Add(comment);
        }

        /// <summary>
        /// Returns this property as it would be written within a ".properties" document.
        /// </summary>
        /// <returns>This property as it would be written within a ".properties" document</returns>
        public override string ToString()
        {
            if (Comments?.Count > 0)
            {
                // TODO: Perhaps cache?
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < Comments.Count; i++)
                    sb.Append('#').Append(' ').AppendLine(Comments[i]);

                sb.Append($"{Key}{Assigner}{Value}");
                return sb.ToString();
            }

            return $"{Key}{Assigner}{Value}";
        }

        /// <inheritdoc/>
        public override int GetHashCode() => ToString().GetHashCode();

        /// <summary>
        /// Returns whether this property is equal to the specified property.
        /// </summary>
        /// <param name="other">The other </param>
        /// <returns>true if this property is equal to the specified property; false otherwise.</returns>
        public virtual bool Equals(PropertiesProperty? other)
        {
            if (!Equals(other?.Key, other?.Value))
                return false;

            if (Assigner != other?.Assigner)
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
        /// Returns whether this property has the same key and value as specified.
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

        /// <inheritdoc/>
        public virtual bool Equals(KeyValuePair<string, string?> other) => Equals(other.Key, other.Value);

        /// <summary>
        /// Returns whether the specified properties are equal.
        /// </summary>
        /// <param name="left">The first property.</param>
        /// <param name="right">The second property.</param>
        /// <returns>true if these properties are equal; false otherwise.</returns>
        public static bool operator ==(PropertiesProperty? left, PropertiesProperty? right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;

            return left.Equals(right);
        }

        /// <summary>
        /// Returns whether the specified properties are not equal.
        /// </summary>
        /// <param name="left">The first property.</param>
        /// <param name="right">The second property.</param>
        /// <returns>true if these properties are equal; false otherwise.</returns>
        public static bool operator !=(PropertiesProperty? left, PropertiesProperty? right) => !(left == right);

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