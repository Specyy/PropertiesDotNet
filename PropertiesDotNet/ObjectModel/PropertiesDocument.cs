using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using PropertiesDotNet.Core;
using PropertiesDotNet.Utils;

namespace PropertiesDotNet.ObjectModel
{
    /// <summary>
    /// Represents a ".properties" document.
    /// </summary>
    public class PropertiesDocument : IEnumerable<PropertiesProperty>, IEnumerable
    {
        private readonly OrderedDictionary<string, PropertiesProperty> _properties;

        /// <summary>
        /// Returns the number of properties within this document.
        /// </summary>
        public virtual int Count => _properties.Count;

        /// <summary>
        /// Creates a new <see cref="PropertiesDocument"/>.
        /// </summary>
        public PropertiesDocument()
        {
            _properties = new OrderedDictionary<string, PropertiesProperty>();
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesDocument"/>.
        /// </summary>
        /// <param name="properties">The properties to include in this document.</param>
        public PropertiesDocument(params KeyValuePair<string, string?>[] properties) : this()
        {
            for (var i = 0; i < properties.Length; i++)
                AddProperty(properties[i]);
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesDocument"/>.
        /// </summary>
        /// <param name="properties">The properties to include in this document.</param>
        public PropertiesDocument(IDictionary<string, string?> properties) : this()
        {
            foreach (var property in properties)
                AddProperty(property);
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesDocument"/>.
        /// </summary>
        /// <param name="properties">The properties to include in this document.</param>
        public PropertiesDocument(IEnumerable<PropertiesProperty> properties) : this()
        {
            foreach (PropertiesProperty property in properties)
                AddProperty(property);
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesDocument"/> from the given data.
        /// </summary>
        /// <param name="stream">The stream containing document data.</param>
        /// <param name="override">Whether to override duplicate properties.</param>
        public PropertiesDocument(Stream stream, bool @override = true) : this()
        {
            LoadDocument(stream, @override);
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesDocument"/> from the given data.
        /// </summary>
        /// <param name="reader">The reader containing document data.</param>
        /// <param name="override">Whether to override duplicate properties.</param>
        public PropertiesDocument(TextReader reader, bool @override = true) : this()
        {
            LoadDocument(reader, @override);
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesDocument"/> from the given data.
        /// </summary>
        /// <param name="reader">The document data.</param>
        /// <param name="override">Whether to override duplicate properties.</param>
        public PropertiesDocument(IPropertiesReader reader, bool @override = true) : this()
        {
            LoadDocument(reader, @override);
        }

        /// <summary>
        /// Loads this document from the given reader.
        /// </summary>
        /// <param name="reader">The reader to load from.</param>
        /// <param name="override">Whether to override duplicate properties.</param>
        public static PropertiesDocument Load(IPropertiesReader reader, bool @override = true)
        {
            return new PropertiesDocument(reader, @override);
        }

        /// <summary>
        /// Loads this document from the given reader.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <param name="override">Whether to override duplicate properties.</param>
        public static PropertiesDocument Load(Stream stream, bool @override = true)
        {
            using var pReader = new PropertiesReader(stream);
            return Load(pReader, @override);
        }

        /// <summary>
        /// Loads this document from the given reader.
        /// </summary>
        /// <param name="reader">The reader to load from.</param>
        /// <param name="override">Whether to override duplicate properties.</param>
        public static PropertiesDocument Load(TextReader reader, bool @override = true)
        {
            using var pReader = new PropertiesReader(reader);
            return Load(pReader, @override);
        }

        /// <summary>
        /// Loads this document from the given text document.
        /// </summary>
        /// <param name="document">The text document to load.</param>
        /// <param name="override">Whether to override duplicate properties.</param>
        public static PropertiesDocument Load(string document, bool @override = true)
        {
            using var reader = new PropertiesReader(document);
            return Load(reader, @override);
        }

        /// <summary>
        /// Loads this document from the given text document.
        /// </summary>
        /// <param name="document">The text document to load.</param>
        /// <param name="override">Whether to override existing properties.</param>
        public virtual void LoadDocument(string document, bool @override = true)
        {
            using var reader = new PropertiesReader(document);
            LoadDocument(reader, @override);
        }

        /// <summary>
        /// Loads this document from the given reader.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <param name="override">Whether to override existing properties.</param>
        public virtual void LoadDocument(Stream stream, bool @override = true)
        {
            using var pReader = new PropertiesReader(stream);
            LoadDocument(pReader, @override);
        }

        /// <summary>
        /// Loads this document from the given reader.
        /// </summary>
        /// <param name="reader">The reader to load from.</param>
        /// <param name="override">Whether to override existing properties.</param>
        public virtual void LoadDocument(TextReader reader, bool @override = true)
        {
            using var pReader = new PropertiesReader(reader);
            LoadDocument(pReader, @override);
        }

        /// <summary>
        /// Loads this document from the given reader.
        /// </summary>
        /// <param name="reader">The reader to load from.</param>
        /// <param name="override">Whether to override existing properties.</param>
        public virtual void LoadDocument(IPropertiesReader reader, bool @override = true)
        {
            while (reader.MoveNext())
            {
                var token = reader.Token;

                switch (token.Type)
                {
                    case PropertiesTokenType.Key:
                        string key = token.Text!;

                        char assigner = '=';

                        if (reader.MoveNext() && (token = reader.Token).Type == PropertiesTokenType.Assigner)
                        {
                            assigner = token.Text[0];
                            reader.MoveNext();
                        }

                        if ((token = reader.Token).Type != PropertiesTokenType.Value)
                            throw new PropertiesException($"Missing value for key \"{key}\"!");

                        if (@override)
                            SetProperty(new PropertiesProperty(key, assigner, token.Text));
                        else
                            AddProperty(new PropertiesProperty(key, assigner, token.Text));
                        break;

                    case PropertiesTokenType.Error:
                        throw new PropertiesException(token.Text);

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Clears all the properties from this document.
        /// </summary>
        public virtual void Clear()
        {
            _properties.Clear();
        }

        /// <summary>
        /// Adds the specified property to this document, if it does not already contain a property with the same key.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        public virtual void AddProperty(string key, string? value)
        {
            AddProperty(new PropertiesProperty(key, value));
        }

        /// <summary>
        /// Adds the specified property to this document, if it does not already contain a property with the same key.
        /// </summary>
        /// <param name="property">The property to add.</param>
        public virtual void AddProperty(PropertiesProperty property)
        {
            _properties.Add(property.Key, property);
        }

        /// <summary>
        /// Sets the specified property within this document; overrides if a property with the same key already exists.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        public virtual void SetProperty(string key, string? value)
        {
            SetProperty(new PropertiesProperty(key, value));
        }

        /// <summary>
        /// Sets the specified property within this document; overrides if a property with the same key already exists.
        /// </summary>
        /// <param name="property">The property to set.</param>
        public virtual void SetProperty(PropertiesProperty property)
        {
            _properties[property.Key] = property;
        }

        /// <summary>
        /// Gets the specified property, if it exists inside this document.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>The specified property, if it exists.</returns>
        /// <exception cref="KeyNotFoundException">The property key is not found.</exception>
        public virtual PropertiesProperty? GetProperty(string key)
        {
            return TryGetProperty(key, out var prop) ? prop :
                throw new KeyNotFoundException($"Could not find key \"{key}\" within properties document!");
        }

        /// <summary>
        /// Gets the specified property value, if it exists inside this document.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>The specified property's value, if it exists.</returns>
        /// <exception cref="KeyNotFoundException">The property key is not found.</exception>
        public virtual string? GetValue(string key)
        {
            return TryGetValue(key, out var val) ? val :
                throw new KeyNotFoundException($"Could not find key \"{key}\" within properties document!");
        }

        /// <summary>
        /// Gets the specified property value as a <see langword="bool"/>, if it exists inside this document.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>The specified property's value, if it exists.</returns>
        /// <exception cref="KeyNotFoundException">The property key is not found.</exception>
        public virtual bool GetBool(string key)
        {
            return bool.Parse(GetValue(key));
        }

        /// <summary>
        /// Gets the specified property value as a <see langword="byte"/>, if it exists inside this document.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>The specified property's value, if it exists.</returns>
        /// <exception cref="KeyNotFoundException">The property key is not found.</exception>
        public virtual byte GetByte(string key)
        {
            return byte.Parse(GetValue(key));
        }

        /// <summary>
        /// Gets the specified property value as a <see langword="sbyte"/>, if it exists inside this document.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>The specified property's value, if it exists.</returns>
        /// <exception cref="KeyNotFoundException">The property key is not found.</exception>
        public virtual sbyte GetSByte(string key)
        {
            return sbyte.Parse(GetValue(key));
        }

        /// <summary>
        /// Gets the specified property value as a <see langword="short"/>, if it exists inside this document.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>The specified property's value, if it exists.</returns>
        /// <exception cref="KeyNotFoundException">The property key is not found.</exception>
        public virtual short GetInt16(string key)
        {
            return short.Parse(GetValue(key));
        }

        /// <summary>
        /// Gets the specified property value as a <see langword="ushort"/>, if it exists inside this document.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>The specified property's value, if it exists.</returns>
        /// <exception cref="KeyNotFoundException">The property key is not found.</exception>
        public virtual ushort GetUInt16(string key)
        {
            return ushort.Parse(GetValue(key));
        }

        /// <summary>
        /// Gets the specified property value as a <see langword="int"/>, if it exists inside this document.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>The specified property's value, if it exists.</returns>
        /// <exception cref="KeyNotFoundException">The property key is not found.</exception>
        public virtual int GetInt32(string key)
        {
            return int.Parse(GetValue(key));
        }

        /// <summary>
        /// Gets the specified property value as a <see langword="uint"/>, if it exists inside this document.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>The specified property's value, if it exists.</returns>
        /// <exception cref="KeyNotFoundException">The property key is not found.</exception>
        public virtual uint GetUInt32(string key)
        {
            return uint.Parse(GetValue(key));
        }

        /// <summary>
        /// Gets the specified property value as a <see langword="long"/>, if it exists inside this document.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>The specified property's value, if it exists.</returns>
        /// <exception cref="KeyNotFoundException">The property key is not found.</exception>
        public virtual long GetInt64(string key)
        {
            return long.Parse(GetValue(key));
        }

        /// <summary>
        /// Gets the specified property value as a <see langword="ulong"/>, if it exists inside this document.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>The specified property's value, if it exists.</returns>
        /// <exception cref="KeyNotFoundException">The property key is not found.</exception>
        public virtual ulong GetUInt64(string key)
        {
            return ulong.Parse(GetValue(key));
        }

        /// <summary>
        /// Gets the specified property value as a <see langword="float"/>, if it exists inside this document.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>The specified property's value, if it exists.</returns>
        /// <exception cref="KeyNotFoundException">The property key is not found.</exception>
        public virtual float GetFloat(string key)
        {
            return float.Parse(GetValue(key));
        }

        /// <summary>
        /// Gets the specified property value as a <see langword="double"/>, if it exists inside this document.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>The specified property's value, if it exists.</returns>
        /// <exception cref="KeyNotFoundException">The property key is not found.</exception>
        public virtual double GetDouble(string key)
        {
            return double.Parse(GetValue(key));
        }

        /// <summary>
        /// Gets the specified property value as a <see langword="decimal"/>, if it exists inside this document.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>The specified property's value, if it exists.</returns>
        /// <exception cref="KeyNotFoundException">The property key is not found.</exception>
        public virtual decimal GetDecimal(string key)
        {
            return decimal.Parse(GetValue(key));
        }

        /// <summary>
        /// Attempts to get specified property.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="property">The retrieved property.</param>
        /// <returns>true if the property with the specified <paramref name="key"/> was found; otherwise false.</returns>
        public virtual bool TryGetProperty(string key, [NotNullWhen(true)] out PropertiesProperty? property)
        {
            return _properties.TryGetValue(key, out property);
        }

        /// <summary>
        /// Attempts to get specified property value.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The retrieved value.</param>
        /// <returns>true if the property with the specified <paramref name="key"/> was found; otherwise false.</returns>
        public virtual bool TryGetValue(string key, [MaybeNullWhen(false)] out string? value)
        {
            if (TryGetProperty(key, out var prop))
            {
                value = prop.Value;
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Checks whether this document contains the specified key.
        /// </summary>
        /// <param name="key">The property key to check for.</param>
        /// <returns>Whether this document contains the specified property.</returns>
        public virtual bool ContainsKey(string key) => _properties.ContainsKey(key);

        /// <summary>
        /// Checks whether this document contains the specified property.
        /// </summary>
        /// <param name="property">The property to check for.</param>
        /// <returns>Whether this document contains the specified property.</returns>
        public virtual bool ContainsProperty(PropertiesProperty property) => ContainsKey(property.Key);

        /// <summary>
        /// Gets or sets the specified property.
        /// </summary>
        /// <param name="key">The property key to get.</param>
        /// <returns>The specified property, if it exists.</returns>
        /// <exception cref="KeyNotFoundException">The property key is not found.</exception>
        public virtual string? this[string key]
        {
            get => GetValue(key);
            set => SetProperty(key, value);
        }

        /// <summary>
        /// Gets or sets the specified property.
        /// </summary>
        /// <param name="index">The index of the property to get.</param>
        /// <returns>The specified property, if it exists.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the index is out of range.</exception>
        public virtual PropertiesProperty this[int index]
        {
            get => _properties[index].Value;
            set => _properties[index] = new KeyValuePair<string, PropertiesProperty>(value.Key, value);
        }

        /// <summary>
        /// Saves this document into the file.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="timestamp">Whether to output a timestamp at the begining of the document.</param>
        public virtual void Save(string path, bool timestamp = true)
        {
            using var fileStream = File.OpenWrite(path);
            Save(fileStream);
        }

        /// <summary>
        /// Saves this document into the specified stream.
        /// </summary>
        /// <param name="stream">The stream to save this document into.</param>
        /// <param name="timestamp">Whether to output a timestamp at the begining of the document.</param>
        public virtual void Save(Stream stream, bool timestamp = true)
        {
            using var writer = new PropertiesWriter(stream);
            Save(writer);
        }

        /// <summary>
        /// Saves this document into the specified writer.
        /// </summary>
        /// <param name="writer">The writer to save this document into.</param>
        /// <param name="timestamp">Whether to output a timestamp at the begining of the document.</param>
        public virtual void Save(TextWriter writer, bool timestamp = true)
        {
            using var pWriter = new PropertiesWriter(writer);
            Save(pWriter);
        }

        /// <summary>
        /// Saves this document into the specified writer.
        /// </summary>
        /// <param name="writer">The writer to save this document into.</param>
        /// <param name="timestamp">Whether to output a timestamp at the begining of the document.</param>
        public virtual void Save(IPropertiesWriter writer, bool timestamp = true)
        {
            if (timestamp)
            {
                // TODO: Output timezone abbreviation
                writer.Write(new PropertiesToken(PropertiesTokenType.Comment, DateTime.Now.ToString($"ddd MMM dd HH:mm:ss '{TimeZoneInfo.Local.DisplayName.Substring(1, 9)}' yyyy")));
            }

            for (int i = 0; i < _properties.Count; i++)
            {
                PropertiesProperty prop = _properties[i].Value;

                for (int j = 0; j < prop.Comments?.Count; j++)
                    // TODO: Allow customization of handle
                    writer.Write(new PropertiesToken(PropertiesTokenType.Comment, prop.Comments[j]));

                writer.Write(new PropertiesToken(PropertiesTokenType.Key, prop.Key));
                writer.Write(new PropertiesToken(PropertiesTokenType.Assigner, prop.Assigner.ToString()));
                writer.Write(new PropertiesToken(PropertiesTokenType.Value, prop.Value));
            }

            writer.Flush();
        }

        /// <inheritdoc/>
        public virtual IEnumerator<PropertiesProperty> GetEnumerator() => _properties.Values.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}