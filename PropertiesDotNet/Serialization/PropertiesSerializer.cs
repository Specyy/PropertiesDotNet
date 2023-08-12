﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using PropertiesDotNet.Core;
using PropertiesDotNet.Serialization.Converters;
using PropertiesDotNet.Serialization.ObjectProviders;
using PropertiesDotNet.Serialization.PropertiesTree;
using PropertiesDotNet.Serialization.ValueProviders;
using PropertiesDotNet.Utils;

namespace PropertiesDotNet.Serialization
{
    /// <summary>
    /// Delegate called when a type is (de)serialized.
    /// </summary>
    /// <param name="serializer">The serializer that (de)serialized the type.</param>
    /// <param name="type">The type that the object was (de)serialized as.</param>
    /// <param name="value">The actual value of the (de)serialized object.</param>
    public delegate void TypeSerialized(PropertiesSerializer serializer, Type type, object? value);

    /// <summary>
    /// Represents a serializer and deserializer that transforms .NET objects into ".properties" objects and documents
    /// and vice-versa.
    /// </summary>
    public class PropertiesSerializer
    {
        /// <summary>
        /// The settings for this <see cref="PropertiesSerializer"/>.
        /// </summary>
        public virtual PropertiesSerializerSettings Settings
        {
            get => _settings;
            set => _settings = value ?? throw new ArgumentException(nameof(value));
        }

        private PropertiesSerializerSettings _settings;

        /// <summary>
        /// Event raised when a type serialized inside a document.
        /// </summary>
        public virtual event TypeSerialized? TypeSerialized;

        /// <summary>
        /// Event raised when a type is deserialized from a document.
        /// </summary>
        public virtual event TypeSerialized? TypeDeserialized;

        /// <summary>
        /// A short-hand to the <see cref="PropertiesSerializerSettings.ObjectProvider"/> for this serializer.
        /// </summary>
        public IObjectProvider ObjectProvider
        {
            get => Settings.ObjectProvider;
            set => Settings.ObjectProvider = value;
        }

        /// <summary>
        /// A short-hand to the <see cref="PropertiesSerializerSettings.ValueProvider"/> for this serializer.
        /// </summary>
        public IValueProvider ValueProvider
        {
            get => Settings.ValueProvider;
            set => Settings.ValueProvider = value;
        }

        /// <summary>
        /// A short-hand to the <see cref="PropertiesSerializerSettings.TreeComposer"/> for this serializer.
        /// </summary>
        public IPropertiesTreeComposer TreeComposer
        {
            get => Settings.TreeComposer;
            set => Settings.TreeComposer = value;
        }

        /// <summary>
        /// A short-hand to the <see cref="PropertiesSerializerSettings.Converters"/> for this serializer.
        /// </summary>
        public LinkedList<IPropertiesConverter> Converters
        {
            get => Settings.Converters;
            set => Settings.Converters = value;
        }

        /// <summary>
        /// A short-hand to the <see cref="PropertiesSerializerSettings.PrimitiveConverters"/> for this serializer.
        /// </summary>
        public LinkedList<IPropertiesPrimitiveConverter> PrimitiveConverters
        {
            get => Settings.PrimitiveConverters;
            set => Settings.PrimitiveConverters = value;
        }

        /// <summary>
        /// A short-hand to the <see cref="PropertiesSerializerSettings.DefaultObjectType"/> for this serializer.
        /// </summary>
        public Type DefaultObjectType
        {
            get => Settings.DefaultObjectType;
            set => Settings.DefaultObjectType = value;
        }

        /// <summary>
        /// A short-hand to the <see cref="PropertiesSerializerSettings.DefaultPrimitiveType"/> for this serializer.
        /// </summary>
        public Type DefaultPrimitiveType
        {
            get => Settings.DefaultPrimitiveType;
            set => Settings.DefaultPrimitiveType = value;
        }

        /// <summary>
        /// Creates a new serializer with the given <paramref name="settings"/>, or the default settings if left null.
        /// </summary>
        /// <param name="settings">The settings for this serializer, or the default settings if left null.</param>
        public PropertiesSerializer(PropertiesSerializerSettings? settings = null)
        {
            Settings = settings ?? new PropertiesSerializerSettings()
            {
                ObjectProvider = new ReflectionObjectProvider(),
                ValueProvider = new ReflectionValueProvider(),
                TreeComposer = new PropertiesTreeComposer(),

                Converters = new LinkedList<IPropertiesConverter>(),
                PrimitiveConverters = new LinkedList<IPropertiesPrimitiveConverter>()
            };

            Converters.AddLast(new DictionaryConverter());
            Converters.AddLast(new ArrayConverter());
            Converters.AddLast(new CollectionConverter());
            Converters.AddLast(new ObjectConverter());
            PrimitiveConverters.AddFirst(new SystemTypeConverter());
        }

        /// <summary>
        /// Deserializes a value from the given <paramref name="input"/>.
        /// </summary>
        /// <param name="input">The reader to read the content from.</param>
        /// <param name="type">The type to deserialize the input as. A null value indicates that <see cref="object"/>'s
        /// type will be used.</param>
        public static object? Deserialize(IPropertiesReader input, Type? type = null)
        {
            var serializer = new PropertiesSerializer();
            return serializer.DeserializeObject(input, type);
        }

        /// <summary>
        /// Deserializes the value from the given <paramref name="input"/>.
        /// </summary>
        /// <param name="input">The stream to read from.</param>
        /// <param name="type">The type to deserialize the input as. A null value indicates that <see cref="object"/>'s
        /// type will be used.</param>
        /// <returns>The deserialized instance.</returns>
        public static object? Deserialize(TextReader input, Type? type = null)
        {
            var serializer = new PropertiesSerializer();
            return serializer.DeserializeObject(input, type);
        }

        /// <summary>
        /// Deserializes the value from the given <paramref name="input"/>.
        /// </summary>
        /// <param name="input">The stream to read from.</param>
        /// <param name="type">The type to deserialize the input as. A null value indicates that <see cref="object"/>'s
        /// type will be used.</param>
        /// <returns>The deserialized instance.</returns>
        public static object? Deserialize(string input, Type? type = null)
        {
            var serializer = new PropertiesSerializer();
            return serializer.DeserializeObject(input, type);
        }

        /// <summary>
        /// Deserializes a value from the given <paramref name="input"/>.
        /// </summary>
        /// <param name="input">The reader to read the content from.</param>
        /// <typeparam name="T">The type to deserialize as.</typeparam>
        /// <returns>The deserialized instance.</returns>
        public static T Deserialize<T>(IPropertiesReader input)
        {
            var serializer = new PropertiesSerializer();
            return serializer.DeserializeObject<T>(input);
        }

        /// <summary>
        /// Deserializes the value from the given <paramref name="input"/>.
        /// </summary>
        /// <param name="input">The stream to read from.</param>
        /// <typeparam name="T">The type to deserialize the <paramref name="input"/> as.</typeparam>
        /// <returns>The deserialized instance.</returns>
        public static T Deserialize<T>(TextReader input)
        {
            var serializer = new PropertiesSerializer();
            return serializer.DeserializeObject<T>(input);
        }

        /// <summary>
        /// Deserializes the value from the given <paramref name="input"/>.
        /// </summary>
        /// <param name="input">The stream to read from.</param>
        /// <typeparam name="T">The type to deserialize the <paramref name="input"/> as.</typeparam>
        /// <returns>The deserialized instance.</returns>
        public static T Deserialize<T>(string input)
        {
            var serializer = new PropertiesSerializer();
            return serializer.DeserializeObject<T>(input);
        }

        /// <summary>
        /// Serializes the object value.
        /// </summary>
        /// <param name="output">The writer to output the value.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="type">The type to serialize the value as. A null value indicates that the <paramref name="value"/>'s
        /// type will be used.</param>
        public static void Serialize(IPropertiesWriter output, object? value, Type? type = null)
        {
            var serializer = new PropertiesSerializer();
            serializer.SerializeObject(output, value, type);
        }

        /// <summary>
        /// Serializes the object value.
        /// </summary>
        /// <param name="output">The stream to output to.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="type">The type to serialize the value as. A null value indicates that the <paramref name="value"/>'s
        /// type will be used.</param>
        public static void Serialize(TextWriter output, object? value, Type? type = null)
        {
            using var writer = new PropertiesWriter(output);
            Serialize(writer, value, type);
        }

        /// <summary>
        /// Serializes the object value.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="type">The type to serialize the value as. A null value indicates that the <paramref name="value"/>'s
        /// type will be used.</param>
        public static void Serialize(StringBuilder output, object? value, Type? type = null)
        {
            using var writer = new PropertiesWriter(output);
            Serialize(writer, value, type);
        }

        /// <summary>
        /// Serializes the object value.
        /// </summary>
        /// <param name="output">The stream to output to.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="type">The type to serialize the value as. A null value indicates that the <paramref name="value"/>'s
        /// type will be used.</param>
        public static void Serialize(Stream output, object? value, Type? type = null)
        {
            using var writer = new PropertiesWriter(output);
            Serialize(writer, value, type);
        }

        /// <summary>
        /// Serializes the object value.
        /// </summary>
        /// <param name="output">The writer to output the value.</param>
        /// <param name="value">The value to serialize.</param>
        /// <typeparam name="T">The type to serialize the value as. A null value indicates that the <paramref name="value"/>'s
        /// type will be used.</typeparam>
        public static void Serialize<T>(IPropertiesWriter output, object? value)
        {
            Serialize(output, value, typeof(T));
        }

        /// <summary>
        /// Serializes the object value.
        /// </summary>
        /// <param name="output">The stream to output to.</param>
        /// <param name="value">The value to serialize.</param>
        /// <typeparam name="T">The type to serialize the value as. A null value indicates that the <paramref name="value"/>'s
        /// type will be used.</typeparam>
        public static void Serialize<T>(TextWriter output, object? value)
        {
            Serialize(output, value, typeof(T));
        }

        /// <summary>
        /// Serializes the object value.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="value">The value to serialize.</param>
        /// <typeparam name="T">The type to serialize the value as. A null value indicates that the <paramref name="value"/>'s
        /// type will be used.</typeparam>
        public static void Serialize<T>(StringBuilder output, object? value)
        {
            Serialize(output, value, typeof(T));
        }

        /// <summary>
        /// Serializes the object value.
        /// </summary>
        /// <param name="output">The stream to output to.</param>
        /// <param name="value">The value to serialize.</param>
        /// <typeparam name="T">The type to serialize the value as. A null value indicates that the <paramref name="value"/>'s
        /// type will be used.</typeparam>
        public static void Serialize<T>(Stream output, T value)
        {
            Serialize(output, value, typeof(T));
        }

        /// <summary>
        /// Deserializes a value from the given <paramref name="input"/>.
        /// </summary>
        /// <param name="input">The reader to read the content from.</param>
        /// <param name="type">The type to deserialize the input as. A null value indicates that <see cref="object"/>'s
        /// type will be used.</param>
        public object? DeserializeObject(IPropertiesReader input, Type? type = null)
        {
            return DeserializeObject(type, TreeComposer.ReadObject(input));
        }

        /// <summary>
        /// Deserializes the value from the given <paramref name="input"/>.
        /// </summary>
        /// <param name="input">The stream to read from.</param>
        /// <param name="type">The type to deserialize the input as. A null value indicates that <see cref="object"/>'s
        /// type will be used.</param>
        /// <returns>The deserialized instance.</returns>
        public virtual object? DeserializeObject(TextReader input, Type? type = null)
        {
            using var reader = new PropertiesReader(input);
            return DeserializeObject(reader, type);
        }

        /// <summary>
        /// Deserializes the value from the given <paramref name="input"/>.
        /// </summary>
        /// <param name="input">The stream to read from.</param>
        /// <param name="type">The type to deserialize the input as. A null value indicates that <see cref="object"/>'s
        /// type will be used.</param>
        /// <returns>The deserialized instance.</returns>
        public virtual object? DeserializeObject(string input, Type? type = null)
        {
            using var reader = new PropertiesReader(new StringReader(input));
            return DeserializeObject(reader, type);
        }

        /// <summary>
        /// Deserializes a value from the given <paramref name="input"/>.
        /// </summary>
        /// <param name="input">The reader to read the content from.</param>
        /// <typeparam name="T">The type to deserialize the <paramref name="input"/> as.</typeparam>
        /// <returns>The deserialized instance.</returns>
        public virtual T DeserializeObject<T>(IPropertiesReader input)
        {
            return (T)DeserializeObject(input, typeof(T))!;
        }

        /// <summary>
        /// Deserializes the value from the given <paramref name="input"/>.
        /// </summary>
        /// <param name="input">The stream to read from.</param>
        /// <typeparam name="T">The type to deserialize the <paramref name="input"/> as.</typeparam>
        /// <returns>The deserialized instance.</returns>
        public virtual T DeserializeObject<T>(TextReader input)
        {
            using var reader = new PropertiesReader(input);
            return DeserializeObject<T>(reader);
        }

        /// <summary>
        /// Deserializes the value from the given <paramref name="input"/>.
        /// </summary>
        /// <param name="input">The stream to read from.</param>
        /// <returns>The deserialized instance.</returns>
        public virtual T DeserializeObject<T>(string input)
        {
            using var reader = new PropertiesReader(input);
            return DeserializeObject<T>(reader);
        }

        /// <summary>
        /// Serializes the object value.
        /// </summary>
        /// <param name="output">The writer to output the value.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="type">The type to serialize the value as. A null value indicates that the <paramref name="value"/>'s
        /// type will be used.</param>
        public void SerializeObject(IPropertiesWriter output, object? value, Type? type = null)
        {
            TreeComposer.WriteObject(SerializeObject(type, value), output);
        }

        /// <summary>
        /// Serializes the object value.
        /// </summary>
        /// <param name="output">The stream to output to.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="type">The type to serialize the value as. A null value indicates that the <paramref name="value"/>'s
        /// type will be used.</param>
        public virtual void SerializeObject(TextWriter output, object? value, Type? type = null)
        {
            using var writer = new PropertiesWriter(output);
            SerializeObject(writer, value, type);
        }

        /// <summary>
        /// Serializes the object value.
        /// </summary>
        /// <param name="output">The stream to output to.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="type">The type to serialize the value as. A null value indicates that the <paramref name="value"/>'s
        /// type will be used.</param>
        public virtual void SerializeObject(StringBuilder output, object? value, Type? type = null)
        {
            using var writer = new PropertiesWriter(output);
            SerializeObject(writer, value, type);
        }

        /// <summary>
        /// Serializes the object value.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="type">The type to serialize the value as. A null value indicates that the <paramref name="value"/>'s
        /// type will be used.</param>
        /// <returns>The serialized data, as a string.</returns>
        public virtual string SerializeObject(object? value, Type? type = null)
        {
            StringBuilder output = new StringBuilder();
            SerializeObject(output, value, type);
            return output.ToString();
        }

        /// <summary>
        /// Deserializes the ".properties" object tree as the given .NET <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to deserialize the tree as.</param>
        /// <param name="root">The root of the object tree.</param>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="PropertiesException">If a deserialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could deserialize the tree as the <paramref name="type"/>.</exception>
        public object? DeserializeObject(Type? type, PropertiesObject root)
        {
            // Default type
            if (type is null || type == typeof(object))
                type = DefaultObjectType;

            try
            {
                foreach (var converter in Converters)
                {
                    if (converter.Accepts(type))
                    {
                        object? value = TypeExtensions.ConvertType(converter.Deserialize(this, type, root), type, ObjectProvider);
                        TypeDeserialized?.Invoke(this, type, value);
                        return value;
                    }
                }
            }
            catch (PropertiesException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PropertiesException($"Could not deserialize type: {type.FullName}", ex);
            }

            throw new PropertiesException($"No {nameof(IPropertiesConverter)} compatible with type {type.FullName}");
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/> as the given <paramref name="type"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="type">The type to serialize the <paramref name="value"/> as.</param>
        /// <param name="value">The value to serialize</param>
        /// <returns>The root of the object tree.</returns>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/> as the <paramref name="type"/>.</exception>
        public PropertiesObject SerializeObject(Type? type, object? value)
        {
            var root = TreeComposer.CreateRoot();
            SerializeObject(type, value, root);
            return root;
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/> as the given <paramref name="type"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="type">The type to serialize the <paramref name="value"/> as.</param>
        /// <param name="value">The value to serialize</param>
        /// <param name="root">The root object to serialize into.</param>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/> as the <paramref name="type"/>.</exception>
        public void SerializeObject(Type? type, object? value, PropertiesObject root)
        {
            type ??= value?.GetType() ?? DefaultObjectType;

            try
            {
                foreach (var converter in Converters)
                {
                    if (converter.Accepts(type))
                    {
                        converter.Serialize(this, type, value, root);
                        TypeSerialized?.Invoke(this, type, value);
                        return;
                    }
                }
            }
            catch (PropertiesException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PropertiesException($"Could not serialize type: {type.FullName}", ex);
            }

            throw new PropertiesException($"No {nameof(IPropertiesConverter)} compatible with type {type.FullName}");
        }

        /// <summary>
        /// Returns whether an <see cref="IPropertiesPrimitiveConverter"/> is registered for the given type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>true if an <see cref="IPropertiesPrimitiveConverter"/> is registered for the given type; false otherwise.</returns>
        public virtual bool IsPrimitive(Type type)
        {
            foreach (var primitiveConverter in Settings.PrimitiveConverters)
                if (primitiveConverter.Accepts(type))
                    return true;

            return false;
        }

        /// <summary>
        /// Deserializes the primitive <paramref name="value"/> as the given <typeparamref name="T"/>.
        /// </summary>
        /// <param name="value">The primitive value to deserialize.</param>
        /// <typeparam name="T">The type to deserialize the <paramref name="value"/> as.</typeparam>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="ArgumentNullException">If both the <paramref name="value"/> and <typeparamref name="T"/> are null.</exception>
        /// <exception cref="PropertiesException">If a deserialization exception occurs or if no 
        /// <see cref="IPropertiesPrimitiveConverter"/> could deserialize the primitive <paramref name="value"/> as the <typeparamref name="T"/>.</exception>
        public T DeserializePrimitive<T>(string? value)
        {
            return (T)DeserializePrimitive(typeof(T), value);
        }

        /// <summary>
        /// Deserializes the primitive <paramref name="value"/> as the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to deserialize the <paramref name="value"/> as.</param>
        /// <param name="value">The primitive value to deserialize.</param>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="ArgumentNullException">If both the <paramref name="value"/> and <paramref name="type"/> are null.</exception>
        /// <exception cref="PropertiesException">If a deserialization exception occurs or if no 
        /// <see cref="IPropertiesPrimitiveConverter"/> could deserialize the primitive <paramref name="value"/> as the <paramref name="type"/>.</exception>
        public object? DeserializePrimitive(Type? type, string? value)
        {
            if (type is null || type == typeof(object))
                type = DefaultPrimitiveType;

            try
            {
                foreach (var converter in PrimitiveConverters)
                {
                    if (converter.Accepts(type))
                    {
                        object? primitive = TypeExtensions.ConvertType(converter.Deserialize(this, type, value), type, ObjectProvider);
                        TypeDeserialized?.Invoke(this, type, primitive);
                        return primitive;
                    }
                }
            }
            catch (PropertiesException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PropertiesException($"Could not deserialize primitive type: {type.FullName} (\"{value}\")", ex);
            }

            throw new PropertiesException($"No {nameof(IPropertiesPrimitiveConverter)} compatible with type {type.FullName}");
        }

        /// <summary>
        /// Serializes the primitive <paramref name="value"/> as the given <typeparamref name="T"/>.
        /// </summary>
        /// <param name="value">The primitive value to serialize.</param>
        /// <typeparam name="T">The type to serialize the <paramref name="value"/> as.</typeparam>
        /// <returns>The serialized value.</returns>
        /// <exception cref="ArgumentNullException">If both the <paramref name="value"/> and <typeparamref name="T"/> are null.</exception>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesPrimitiveConverter"/> could serialize the primitive <paramref name="value"/> as the <typeparamref name="T"/>.</exception>
        public string? SerializePrimitive<T>(T value) => SerializePrimitive(typeof(T), (T)value);

        /// <summary>
        /// Serializes the primitive <paramref name="value"/> as the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to serialize the <paramref name="value"/> as.</param>
        /// <param name="value">The primitive value to serialize.</param>
        /// <returns>The serialized value.</returns>
        /// <exception cref="ArgumentNullException">If both the <paramref name="value"/> and <paramref name="type"/> are null.</exception>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesPrimitiveConverter"/> could serialize the primitive <paramref name="value"/> as the <paramref name="type"/>.</exception>
        public string? SerializePrimitive(Type? type, object? value)
        {
            if (type is null || type == typeof(object))
                type = value?.GetType() ?? DefaultPrimitiveType;

            try
            {
                foreach (var converter in PrimitiveConverters)
                {
                    if (converter.Accepts(type))
                    {
                        string? primitive = converter.Serialize(this, type, value);
                        TypeSerialized?.Invoke(this, type, primitive);
                        return primitive;
                    }
                }
            }
            catch (PropertiesException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PropertiesException($"Could not serialize primitive type: {type.FullName}", ex);
            }

            throw new PropertiesException($"No {nameof(IPropertiesPrimitiveConverter)} compatible with type {type.FullName}");
        }
    }
}