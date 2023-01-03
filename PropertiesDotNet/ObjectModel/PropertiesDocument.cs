using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

using PropertiesDotNet.Core;
using PropertiesDotNet.Core.Events;
using PropertiesDotNet.Utils;

namespace PropertiesDotNet.ObjectModel
{
    /// <summary>
    /// Represents a ".properties" document.
    /// </summary>
    public sealed class PropertiesDocument : IEnumerable<PropertiesProperty>
    {
        /// <summary>
        /// Returns an ordered list of all the properties inside this document.
        /// </summary>
        public IEnumerable<PropertiesProperty> AllProperties => _properties.Values;

        private readonly OrderedDictionary<PropertiesKey, PropertiesProperty> _properties;

        /// <summary>
        /// Creates a new <see cref="PropertiesDocument"/>.
        /// </summary>
        public PropertiesDocument()
        {
            _properties = new OrderedDictionary<PropertiesKey, PropertiesProperty>();
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesDocument"/>.
        /// </summary>
        /// <param name="properties">The properties to include in this document.</param>
        public PropertiesDocument(params KeyValuePair<string, string?>[] properties) : this()
        {
            for (var i = 0; i < properties.Length; i++)
            {
                ref var property = ref properties[i];
                AddProperty(property.Key, property.Value);
            }
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesDocument"/>.
        /// </summary>
        /// <param name="properties">The properties to include in this document.</param>
        public PropertiesDocument(params KeyValuePair<PropertiesKey, PropertiesValue?>[] properties) : this()
        {
            for (var i = 0; i < properties.Length; i++)
            {
                ref var property = ref properties[i];
                AddProperty(property.Key, property.Value);
            }
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesDocument"/>.
        /// </summary>
        /// <param name="properties">The properties to include in this document.</param>
        public PropertiesDocument(IDictionary<string, string?> properties) : this()
        {
            foreach (var property in properties)
                AddProperty(property.Key, property.Value);
        }

        /// <summary>
        /// Creates a new <see cref="PropertiesDocument"/>.
        /// </summary>
        /// <param name="properties">The properties to include in this document.</param>
        public PropertiesDocument(IDictionary<PropertiesKey, PropertiesValue?> properties) : this()
        {
            foreach (var property in properties)
                AddProperty(property.Key, property.Value);
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
        /// <param name="reader">The document data.</param>
        public PropertiesDocument(IPropertiesReader reader) : this()
        {
            Load(reader, false);
        }

        /// <summary>
        /// Loads this document from the given reader.
        /// </summary>
        /// <param name="reader">The reader to load from.</param>
        public static PropertiesDocument Load(IPropertiesReader reader)
        {
            return new PropertiesDocument(reader);
        }

        /// <summary>
        /// Loads this document from the given reader.
        /// </summary>
        /// <param name="reader">The reader to load from.</param>
        public static PropertiesDocument Load(TextReader reader)
        {
            return Load(new PropertiesReader(reader));
        }

        /// <summary>
        /// Loads this document from the given text document.
        /// </summary>
        /// <param name="document">The text document to load.</param>
        public static PropertiesDocument Load(string document)
        {
            return Load(new PropertiesReader(document));
        }

        /// <summary>
        /// Loads this document from the given text document.
        /// </summary>
        /// <param name="document">The text document to load.</param>
        /// <param name="clear">Whether to clear the current document.</param>
        public void Load(string document, bool clear = true)
        {
            Load(new PropertiesReader(document), clear);
        }

        /// <summary>
        /// Loads this document from the given reader.
        /// </summary>
        /// <param name="reader">The reader to load from.</param>
        /// <param name="clear">Whether to clear the current document.</param>
        public void Load(TextReader reader, bool clear = true)
        {
            Load(new PropertiesReader(reader), clear);
        }

        /// <summary>
        /// Loads this document from the given reader.
        /// </summary>
        /// <param name="reader">The reader to load from.</param>
        /// <param name="clear">Whether to clear the current document.</param>
        public void Load(IPropertiesReader reader, bool clear = true)
        {
            // Clear to re-init
            if (clear)
                Clear();

            // Ensure start
            reader.ReadSerialized<DocumentStart>();

            // Read until end
            while (true)
            {
                var next = reader.Peek();

                if (next is DocumentEnd)
                {
                    reader.Read();
                    break;
                }

                if (next is null)
                {
                    throw new PropertiesSerializationException($"Encountered null node while loading document!");
                }
                
                if (next is PropertyStart)
                {
                    AddProperty(new PropertiesProperty(reader));
                }
                else if (next is Error error)
                {
                    throw new PropertiesStreamException(error.Start, error.End, error.Message);
                }
                else
                {
                    // Skip comments and others
                    reader.Read();
                }
            }
        }

        /// <summary>
        /// Clears all the properties from this document.
        /// </summary>
        public void Clear()
        {
            _properties.Clear();
        }

        /// <summary>
        /// Adds the specified property to this document, if it does not already contain a property with the same key.
        /// </summary>
        /// <param name="property">The property to add.</param>
        public void AddProperty(PropertiesProperty property)
        {
            _properties.Add(property.Key, property);
        }

        /// <summary>
        /// Adds the specified property to this document, if it does not already contain a property with the same key.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        public void AddProperty(PropertiesKey key, PropertiesValue? value)
        {
            AddProperty(new PropertiesProperty(key, value));
        }

        /// <summary>
        /// Adds the specified property to this document, if it does not already contain a property with the same key.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        public void AddProperty(string key, PropertiesValue? value)
        {
            AddProperty((PropertiesKey)key, value);
        }

        /// <summary>
        /// Adds the specified property to this document, if it does not already contain a property with the same key.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        public void AddProperty(string key, string? value)
        {
            AddProperty((PropertiesKey)key, (PropertiesValue)value);
        }

        /// <summary>
        /// Sets the specified property to this document; overrides if a property with the same key already exists.
        /// </summary>
        /// <param name="property">The property to set.</param>
        public void SetProperty(PropertiesProperty property)
        {
            _properties[property.Key] = property;
        }

        /// <summary>
        /// Sets the specified property to this document; overrides if a property with the same key already exists.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        public void SetProperty(PropertiesKey key, PropertiesValue? value)
        {
            SetProperty(new PropertiesProperty(key, value));
        }

        /// <summary>
        /// Sets the specified property to this document; overrides if a property with the same key already exists.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        public void SetProperty(string key, PropertiesValue? value)
        {
            SetProperty((PropertiesKey)key, value);
        }

        /// <summary>
        /// Sets the specified property to this document; overrides if a property with the same key already exists.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        public void SetProperty(string key, string? value)
        {
            SetProperty((PropertiesKey)key, (PropertiesValue)value);
        }

        /// <summary>
        /// Gets the specified property, if it exists inside this document.
        /// </summary>
        /// <param name="key">The property key to get.</param>
        /// <returns>The specified property, if it exists.</returns>
        /// <exception cref="KeyNotFoundException">The property key is not found.</exception>
        public PropertiesProperty? GetProperty(string key)
        {
            return GetProperty((PropertiesKey)key);
        }

        /// <summary>
        /// Gets the specified property, if it exists inside this document.
        /// </summary>
        /// <param name="key">The property key to get.</param>
        /// <returns>The specified property, if it exists.</returns>
        /// <exception cref="KeyNotFoundException">The property key is not found.</exception>
        public PropertiesProperty? GetProperty(PropertiesKey key)
        {
            return _properties[key];
        }

        /// <summary>
        /// Attempts to get specified property.
        /// </summary>
        /// <param name="key">The property key to get.</param>
        /// <param name="property">The retrieved property.</param>
        /// <returns>true if the property with the specified <paramref name="key"/> was found; otherwise false.</returns>
        public bool TryGetProperty(string key, [NotNullWhen(true)] out PropertiesProperty? property)
        {
            return TryGetProperty((PropertiesKey)key, out property);
        }

        /// <summary>
        /// Attempts to get specified property.
        /// </summary>
        /// <param name="key">The property key to get.</param>
        /// <param name="property">The retrieved property.</param>
        /// <returns>true if the property with the specified <paramref name="key"/> was found; otherwise false.</returns>
        public bool TryGetProperty(PropertiesKey key, [NotNullWhen(true)] out PropertiesProperty? property)
        {
            return _properties.TryGetValue(key, out property);
        }

        /// <summary>
        /// Checks whether this document contains the specified property.
        /// </summary>
        /// <param name="property">The property to check for.</param>
        /// <returns>Whether this document contains the specified property.</returns>
        public bool ContainsProperty(PropertiesProperty property)
        {
            return TryGetProperty(property.Key, out PropertiesProperty inner) && inner.Value.Equals(property.Value);
        }

        /// <summary>
        /// Checks whether this document contains the specified property.
        /// </summary>
        /// <param name="key">The property key to check for.</param>
        /// <returns>Whether this document contains the specified property.</returns>
        public bool ContainsProperty(string key)
        {
            return ContainsProperty((PropertiesKey)key);
        }

        /// <summary>
        /// Checks whether this document contains the specified property.
        /// </summary>
        /// <param name="key">The property key to check for.</param>
        /// <returns>Whether this document contains the specified property.</returns>
        public bool ContainsProperty(PropertiesKey key)
        {
            return _properties.ContainsKey(key);
        }

        /// <summary>
        /// Checks whether this document contains the specified value.
        /// </summary>
        /// <param name="value">The value to check for.</param>
        /// <returns>Whether this document contains the specified property.</returns>
        public bool ContainsValue(PropertiesValue value)
        {
            return _properties.Values.FirstOrDefault(inner => inner.Value.Equals(value)) != default;
        }

        /// <summary>
        /// Gets or sets the specified property.
        /// </summary>
        /// <param name="key">The property key to get.</param>
        /// <returns>The specified property, if it exists.</returns>
        /// <exception cref="KeyNotFoundException">The property key is not found.</exception>
        public PropertiesProperty? this[string key]
        {
            get => this[(PropertiesKey)key];
            set => this[(PropertiesKey)key] = value;
        }

        /// <summary>
        /// Gets or sets the specified property.
        /// </summary>
        /// <param name="key">The property key to get.</param>
        /// <returns>The specified property, if it exists.</returns>
        /// <exception cref="KeyNotFoundException">The property key is not found.</exception>
        public PropertiesProperty? this[PropertiesKey key]
        {
            get => GetProperty(key);
            set
            {
                if (!key.Equals(value.Key))
                    throw new ArgumentException($"Cannot set property with key \"{value.Value}\" to key \"{key.Value}\"!");

                SetProperty(value);
            }
        }

        /// <summary>
        /// Gets or sets the specified property.
        /// </summary>
        /// <param name="index">The index of the property to get.</param>
        /// <returns>The specified property, if it exists.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the index is out of range.</exception>
        public PropertiesProperty this[int index]
        {
            get => _properties[index].Value;
            set => _properties[index] = new KeyValuePair<PropertiesKey, PropertiesProperty>(value.Key, value);
        }

        /// <summary>
        /// Serializes this document into the specified writer.
        /// </summary>
        /// <param name="writer">The writer to serialize this document into.</param>
        public void Serialize(IPropertiesWriter writer)
        {
            writer.Write(new DocumentStart());

            // TODO: Write header comment with timestamp
            // TODO: Custom serialize method?

            // Write all properties
            for (var i = 0; i < _properties.Count; i++)
            {
                PropertiesProperty prop = _properties[i].Value;
                
                writer.Write(new PropertyStart());
                writer.Write(_properties[i].Value.Key);
                
                if(!(prop.Assigner is null))
                    writer.Write(_properties[i].Value.Assigner);
                
                if(!(prop.Value is null))
                    writer.Write(_properties[i].Value.Value);
                
                writer.Write(new PropertyEnd());
            }

            writer.Write(new DocumentEnd());
        }

        /// <inheritdoc/>
        public IEnumerator<PropertiesProperty> GetEnumerator() => AllProperties.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}