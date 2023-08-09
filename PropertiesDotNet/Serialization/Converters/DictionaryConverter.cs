using System;
using System.Collections;
using System.Collections.Generic;

using PropertiesDotNet.Core;
using PropertiesDotNet.Serialization.PropertiesTree;
using PropertiesDotNet.Serialization.ObjectProviders;
using PropertiesDotNet.Utils;

namespace PropertiesDotNet.Serialization.Converters
{
    /// <summary>
    /// Represents a converter for <see cref="IDictionary{TKey, TValue}"/> and <see cref="IDictionary"/> types.
    /// </summary>
    public sealed class DictionaryConverter : IPropertiesConverter
    {
        /// <inheritdoc/>
        public bool Accepts(Type type)
        {
            if (type.IsAbstract() || type.IsInterface())
                return false;

            return !(TypeExtensions.GetGenericInterface(type, typeof(IDictionary<,>)) is null) || typeof(IDictionary).IsAssignableFrom(type);
        }

        /// <inheritdoc/>
        public object? Deserialize(PropertiesSerializer serializer, Type type, PropertiesObject tree)
        {
            GetDictionaryTypes(serializer, type, out Type keyType, out Type valueType);
            object? rawValue = serializer.ObjectProvider.Construct(type);
            IDictionary dictionary = rawValue as IDictionary ?? (IDictionary)serializer.ObjectProvider.Construct(typeof(DynamicGenericDictionary<,>).MakeGenericType(keyType, valueType), new[] { rawValue });

            foreach (var node in tree)
            {
                // Key must be primitive
                object? key = serializer.DeserializePrimitive(keyType, node.Name);
                object? value;

                if (node is PropertiesPrimitive prop)
                {
                    value = serializer.DeserializePrimitive(valueType, prop.Value);
                }
                else if (node is PropertiesObject obj)
                {
                    // TODO: Maybe if value type is string, read the obj as a single string
                    // Check if key and value are both primitive, then do it
                    value = serializer.DeserializeObject(valueType, obj);
                }
                else throw new PropertiesException($"Cannot deserialize tree node of type \"{node.GetType().FullName}\"!");

                dictionary.Add(key, value);
            }

            return dictionary;
        }

        private void GetDictionaryTypes(PropertiesSerializer serializer, Type type, out Type keyType, out Type valueType)
        {
            var dictionaryInterface = TypeExtensions.GetGenericInterface(type, typeof(IDictionary<,>));

            if (dictionaryInterface is null)
            {
                keyType = serializer.DefaultPrimitiveType;
                valueType = serializer.DefaultObjectType;
            }
            else
            {
                keyType = dictionaryInterface.GetGenericArguments()[0];
                valueType = dictionaryInterface.GetGenericArguments()[1];
            }
        }

        /// <inheritdoc/>
        public void Serialize(PropertiesSerializer serializer, Type type, object? value, PropertiesObject tree)
        {
            GetDictionaryTypes(serializer, type, out Type keyType, out Type valueType);
            IDictionary dictionary = value as IDictionary ?? (IDictionary)serializer.ObjectProvider.Construct(typeof(DynamicGenericDictionary<,>).MakeGenericType(keyType, valueType), new[] { value });

            foreach (DictionaryEntry entry in dictionary)
            {
                object? entryKey = entry.Key;
                object? entryValue = entry.Value;
                // Key must be primitive
                string? keyText = serializer.SerializePrimitive(entryKey?.GetType(), entryKey);

                if (serializer.IsPrimitive(entryValue?.GetType()))
                {
                    tree.AddPrimitive(keyText, serializer.SerializePrimitive(entryValue?.GetType(), entryValue));
                }
                else
                {
                    PropertiesObject entryObj = new PropertiesObject(keyText);
                    serializer.SerializeObject(entryValue?.GetType(), entryValue, entryObj);
                    tree.Add(entryObj);
                }
            }
        }
    }
}
