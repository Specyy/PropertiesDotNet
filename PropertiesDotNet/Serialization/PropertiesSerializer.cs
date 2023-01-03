using PropertiesDotNet.Core;
using PropertiesDotNet.Core.Events;
using PropertiesDotNet.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using PropertiesDotNet.Serialization.ObjectProviders;

namespace PropertiesDotNet.Serialization
{
    /// <summary>
    /// Represents the default ".properties" object graph serializer and deserializer.
    /// </summary>
    public sealed class PropertiesSerializer : IPropertiesSerializer
    {
        private IObjectProvider _objectProvider;

        /// <summary>
        /// Gets or sets the <see cref="IObjectProvider"/> used to create instances of objects
        /// in this <see cref="IPropertiesSerializer"/>.
        /// </summary>
        public IObjectProvider ObjectProvider
        {
            get => _objectProvider;
            set => _objectProvider = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc/>
        public event TypeSerialized? TypeSerialized;

        /// <inheritdoc/>
        public event TypeDeserialized? TypeDeserialized;

        /// <summary>
        /// Creates a new <see cref="PropertiesSerializer"/>.
        /// </summary>
        public PropertiesSerializer()
        {
        }

        /// <inheritdoc/>
        public void Serialize(IPropertiesWriter output, object? value, Type? type = null)
        {
            type ??= value?.GetType() ?? typeof(object);

            output.Write(new DocumentStart());

            output.Write(new DocumentEnd());
        }

        /// <inheritdoc/>
        public object? Deserialize(IPropertiesReader input, Type? type = null)
        {
            input.Read<DocumentStart>();

            object? value = DeserializeInput(input, type ?? typeof(object));

            ObjectProvider.Construct<Key>(new object?[] { "key1" });

            input.Read<DocumentEnd>();

            return value;
        }

        private object? DeserializeInput(IPropertiesReader input, Type type)
        {
            try
            {
                // hello."zook.me" = 5
                // hello."zook.me = 5
                // foreach (IEventSerializer deserializer in Settings.EventSerializers)
                // {
                //     if (deserializer.TryDeserialize(this, DeserializeInput, input, type, out object? value))
                //     {
                //         value = TypeExtensions.ConvertType(value, type);
                //
                //         TypeDeserializedEventArgs eventArgs =
                //             new TypeDeserializedEventArgs(deserializer, input, type, value);
                //         TypeDeserialized?.Invoke(this, eventArgs);
                //         return value;
                //     }
                // }
            }
            catch (PropertiesException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PropertiesException(
                    $"An error occured while deserialzing type \"{type.FullName}\": {ex.Message}", ex);
            }

            throw new PropertiesSerializationException($"Could not deserialize type \"{type.FullName}\"!");
        }
    }
}