using System;
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
            set => _settings = value ?? throw new ArgumentException($"{nameof(PropertiesSerializerSettings)} cannot be null", nameof(value));
        }

        private PropertiesSerializerSettings _settings;

        /// <summary>
        /// Event raised when a type serialized inside a document.
        /// </summary>
        public event TypeSerialized? TypeSerialized;

        /// <summary>
        /// Event raised when a type is deserialized from a document.
        /// </summary>
        public event TypeSerialized? TypeDeserialized;

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
        /// Creates a new serializer with the default settings
        /// </summary>
        public PropertiesSerializer() : this(null)
        {

        }

        /// <summary>
        /// Creates a new serializer with the given <paramref name="settings"/>, or the default settings if left null.
        /// </summary>
        /// <param name="settings">The settings for this serializer, or the default settings if left null.</param>
        public PropertiesSerializer(PropertiesSerializerSettings? settings)
        {
            _settings = settings ?? new PropertiesSerializerSettings()
            {
                ObjectProvider = new ReflectionObjectProvider(),
                ValueProvider = new ReflectionValueProvider(),
                TreeComposer = new PropertiesTreeComposer(),

                Converters = new LinkedList<IPropertiesConverter>(),
                PrimitiveConverters = new LinkedList<IPropertiesPrimitiveConverter>()
            };

            if (settings is null)
            {
                var nullableConverter = new NullableTypeConverter();

                Converters.AddLast(new DictionaryConverter());
                Converters.AddLast(new ArrayConverter());
                Converters.AddLast(new CollectionConverter());
                Converters.AddLast(nullableConverter);
                Converters.AddLast(new ObjectConverter());

                PrimitiveConverters.AddLast(new SystemTypeConverter());
                PrimitiveConverters.AddLast(nullableConverter);
            }
        }

        /// <summary>
        /// Deserializes the ".properties" object tree as the given .NET <typeparamref name="T"/>.
        /// </summary>
        /// <param name="input">The input document to deserialize.</param>
        /// <typeparam name="T">The type to deserialize the tree as.</typeparam>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="PropertiesException">If a deserialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could deserialize the tree as <typeparamref name="T"/>.</exception>
        public static T Deserialize<T>(IPropertiesReader input) where T : notnull
        {
            return new PropertiesSerializer().DeserializeObject<T>(input);
        }

        /// <summary>
        /// Deserializes the ".properties" object tree as the given .NET <typeparamref name="T"/>.
        /// </summary>
        /// <param name="input">The input document to deserialize.</param>
        /// <typeparam name="T">The type to deserialize the tree as.</typeparam>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="PropertiesException">If a deserialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could deserialize the tree as <typeparamref name="T"/>.</exception>
        public static T Deserialize<T>(TextReader input) where T : notnull
        {
            using var reader = new PropertiesReader(input);
            return new PropertiesSerializer().DeserializeObject<T>(reader);
        }

        /// <summary>
        /// Deserializes the ".properties" object tree as the given .NET <typeparamref name="T"/>.
        /// </summary>
        /// <param name="input">The input document to deserialize.</param>
        /// <typeparam name="T">The type to deserialize the tree as.</typeparam>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="PropertiesException">If a deserialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could deserialize the tree as <typeparamref name="T"/>.</exception>
        public static T Deserialize<T>(Stream input) where T : notnull
        {
            using var reader = new PropertiesReader(input);
            return new PropertiesSerializer().DeserializeObject<T>(reader);
        }

        /// <summary>
        /// Deserializes the ".properties" object tree as the given .NET <typeparamref name="T"/>.
        /// </summary>
        /// <param name="document">The input document to deserialize as a string.</param>
        /// <typeparam name="T">The type to deserialize the tree as.</typeparam>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="PropertiesException">If a deserialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could deserialize the tree as <typeparamref name="T"/>.</exception>
        public static T Deserialize<T>(string document) where T : notnull
        {
            return new PropertiesSerializer().DeserializeObject<T>(document);
        }

        /// <summary>
        /// Deserializes the ".properties" object tree as the given .NET <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to deserialize the tree as.</param>
        /// <param name="input">The input document to deserialize.</param>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="PropertiesException">If a deserialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could deserialize the tree as the <paramref name="type"/>.</exception>
        public static object? Deserialize(Type? type, IPropertiesReader input)
        {
            return new PropertiesSerializer().DeserializeObject(type, input);
        }

        /// <summary>
        /// Deserializes the ".properties" object tree as the given .NET <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to deserialize the tree as.</param>
        /// <param name="input">The input document to deserialize.</param>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="PropertiesException">If a deserialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could deserialize the tree as the <paramref name="type"/>.</exception>
        public static object? Deserialize(Type? type, TextReader input)
        {
            using var reader = new PropertiesReader(input);
            return new PropertiesSerializer().DeserializeObject(type, reader);
        }

        /// <summary>
        /// Deserializes the ".properties" object tree as the given .NET <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to deserialize the tree as.</param>
        /// <param name="document">The input document to deserialize as a string.</param>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="PropertiesException">If a deserialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could deserialize the tree as the <paramref name="type"/>.</exception>
        public static object? Deserialize(Type? type, string document)
        {
            return new PropertiesSerializer().DeserializeObject(type, document);
        }

        /// <summary>
        /// Deserializes the ".properties" object tree as the given .NET <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to deserialize the tree as.</param>
        /// <param name="input">The input document to deserialize.</param>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="PropertiesException">If a deserialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could deserialize the tree as the <paramref name="type"/>.</exception>
        public static object? Deserialize(Type? type, Stream input)
        {
            using var reader = new PropertiesReader(input);
            return new PropertiesSerializer().DeserializeObject(type, reader);
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/>  into a ".properties" object tree.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <returns>The document as a string.</returns>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/>.</exception>
        public static string Serialize(object? value)
        {
            return new PropertiesSerializer().SerializeObject(value);
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/> as the given <paramref name="type"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="type">The type to serialize the <paramref name="value"/> as.</param>
        /// <param name="value">The value to serialize.</param>
        /// <returns>The document as a string.</returns>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/> as the <paramref name="type"/>.</exception>
        public static string Serialize(Type? type, object? value)
        {
            return new PropertiesSerializer().SerializeObject(type, value);
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="output">The writer to serialize into.</param>
        /// <returns>The root of the object tree.</returns>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/>.</exception>
        public static void Serialize(object? value, IPropertiesWriter output)
        {
            new PropertiesSerializer().SerializeObject(value, output);
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="output">The writer to serialize into.</param>
        /// <returns>The root of the object tree.</returns>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/>.</exception>
        public static void Serialize(object? value, TextWriter output)
        {
            new PropertiesSerializer().SerializeObject(value, output);
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="output">The writer to serialize into.</param>
        /// <returns>The root of the object tree.</returns>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/>.</exception>
        public static void Serialize(object? value, Stream output)
        {
            new PropertiesSerializer().SerializeObject(value, output);
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="path">The path of the file to serialize into.</param>
        /// <returns>The root of the object tree.</returns>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/>.</exception>
        public static void Serialize(object? value, string path)
        {
            new PropertiesSerializer().SerializeObject(value, path);
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/> as the given <paramref name="type"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="type">The type to serialize the <paramref name="value"/> as.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="output">The writer to serialize into.</param>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/> as the <paramref name="type"/>.</exception>
        public static void Serialize(Type? type, object? value, IPropertiesWriter output)
        {
            new PropertiesSerializer().SerializeObject(type, value, output);
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/> as the given <paramref name="type"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="type">The type to serialize the <paramref name="value"/> as.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="output">The writer to serialize into.</param>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/> as the <paramref name="type"/>.</exception>
        public static void Serialize(Type? type, object? value, TextWriter output)
        {
            new PropertiesSerializer().SerializeObject(type, value, output);
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/> as the given <paramref name="type"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="type">The type to serialize the <paramref name="value"/> as.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="output">The writer to serialize into.</param>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/> as the <paramref name="type"/>.</exception>
        public static void Serialize(Type? type, object? value, Stream output)
        {
            new PropertiesSerializer().SerializeObject(type, value, output);
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/> as the given <paramref name="type"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="type">The type to serialize the <paramref name="value"/> as.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="path">The path of the file to serialize into.</param>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/> as the <paramref name="type"/>.</exception>
        public static void Serialize(Type? type, object? value, string path)
        {
            new PropertiesSerializer().SerializeObject(type, value, path);
        }

        /// <summary>
        /// Deserializes the ".properties" object tree as the given .NET <typeparamref name="T"/>.
        /// </summary>
        /// <param name="input">The input document to deserialize.</param>
        /// <typeparam name="T">The type to deserialize the tree as.</typeparam>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="PropertiesException">If a deserialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could deserialize the tree as <typeparamref name="T"/>.</exception>
        public T DeserializeObject<T>(IPropertiesReader input) where T : notnull
        {
            return (T)DeserializeObject(typeof(T), input);
        }

        /// <summary>
        /// Deserializes the ".properties" object tree as the given .NET <typeparamref name="T"/>.
        /// </summary>
        /// <param name="input">The input document to deserialize.</param>
        /// <typeparam name="T">The type to deserialize the tree as.</typeparam>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="PropertiesException">If a deserialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could deserialize the tree as <typeparamref name="T"/>.</exception>
        public T DeserializeObject<T>(TextReader input) where T : notnull
        {
            return (T)DeserializeObject(typeof(T), input);
        }

        /// <summary>
        /// Deserializes the ".properties" object tree as the given .NET <typeparamref name="T"/>.
        /// </summary>
        /// <param name="document">The input document to deserialize as a string.</param>
        /// <typeparam name="T">The type to deserialize the tree as.</typeparam>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="PropertiesException">If a deserialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could deserialize the tree as <typeparamref name="T"/>.</exception>
        public T DeserializeObject<T>(string document) where T : notnull
        {
            return (T)DeserializeObject(typeof(T), document);
        }

        /// <summary>
        /// Deserializes the ".properties" object tree as the given .NET <typeparamref name="T"/>.
        /// </summary>
        /// <param name="input">The input document to deserialize.</param>
        /// <typeparam name="T">The type to deserialize the tree as.</typeparam>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="PropertiesException">If a deserialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could deserialize the tree as <typeparamref name="T"/>.</exception>
        public T DeserializeObject<T>(Stream input) where T : notnull
        {
            return (T)DeserializeObject(typeof(T), input);
        }

        /// <summary>
        /// Deserializes the ".properties" object tree as the given .NET <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to deserialize the tree as.</param>
        /// <param name="input">The input document to deserialize.</param>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="PropertiesException">If a deserialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could deserialize the tree as the <paramref name="type"/>.</exception>
        public object? DeserializeObject(Type? type, IPropertiesReader input)
        {
            return DeserializeObject(type, TreeComposer.ReadObject(input));
        }

        /// <summary>
        /// Deserializes the ".properties" object tree as the given .NET <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to deserialize the tree as.</param>
        /// <param name="input">The input document to deserialize.</param>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="PropertiesException">If a deserialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could deserialize the tree as the <paramref name="type"/>.</exception>
        public object? DeserializeObject(Type? type, TextReader input)
        {
            using var reader = new PropertiesReader(input);
            return DeserializeObject(type, reader);
        }

        /// <summary>
        /// Deserializes the ".properties" object tree as the given .NET <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to deserialize the tree as.</param>
        /// <param name="document">The input document to deserialize as a string.</param>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="PropertiesException">If a deserialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could deserialize the tree as the <paramref name="type"/>.</exception>
        public object? DeserializeObject(Type? type, string document)
        {
            using var reader = new PropertiesReader(document);
            return DeserializeObject(type, reader);
        }

        /// <summary>
        /// Deserializes the ".properties" object tree as the given .NET <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to deserialize the tree as.</param>
        /// <param name="input">The input document to deserialize.</param>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="PropertiesException">If a deserialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could deserialize the tree as the <paramref name="type"/>.</exception>
        public object? DeserializeObject(Type? type, Stream input)
        {
            using var reader = new PropertiesReader(input);
            return DeserializeObject(type, reader);
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
        /// Serializes the .NET object <paramref name="value"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="output">The writer to serialize into.</param>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/>.</exception>
        public void SerializeObject(object? value, IPropertiesWriter output)
        {
            SerializeObject(null, value, output);
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="output">The writer to serialize into.</param>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/>.</exception>
        public void SerializeObject(object? value, TextWriter output)
        {
            SerializeObject(null, value, output);
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="output">The writer to serialize into.</param>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/>.</exception>
        public void SerializeObject(object? value, Stream output)
        {
            SerializeObject(null, value, output);
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="path">The path of the file to serialize into.</param>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/>.</exception>
        public void SerializeObject(object? value, string path)
        {
            SerializeObject(null, value, path);
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/> as the given <paramref name="type"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="type">The type to serialize the <paramref name="value"/> as.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="output">The writer to serialize into.</param>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/> as the <paramref name="type"/>.</exception>
        public void SerializeObject(Type? type, object? value, IPropertiesWriter output)
        {
            var root = TreeComposer.CreateRoot();
            SerializeObject(type, value, root);
            TreeComposer.WriteObject(root, output);
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/> as the given <paramref name="type"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="type">The type to serialize the <paramref name="value"/> as.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="output">The writer to serialize into.</param>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/> as the <paramref name="type"/>.</exception>
        public void SerializeObject(Type? type, object? value, TextWriter output)
        {
            using var writer = new PropertiesWriter(output);
            SerializeObject(type, value, writer);
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/> as the given <paramref name="type"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="type">The type to serialize the <paramref name="value"/> as.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="output">The writer to serialize into.</param>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/> as the <paramref name="type"/>.</exception>
        public void SerializeObject(Type? type, object? value, Stream output)
        {
            using var writer = new PropertiesWriter(output);
            SerializeObject(type, value, writer);
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/> as the given <paramref name="type"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="type">The type to serialize the <paramref name="value"/> as.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="path">The path of the file to serialize into.</param>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/> as the <paramref name="type"/>.</exception>
        public void SerializeObject(Type? type, object? value, string path)
        {
            using var writer = new PropertiesWriter(path);
            SerializeObject(type, value, writer);
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <returns>The document as a string.</returns>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/>.</exception>
        public string SerializeObject(object? value)
        {
            return SerializeObject(null, value);
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/> as the given <paramref name="type"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="type">The type to serialize the <paramref name="value"/> as.</param>
        /// <param name="value">The value to serialize.</param>
        /// <returns>The document as a string.</returns>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/> as the <paramref name="type"/>.</exception>
        public virtual string SerializeObject(Type? type, object? value)
        {
            var root = TreeComposer.CreateRoot();
            SerializeObject(type, value, root);

            var doc = new StringBuilder();

            using (var writer = new PropertiesWriter(doc))
                TreeComposer.WriteObject(root, writer);

            return doc.ToString();
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="root">The root object to serialize into.</param>
        /// <returns>The root of the object tree.</returns>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesConverter"/> could serialize the <paramref name="value"/>.</exception>
        public void SerializeObject(object? value, PropertiesObject root)
        {
            SerializeObject(null, value, root);
        }

        /// <summary>
        /// Serializes the .NET object <paramref name="value"/> as the given <paramref name="type"/> into a ".properties" object tree.
        /// </summary>
        /// <param name="type">The type to serialize the <paramref name="value"/> as.</param>
        /// <param name="value">The value to serialize.</param>
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
        /// Deserializes the primitive <paramref name="value"/> as a(n) <typeparamref name="T"/>.
        /// </summary>
        /// <param name="value">The primitive value to deserialize.</param>
        /// <typeparam name="T">The type to deserialize the <paramref name="value"/> as.</typeparam>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="PropertiesException">If a deserialization exception occurs or if no 
        /// <see cref="IPropertiesPrimitiveConverter"/> could deserialize the primitive <paramref name="value"/> as <typeparamref name="T"/>.</exception>
        public T DeserializePrimitive<T>(string? value) where T : notnull
        {
            return (T)DeserializePrimitive(typeof(T), value);
        }

        /// <summary>
        /// Deserializes the primitive <paramref name="value"/> as the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to deserialize the <paramref name="value"/> as.</param>
        /// <param name="value">The primitive value to deserialize.</param>
        /// <returns>The deserialized value.</returns>
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
        /// Serializes the primitive <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The primitive value to serialize.</param>
        /// <returns>The serialized value.</returns>
        /// <exception cref="PropertiesException">If a serialization exception occurs or if no 
        /// <see cref="IPropertiesPrimitiveConverter"/> could serialize the primitive <paramref name="value"/>.</exception>
        public string? SerializePrimitive(object? value)
        {
            return SerializePrimitive(null, value);
        }

        /// <summary>
        /// Serializes the primitive <paramref name="value"/> as the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to serialize the <paramref name="value"/> as.</param>
        /// <param name="value">The primitive value to serialize.</param>
        /// <returns>The serialized value.</returns>
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