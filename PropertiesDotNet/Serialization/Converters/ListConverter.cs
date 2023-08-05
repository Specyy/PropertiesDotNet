using System;
using System.Collections.Generic;

using PropertiesDotNet.Serialization.PropertiesTree;
using PropertiesDotNet.Utils;

namespace PropertiesDotNet.Serialization.Converters
{
    public class ListConverter : IPropertiesConverter
    {
        /// <inheritdoc/>
        public virtual bool Accepts(Type type)
        {
            if (type.IsAbstract() || type.IsInterface())
                return false;

            return TypeExtensions.GetGenericInterface(type, typeof(IList<>)) is null;
        }

        /// <inheritdoc/>
        public virtual object? Deserialize(PropertiesSerializer serializer, Type type, PropertiesObject tree)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual void Serialize(PropertiesSerializer serializer, Type type, object? value, PropertiesObject tree)
        {
            throw new NotImplementedException();
        }
    }
}
