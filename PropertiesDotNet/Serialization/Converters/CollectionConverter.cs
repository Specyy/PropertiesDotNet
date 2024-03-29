﻿using System;
using System.Collections;
using System.Collections.Generic;

using PropertiesDotNet.Core;
using PropertiesDotNet.Serialization.ObjectProviders;
using PropertiesDotNet.Serialization.PropertiesTree;
using PropertiesDotNet.Utils;

namespace PropertiesDotNet.Serialization.Converters
{
    /// <summary>
    /// Represents a converter for <see cref="ICollection{T}"/> and <see cref="IList"/> types.
    /// </summary>
    /// <remarks>This implementation is most optimized for collections that implement <see cref="IList"/> or <see cref="IList{T}"/>.
    /// Please attempt to do so for the greatest performance.</remarks>
    public class CollectionConverter : IPropertiesConverter
    {
        /// <summary>
        /// Creates a new <see cref="CollectionConverter"/>.
        /// </summary>
        public CollectionConverter() { }

        /// <inheritdoc/>
        public virtual bool Accepts(Type type)
        {
            if (type.IsAbstract() || type.IsInterface() || type.IsArray)
                return false;

            return !(TypeExtensions.GetGenericInterface(type, typeof(ICollection<>)) is null) || typeof(IList).IsAssignableFrom(type);
        }

        /// <inheritdoc/>
        public virtual object? Deserialize(PropertiesSerializer serializer, Type type, PropertiesObject obj)
        {
            // TODO: Allow well-formatted collections to be deserialized as HashSets
            if (type.GetGenericTypeDefinition() == typeof(HashSet<>))
                throw new NotSupportedException($"Deserialization of {typeof(HashSet<>).FullName} is not supported");

            object? rawValue;
            IList list;
            Type itemType;

            var genericCollectionType = TypeExtensions.GetGenericInterface(type, typeof(ICollection<>));
            if (genericCollectionType is null)
            {
                // Assume typeof(IList)
                itemType = typeof(object);
                list = (IList)(rawValue = serializer.ObjectProvider.Construct(type));
            }
            else
            {
                itemType = genericCollectionType.GetGenericArguments()[0];
                rawValue = serializer.ObjectProvider.Construct(type);
                list = rawValue as IList ?? (IList)serializer.ObjectProvider.Construct(typeof(DynamicGenericList<>).MakeGenericType(itemType), new[] { rawValue });
            }

            Deserialize(serializer, itemType, list, obj);
            return rawValue;
        }

        /// <summary>
        /// Deserializes the <paramref name="list"/> (collection) from the <paramref name="obj"/>.
        /// </summary>
        /// <param name="serializer">The underlying serializer.</param>
        /// <param name="itemType">The list item type.</param>
        /// <param name="list">A new instance of the list.</param>
        /// <param name="obj">The document tree.</param>
        /// <exception cref="PropertiesException">If the list could not be deserialized.</exception>
        protected void Deserialize(PropertiesSerializer serializer, Type itemType, IList list, PropertiesObject obj)
        {
            foreach (var node in obj)
            {
                int index = serializer.DeserializePrimitive<int>(node.Name);
                object? value;

                if (node is PropertiesPrimitive prop)
                {
                    value = serializer.DeserializePrimitive(itemType, prop.Value);
                }
                else if (node is PropertiesObject innerObj)
                {
                    value = serializer.DeserializeObject(itemType, innerObj);
                }
                else throw new PropertiesException($"Cannot deserialize tree node of type \"{node.GetType().FullName}\"!");

                if (index < list.Count)
                {
                    list[index] = value;
                }
                else
                {
                    for (int i = 0, count = list.Count; i < index - count; i++)
                        list.Add(TypeExtensions.ConvertType(null, itemType, serializer.ObjectProvider));

                    list.Add(value);
                }
            }
        }

        /// <inheritdoc/>
        public virtual void Serialize(PropertiesSerializer serializer, Type type, object? value, PropertiesObject tree)
        {
            var genericCollectionType = TypeExtensions.GetGenericInterface(type, typeof(ICollection<>));
            var itemType = genericCollectionType is null ? typeof(object) : genericCollectionType.GetGenericArguments()[0];
            IList list = value as IList ?? (IList)serializer.ObjectProvider.Construct(typeof(DynamicGenericList<>).MakeGenericType(itemType), new[] { value });

            // Prefer for-loop since it is usually faster
            if (value is IList)
            {
                for (int i = 0; i < list.Count; i++)
                    SerializeItem(serializer, itemType, list[i], i, tree);
            }
            // We need to use iterator since not all collections have index accessors
            else
            {
                int i = 0;
                foreach (var item in list)
                    SerializeItem(serializer, itemType, item, i++, tree);
            }
        }

        private void SerializeItem(PropertiesSerializer serializer, Type itemType, object? item, int i, PropertiesObject tree)
        {
            string? index = serializer.SerializePrimitive(i);

            if (serializer.IsPrimitive(item?.GetType() ?? itemType))
            {
                // TODO: if itemType is primitive, serialize into inline array
                tree.AddPrimitive(index, serializer.SerializePrimitive(item?.GetType() ?? itemType, item));
            }
            else
            {
                PropertiesObject itemObj = new PropertiesObject(index);
                serializer.SerializeObject(item?.GetType() ?? itemType, item, itemObj);
                tree.Add(itemObj);
            }
        }
    }
}