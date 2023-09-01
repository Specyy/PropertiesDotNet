using System;
using System.Collections.Generic;

using PropertiesDotNet.Serialization.Converters;
using PropertiesDotNet.Serialization.ObjectProviders;
using PropertiesDotNet.Serialization.PropertiesTree;
using PropertiesDotNet.Serialization.ValueProviders;

namespace PropertiesDotNet.Serialization
{
    /// <summary>
    /// Represents a class that provides settings for a <see cref="PropertiesSerializer"/>.
    /// </summary>
    public class PropertiesSerializerSettings
    {
        private IObjectProvider _objectProvider;

        /// <summary>
        /// Gets or sets the <see cref="IObjectProvider"/> used to create instances of objects
        /// in this <see cref="PropertiesSerializer"/>.
        /// </summary>
        public virtual IObjectProvider ObjectProvider
        {
            get => _objectProvider;
            set => _objectProvider = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IValueProvider _valueProvider;

        /// <summary>
        /// Gets or sets the <see cref="ValueProvider"/> used to create instances of objects
        /// in this <see cref="PropertiesSerializer"/>.
        /// </summary>
        public virtual IValueProvider ValueProvider
        {
            get => _valueProvider;
            set => _valueProvider = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IPropertiesTreeComposer _treeComposer;

        /// <summary>
        /// Gets or sets the <see cref="IPropertiesTreeComposer"/> to be used to create the object tree
        /// to be serialized or deserialized.
        /// </summary>
        public virtual IPropertiesTreeComposer TreeComposer
        {
            get => _treeComposer;
            set => _treeComposer = value ?? throw new ArgumentNullException(nameof(value));
        }

        private LinkedList<IPropertiesConverter> _converters;

        /// <summary>
        /// Gets or sets the <see cref="IPropertiesConverter"/>s to use for the serialization and deserialization
        /// of .NET objects into and from ".properties" objects.
        /// </summary>
        public virtual LinkedList<IPropertiesConverter> Converters
        {
            get => _converters;
            set => _converters = value ?? throw new ArgumentNullException(nameof(value));
        }

        private LinkedList<IPropertiesPrimitiveConverter> _primitiveConverters;

        /// <summary>
        /// Gets or sets the <see cref="IPropertiesPrimitiveConverter"/>s to use for the serialization and deserialization
        /// of primitive .NET objects into and from ".properties" primitve values.
        /// </summary>
        public virtual LinkedList<IPropertiesPrimitiveConverter> PrimitiveConverters
        {
            get => _primitiveConverters;
            set => _primitiveConverters = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the default .NET type for ".properties" objects. This default type is utilized
        /// whenever no type is specified within a <see cref="PropertiesSerializer.SerializeObject(Type?, object?, PropertiesObject)"/>
        /// and the <see langword="value"/>'s type cannot be retrieved, or if the type is <see langword="typeof"/>(<see langword="object"/>). 
        /// </summary>
        public virtual Type DefaultObjectType
        {
            get => _defaultObjectType ??= typeof(Dictionary<string, object>);
            set => _defaultObjectType = value ?? typeof(Dictionary<string, object>);
        }

        private Type _defaultObjectType;

        /// <summary>
        /// Gets or sets the default .NET type for ".properties" primitive values.
        /// </summary>
        public virtual Type DefaultPrimitiveType
        {
            get => _defaultPrimitiveType ??= typeof(string);
            set => _defaultPrimitiveType = value ?? typeof(string);
        }

        private Type _defaultPrimitiveType;

        /// <summary>
        /// Creates a new blank <see cref="PropertiesSerializerSettings"/>.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public PropertiesSerializerSettings()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }
    }
}