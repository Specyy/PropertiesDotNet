﻿using System;

using PropertiesDotNet.Serialization.PropertiesTree;
using PropertiesDotNet.Utils;

namespace PropertiesDotNet.Serialization.Converters
{
    /// <summary>
    /// Represents a converter for array types.
    /// </summary>
    public sealed class ArrayConverter : CollectionConverter
    {
        /// <summary>
        /// Creates a new <see cref="ArrayConverter"/>.
        /// </summary>
        public ArrayConverter() : base() { }

        /// <inheritdoc/>
        public override bool Accepts(Type type)
        {
            if (!type.IsArray)
                return false;

            var itemType = type.GetElementType()!;
            return !itemType.IsAbstract() && !itemType.IsInterface();
        }

        /// <inheritdoc/>
        public override object? Deserialize(PropertiesSerializer serializer, Type type, PropertiesObject obj)
        {
            var itemType = type.GetElementType();
            Array array;
            var arrayList = new ArrayList();

            Deserialize(serializer, itemType, arrayList, obj);
            arrayList.CopyTo(array = Array.CreateInstance(itemType, arrayList.Count), 0);

            return array;
        }

        /// <inheritdoc/>
        public override void Serialize(PropertiesSerializer serializer, Type type, object? value, PropertiesObject obj)
        {
            var array = TypeExtensions.ConvertType<Array>(value, serializer.ObjectProvider);
            var itemType = type.GetElementType()!;

            for (int i = 0; i < array.Length; i++)
            {
                object? item = array.GetValue(i);
                string? index = serializer.SerializePrimitive(i);

                if (serializer.IsPrimitive(item?.GetType() ?? itemType))
                {
                    obj.AddPrimitive(index, serializer.SerializePrimitive(item?.GetType() ?? itemType, item));
                }
                else
                {
                    PropertiesObject itemObj = new PropertiesObject(index);
                    serializer.SerializeObject(item?.GetType() ?? itemType, item, itemObj);
                    obj.Add(itemObj);
                }
            }
        }
    }
}
