﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PropertiesDotNet.Core;
using PropertiesDotNet.Serialization.ObjectProviders;
using PropertiesDotNet.Serialization.PropertiesTree;
using PropertiesDotNet.Utils;

namespace PropertiesDotNet.Serialization.Converters
{
    /// <summary>
    /// Represents a converter for <see cref="ICollection{T}"/> and <see cref="IList"/> types.
    /// </summary>
    public class CollectionConverter : IPropertiesConverter
    {
        /// <inheritdoc/>
        public virtual bool Accepts(Type type)
        {
            if (type.IsAbstract() || type.IsInterface() || type.IsArray)
                return false;

            return !(TypeExtensions.GetGenericInterface(type, typeof(ICollection<>)) is null) || typeof(IList).IsAssignableFrom(type);
        }

        /// <inheritdoc/>
        public virtual object? Deserialize(PropertiesSerializer serializer, Type type, PropertiesObject tree)
        {
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

            Deserialize(serializer, itemType, list, tree);
            return rawValue;
        }

        protected void Deserialize(PropertiesSerializer serializer, Type itemType, IList list, PropertiesObject tree)
        {
            foreach (var node in tree)
            {
                int index = serializer.DeserializePrimitive<int>(node.Name);
                object? value;

                if (node is PropertiesPrimitive prop)
                {
                    value = serializer.DeserializePrimitive(itemType, prop.Value);
                }
                else if (node is PropertiesObject obj)
                {
                    value = serializer.DeserializeObject(itemType, obj);
                }
                else throw new PropertiesException($"Cannot deserialize tree node of type \"{node.GetType().FullName}\"!");

                if (index < list.Count)
                {
                    list[index] = value;
                }
                else
                {
                    int count = list.Count;
                    for (int i = 0; i < index - count; i++)
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

            for (int i = 0; i < list.Count; i++)
            {
                object? item = list[i];
                string? index = serializer.SerializePrimitive(i);

                if (serializer.IsPrimitive(item?.GetType() ?? itemType))
                {
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
}
